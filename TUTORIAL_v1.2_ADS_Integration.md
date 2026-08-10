# TUTORIAL Kapitel v1.2 — ADS Integration: Echte SPS-Werte lesen

**Anschluss an:** TUTORIAL_v1.0_TwinCAT_Roadmap.md
**Stand:** Juli 2026
**Umgebung:** Nativer Windows-PC, Visual Studio 2022, .NET 10, Beckhoff.TwinCAT.Ads 7.0.292
**Ziel:** Live-Verbindung zwischen SensorAPI (.NET) und PalettenStation-SPS (TwinCAT) über ADS

> **Meilenstein:** Erste Live-Verbindung zwischen SensorAPI und einer echten TwinCAT-SPS über ADS. Ab hier reden Web-API und Anlagen-Steuerung wirklich miteinander.

---

## Inhaltsverzeichnis

1. [Konzept: Symbolische Adressierung](#1-konzept-symbolische-adressierung)
2. [Der Datentyp-Übersetzer](#2-der-datentyp-übersetzer)
3. [Silent-Bug-Falle: TwinCAT INT ≠ C# int](#3-silent-bug-falle-twincat-int--c-int)
4. [AdsService erweitern](#4-adsservice-erweitern)
5. [Guard Clauses und Exception Handling](#5-guard-clauses-und-exception-handling)
6. [Live-Endpoint bauen](#6-live-endpoint-bauen)
7. [Switch Expression — der C#-Zwilling von CASE OF](#7-switch-expression--der-c-zwilling-von-case-of)
8. [Was du gelernt hast](#8-was-du-gelernt-hast)
9. [Was noch offen ist](#9-was-noch-offen-ist)

---

## 1. Konzept: Symbolische Adressierung

Zwei Wege, in einer SPS auf eine Variable zuzugreifen:

**Alt / Low-Level — über die Speicheradresse:**

```text
%IX0.3  →  "Input-Byte 0, Bit 3"
%MW42   →  "Merker-Wort 42"
```

Klingt technisch, ist aber Steinzeit. Beim kleinsten Umbau der Hardware verschiebt sich alles.

**Modern / Symbolisch — über den Namen:**

```text
MAIN.bNotHaltFrei
GVL_IO.AI_FolienTemperatur
```

Der TwinCAT-Router weiß intern wo diese Variable im Speicher liegt und schickt den Wert. **Man muss sich nie eine Adresse merken.** Das ist der ganze Sinn von ADS.

**Analogie:** Das ist der Unterschied zwischen *"wohnt in Berlin, Straße Nr. 12, Haus 3, Wohnung 47"* (Adresse) und *"wohnt bei Familie Müller"* (Symbol). Beides funktioniert — aber wenn Familie Müller umzieht, findest du sie über den Namen weiter, über die Adresse nicht.

---

## 2. Der Datentyp-Übersetzer

TwinCAT (IEC 61131-3) und C# reden verschiedene Sprachen. Der `AdsClient` übersetzt — aber nur wenn man ihm sagt *was* man erwartet.

| TwinCAT | Größe | C# Typ | Wertebereich |
|---------|-------|--------|--------------|
| `BOOL` | 1 Bit | `bool` | true/false |
| `BYTE` | 8 Bit | `byte` | 0..255 |
| `SINT` | 8 Bit | `sbyte` | -128..127 |
| `INT` | 16 Bit | **`short`** | -32.768..32.767 |
| `UINT` | 16 Bit | `ushort` | 0..65.535 |
| `DINT` | 32 Bit | **`int`** | ±2 Milliarden |
| `UDINT` | 32 Bit | `uint` | 0..4 Milliarden |
| `LINT` | 64 Bit | `long` | ±9,2 Trillionen |
| `REAL` | 32 Bit | **`float`** | Fließkomma einfach |
| `LREAL` | 64 Bit | **`double`** | Fließkomma doppelt |
| `STRING` | variabel | `string` | Text |
| `TIME` | 32 Bit | `TimeSpan` | Zeitdauer |

**Falscher Typ = falscher Wert.** Wenn eine SPS-`INT` als C#-`int` gelesen wird (statt `short`), kommt Müll raus — weil C# 4 Bytes einliest wo nur 2 stehen.

---

## 3. Silent-Bug-Falle: TwinCAT `INT` ≠ C# `int`

Das ist die häufigste Falle. Namen sehen gleich aus, Größe ist verschieden:

| Name | TwinCAT (IEC 61131) | C# / .NET |
|------|---------------------|-----------|
| `INT` | **16 Bit** (±32.767) | *existiert nicht als Keyword* |
| `int` | *existiert nicht* | **32 Bit** (±2 Milliarden) |
| `DINT` | **32 Bit** (±2 Milliarden) | *existiert nicht* |
| `short` | *existiert nicht* | **16 Bit** (±32.767) |

### Warum kommt Müll raus?

Angenommen `GVL_IO.Schritt` ist in TwinCAT ein `INT` mit dem Wert **5**. Im Speicher liegen genau **2 Bytes**:

```text
Speicheradresse:  ...0x8000  0x8001  0x8002  0x8003  0x8004...
Inhalt:           |  0x05  |  0x00  |  0x??  |  0x??  |  0x??  |
                  └───────────────┘
                    INT = 5
```

Beim Aufruf `ReadValue(..., typeof(int))` wird der Client instruiert:

> *"Lies mir 4 Bytes ab dieser Adresse und interpretier sie als 32-Bit-Zahl"*

Er liest **4 Bytes** — die 2 Bytes der Variable UND 2 Bytes von irgendwas dahinter (nächste Variable, Padding, wer weiß was):

```text
Gelesen: 0x05, 0x00, 0x??, 0x??
```

Diese 4 Bytes werden als 32-Bit-Zahl interpretiert. Wenn dahinter zufällig Nullen stehen, kommt zufällig `5` raus und der Fehler wird nicht bemerkt. Wenn dahinter was anderes steht, kommt eine völlig random große Zahl raus. **Klassischer Silent Bug** — läuft manchmal, manchmal nicht.

### Merksatz

> *"C# `int` ist TwinCAT `DINT`, weil beide 32 Bit haben."*

Der Buchstabe **D** vor INT ist der Hinweis auf **D**oppelte Breite.

---

## 4. AdsService erweitern

Drei generische Read-Methoden für die drei häufigsten Datentypen — jede mit Guard Clause und Exception Handling.

### AdsService.cs — Read-Methoden

```csharp
// Liest eine BOOL-Variable aus der SPS
public bool ReadBool(string symbolPath)
{
    if (!IsConnected) return false;
    try
    {
        return (bool)_client.ReadValue(symbolPath, typeof(bool));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ReadBool({symbolPath}) fehlgeschlagen: {ex.Message}");
        return false;
    }
}

// Liest eine REAL-Variable (32 Bit Float) aus der SPS
public float ReadReal(string symbolPath)
{
    if (!IsConnected) return 0f;
    try
    {
        return (float)_client.ReadValue(symbolPath, typeof(float));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ReadReal({symbolPath}) fehlgeschlagen: {ex.Message}");
        return 0f;
    }
}

// Liest eine INT-Variable (16 Bit) aus der SPS
public short ReadInt(string symbolPath)
{
    if (!IsConnected) return 0;
    try
    {
        return (short)_client.ReadValue(symbolPath, typeof(short));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ReadInt({symbolPath}) fehlgeschlagen: {ex.Message}");
        return 0;
    }
}
```

### Zeilen-Analyse: `_client.ReadValue(symbolPath, typeof(bool))`

- **`_client`** — der `AdsClient`, hat schon die Verbindung zum CP6606
- **`.ReadValue(...)`** — die generische "lies mir einen Wert"-Methode
- **`symbolPath`** — was gelesen werden soll (z.B. `"MAIN.bNotHaltFrei"`)
- **`typeof(bool)`** — als welcher Typ interpretiert werden soll

Intern schickt der `AdsClient` eine Anfrage über TCP an den CP6606:

> *"Router, gib mir den Wert der Variable die 'MAIN.bNotHaltFrei' heißt, ich erwarte 1 Byte (BOOL)"*

Der Router auf dem CP6606 schaut in seiner Symbol-Tabelle, liest das Byte, schickt es zurück. `AdsClient` verpackt es als `bool` und gibt's zurück.

### Der Cast `(bool)`

`ReadValue()` gibt intern `object` zurück (weil es alle Typen kann). Der Cast `(bool)` sagt dem Compiler explizit: *"Ich weiß, das ist ein bool, mach ein bool draus."*

---

## 5. Guard Clauses und Exception Handling

Zwei defensive Patterns die produktions-tauglichen Code auszeichnen.

### Guard Clause: `if (!IsConnected) return false;`

Sicherheitsgurt. Wenn die ADS-Verbindung tot ist, sofort raus mit einem sicheren Default — sonst würde `_client.ReadValue` eine Exception werfen. Das nennt man **Early Return** oder **Guard Clause**: ein sauberes Pattern, macht den Rest der Methode einfacher weil man nicht mehr in einem großen `if`-Block sitzt.

### Try/Catch: Warum das kein Nice-to-Have ist

Wenn irgendwas schiefgeht — Netzwerkausfall, Variable heißt anders, SPS geht in Stopp — fängt `catch` den Fehler ab. Er wird geloggt (in Visual Studio Console) und ein sicherer Default zurückgegeben. Ohne `try/catch` würde ein einziger toter ADS-Read die ganze API abschießen.

**Das ist Real-World-Produktions-Denken:** Automatisierungs-Software muss mit Ausfällen umgehen können, nicht mit ihnen sterben.

| Pattern | Bedeutung |
|---------|-----------|
| Guard Clause | Frühzeitig prüfen, sicher rausgehen bevor was schiefgeht |
| Try/Catch | Fehler abfangen und graceful degradieren |
| Sicherer Default | `false`, `0`, `""` — nicht null, kein Crash |
| Logging | Console.WriteLine für Sichtbarkeit im Debug |

---

## 6. Live-Endpoint bauen

Der Endpoint liest vier SPS-Variablen und gibt sie als JSON zurück.

### SensorController.cs — GET api/sensor/live

```csharp
// GET api/sensor/live — Live-Werte aus der PalettenStation-SPS
[HttpGet("live")]
public ActionResult GetLive()
{
    var schritt = _ads.ReadInt("GVL_IO.Schritt");
    var folienTemp = _ads.ReadReal("GVL_IO.AI_FolienTemperatur");
    var motorTemp = _ads.ReadReal("GVL_IO.AI_MotorTemperatur");
    var alarm = _ads.ReadBool("GVL_IO.AlarmAktiv");

    return Ok(new
    {
        Connected = _ads.IsConnected,
        Schritt = schritt,
        SchrittText = SchrittZuText(schritt),
        FolienTemperatur = Math.Round(folienTemp, 1),
        MotorTemperatur = Math.Round(motorTemp, 1),
        AlarmAktiv = alarm,
        Zeitstempel = DateTime.Now.ToString("HH:mm:ss")
    });
}

private string SchrittZuText(short schritt)
{
    return schritt switch
    {
        0 => "S0 - Bereit, wartet auf Palette",
        1 => "S1 - Foerderband transportiert",
        2 => "S2 - Palette stoppen",
        3 => "S3 - Hubwerk faehrt hoch",
        4 => "S4 - Temperatur pruefen",
        5 => "S5 - Bearbeitung laeuft",
        6 => "S6 - Hubwerk faehrt runter",
        7 => "S7 - Palette verlaesst Station",
        8 => "S8 - ALARM gesetzt",
        9 => "S9 - Warten auf Quittierung",
        _ => $"Unbekannter Schritt: {schritt}"
    };
}
```

### Anonymer Typ: `new { ... }` ohne Klassennamen

Das `new { ... }` ohne Klassennamen ist ein **anonymer Typ** — eine On-the-fly-Klasse. Braucht keine Deklaration, kein `class LiveData { ... }`. ASP.NET wandelt das automatisch in JSON. Praktisch für Endpoints wo die Struktur nur an dieser einen Stelle vorkommt.

### `Math.Round(folienTemp, 1)`

Auf 1 Nachkommastelle runden. Grund: `float` hat Fließkomma-Ungenauigkeit — man will nicht `174.83729553` im Dashboard sehen sondern `174.8`.

---

## 7. Switch Expression — der C#-Zwilling von CASE OF

Der Teil `schritt switch { ... }` ist eine **Switch Expression** (C# 8+). Nicht zu verwechseln mit der klassischen `switch`-Anweisung.

### Alt: switch statement

```csharp
string text;
switch (schritt)
{
    case 0:
        text = "Bereit";
        break;
    case 1:
        text = "Transport";
        break;
    default:
        text = "Unbekannt";
        break;
}
```

### Neu: switch expression

```csharp
string text = schritt switch
{
    0 => "Bereit",
    1 => "Transport",
    _ => "Unbekannt"
};
```

Beides tut dasselbe, aber die Expression ist **ein Ausdruck der einen Wert liefert** — kann direkt einer Variable zugewiesen oder returned werden. Kürzer, keine `break`s, kein Vergessen von `break`.

| Element | Bedeutung |
|---------|-----------|
| `switch { ... }` | Nach der Variable (nicht davor) |
| `=>` | Ergibt (wie bei Lambdas) |
| `_` | Wildcard — entspricht `default:` |
| `,` | Fälle mit Komma trennen, kein `break` nötig |

### Bezug zu TwinCAT

Das entspricht genau `CASE ... OF ... ELSE ... END_CASE` in Structured Text:

```iecst
CASE Schritt OF
    0: text := 'Bereit';
    1: text := 'Transport';
    ELSE text := 'Unbekannt';
END_CASE
```

C# hat das Muster von den funktionalen Sprachen (F#, Haskell) übernommen und in C# 8 als Expression eingebaut.

---

## 8. Was du gelernt hast

Nach v1.2 kannst du im Interview sagen:

> *"Werte aus einer SPS in eine .NET-Anwendung lese ich über symbolische Adressierung mit der Beckhoff ADS-Bibliothek. Der AmsRouter auf dem Zielsystem stellt die Variablen namentlich zur Verfügung, der Client greift per `ReadValue()` drauf zu. Die Typ-Konvertierung SPS↔.NET erfolgt automatisch, wenn der übergebene Typ zum SPS-Datentyp passt — wobei zu beachten ist dass TwinCAT `INT` 16 Bit hat, C# `int` aber 32."*

Vier fundamentale Konzepte sind jetzt fest verdrahtet:

1. **Symbolische Adressierung** — Variablen über Namen statt Speicheradressen
2. **Typ-Mapping SPS ↔ .NET** — mit besonderem Blick auf die Silent-Bug-Falle bei INT/int
3. **Guard Clauses und Exception Handling** — produktions-tauglicher Code der nicht bei jedem Netzwerk-Hicks abraucht
4. **Switch Expressions** — moderne C# Syntax mit direktem Bezug zur SPS-Welt

---

## 9. Was noch offen ist

Für kommende Versionen:

- [ ] **Bulk Read** — statt 4 einzelner ADS-Requests einen Roundtrip mit `SumReader`
- [ ] **Write-Endpoint** — Werte aus dem Dashboard IN die SPS zurückschreiben (z.B. Alarm quittieren, Schritt zurücksetzen)
- [ ] **Symbol-Handle-Caching** — schnellere Reads durch vorab aufgelöste Handles
- [ ] **ADS Notifications** — Push-Benachrichtigungen statt Polling (SPS meldet Änderungen aktiv)
- [ ] **Dashboard-Integration** — Live-Werte in der bestehenden `index.html` anzeigen
- [x] **WCF-Vergleich** — warum ADS in dieser Art ähnlich funktioniert wie ein WCF-Service (Vorbereitung auf WITRON-Stack) → **erledigt in v1.3**, siehe `TUTORIAL_v1.3_CoreWCF_SOAP.md`. Derselbe `AdsService` wurde dabei unverändert von SensorAPI (REST) auf `WcfDemo` (SOAP/CoreWCF) übertragen — die ADS-Anbindung selbst ist völlig unabhängig davon, ob die Werte per REST oder SOAP nach außen gegeben werden.

---

> *"Ein Physiker versteht was in den Bytes passiert, nicht welchen Button er drücken soll. Genau deswegen ist Silent-Bug-Erkennung so viel wichtiger als Copy-Paste-Skills."*
> — Jackie, Juli 2026