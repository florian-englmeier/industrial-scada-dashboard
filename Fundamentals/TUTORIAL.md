# C# Tutorial — Grundlagen

> **Autor:** Flo Englmeier (bavarian-dataforge)  
> **Stand:** v0.4.0 | Juni 2026  
> **Umgebung:** Windows 11 (Parallels), Visual Studio 2022, .NET 10.0

---

## Inhaltsverzeichnis

1. [Setup & Umgebung](#1-setup--umgebung)
2. [v0.1 — Hello World: Grundlagen](#2-v01--hello-world-grundlagen)
3. [v0.2 — List und foreach](#3-v02--list-und-foreach)
4. [v0.3 — Vererbung, override, Alarm-System](#4-v03--vererbung-override-alarm-system)
5. [v0.4 — Interface und LINQ](#5-v04--interface-und-linq)
6. [Git Workflow](#6-git-workflow)
7. [Spickzettel: C# Syntax-Referenz](#7-spickzettel-c-syntax-referenz)
8. [Typische Fehler & Lösungen](#8-typische-fehler--lösungen)

---

## 1. Setup & Umgebung

### .NET 10 SDK installieren
- Download: https://dot.net/download
- Nach Installation: `dotnet --version` → `10.0.300`
- PATH-Problem auf Parallels: dotnet.exe liegt in `C:\Program Files\dotnet\x64`
- Fix: `setx PATH "%PATH%;C:\Program Files\dotnet\x64"`
- WICHTIG: Eingabeaufforderung nach PATH-Änderung neu öffnen!

### Visual Studio 2022 Community
- Download: https://visualstudio.microsoft.com/de/downloads/
- Workloads anhaken:
  - **ASP.NET und Webentwicklung**
  - **.NET-Desktopentwicklung**
- Startprojekt festlegen: Projektmappen-Explorer → Rechtsklick auf Projekt → "Als Startprojekt festlegen"

### Git auf Windows
- Download: https://git-scm.com/download/win
- Alle Standardeinstellungen übernehmen
- Nach Installation: `git --version`
- Parallels-Netzlaufwerk Fix: `git config --global safe.directory *`

---

## 2. v0.1 — Hello World: Grundlagen

### Neues Projekt erstellen
Visual Studio → Neues Projekt → Konsolenanwendung → C# → .NET 10.0

### Erstes Programm
```csharp
Console.WriteLine("Hello, World!");
```
- `Console.WriteLine()` = Ausgabe auf der Konsole
- Jede Anweisung endet mit `;` (Semikolon)
- **F5** zum Starten

### Klasse mit Properties und Konstruktor
```csharp
class Sensor
{
    // Properties — öffentliche Eigenschaften
    public string Name { get; set; }
    public string Einheit { get; set; }
    public double Messwert { get; private set; }

    // Privates Feld — nur innerhalb der Klasse sichtbar
    private Random _random = new Random();

    // Konstruktor — wird beim Erstellen aufgerufen
    public Sensor(string name, string einheit)
    {
        Name = name;
        Einheit = einheit;
    }

    // Methode — erzeugt einen Zufallswert
    public void Messen()
    {
        Messwert = Math.Round(_random.NextDouble() * 100, 2);
    }

    // Methode — gibt den Messwert aus
    public void Anzeigen()
    {
        Console.WriteLine($"{Name}: {Messwert} {Einheit}");
    }
}
```

### Wichtige Konzepte v0.1
| Konzept | Erklärung |
|---------|-----------|
| `class` | Bauplan für Objekte |
| `public` / `private` | Sichtbarkeit: öffentlich vs. nur innerhalb der Klasse |
| `get; set;` | Auto-Property: Getter und Setter automatisch |
| `new` | Neues Objekt erstellen: `new Sensor("Temp", "°C")` |
| `$"..."` | Interpolated String: Variablen direkt mit `{variable}` einfügen |
| `;` | Jede Anweisung endet mit Semikolon |

### Reihenfolge in C#
**WICHTIG:** In modernem C# (Top-Level Statements) muss das Hauptprogramm OBEN stehen, Klassen UNTEN. Fehler CS8803 = Reihenfolge falsch.

---

## 3. v0.2 — List und foreach

### List statt einzelne Variablen
```csharp
// Statt:
Sensor temperatur = new Sensor("Temperatur", "Grad C");
Sensor druck = new Sensor("Druck", "bar");

// Besser — typisierte Liste:
List<Sensor> sensoren = new List<Sensor>
{
    new Sensor("Temperatur", "Grad C"),
    new Sensor("Druck", "bar"),
    new Sensor("Feuchtigkeit", "%")
};
```
- `List<Sensor>` = Liste die nur Sensor-Objekte enthält (typsicher)
- Initialisierung mit `{ }` direkt bei der Deklaration

### foreach-Schleife
```csharp
foreach (Sensor s in sensoren)
{
    s.Messen();
    s.Anzeigen();
}
```
- `foreach` iteriert über alle Elemente der Liste
- `s` ist die Laufvariable — könnte auch `sensor` oder `x` heißen
- Vorteil: Neuer Sensor = eine Zeile in der Liste, Schleife bleibt gleich

### Vergleich for vs. foreach
```csharp
// for — wenn du den Index brauchst:
for (int i = 0; i < 5; i++)
{
    Console.WriteLine($"Zyklus {i + 1}");
}

// foreach — wenn du über eine Sammlung iterierst:
foreach (Sensor s in sensoren)
{
    s.Messen();
}
```

---

## 4. v0.3 — Vererbung, override, Alarm-System

### Erweiterte Basisklasse mit Min/Max
```csharp
class Sensor
{
    public string Name { get; set; }
    public string Einheit { get; set; }
    public double Messwert { get; protected set; }  // protected!
    public double Min { get; set; }
    public double Max { get; set; }

    private Random _random = new Random();

    public Sensor(string name, string einheit, double min, double max)
    {
        Name = name;
        Einheit = einheit;
        Min = min;
        Max = max;
    }

    // virtual = "darf von Kindklassen überschrieben werden"
    public virtual void Messen()
    {
        double spielraum = (Max - Min) * 0.1;
        Messwert = Math.Round(
            _random.NextDouble() * (Max - Min + 2 * spielraum) + Min - spielraum, 2);
    }

    public bool IstAlarm()
    {
        return Messwert < Min || Messwert > Max;
    }

    // ToString — wird automatisch bei Console.WriteLine(objekt) aufgerufen
    public override string ToString()
    {
        return $"  {Name,-15} {Messwert,8:F2} {Einheit}";
    }
}
```

### Wichtige Schlüsselwörter

| Schlüsselwort | Bedeutung |
|---------------|-----------|
| `protected set` | Kindklassen dürfen den Wert setzen, von außen nicht |
| `virtual` | Methode DARF von Kindklassen überschrieben werden |
| `override` | Kindklasse ÜBERSCHREIBT die Methode der Elternklasse |
| `base()` | Ruft den Konstruktor der Elternklasse auf |
| `base.Messen()` | Ruft die Messen-Methode der Elternklasse auf |
| `bool` | Datentyp: true oder false |
| `||` | Logisches ODER |

### Kindklassen (Vererbung)
```csharp
// TemperaturSensor erbt von Sensor
class TemperaturSensor : Sensor
{
    // Konstruktor — ruft Eltern-Konstruktor mit base() auf
    public TemperaturSensor(string name)
        : base(name, "Grad C", -20, 120)
    {
    }

    // override — eigene Mess-Logik
    public override void Messen()
    {
        base.Messen();  // erst normal messen
        Messwert = Math.Round(Messwert);  // dann auf ganze Grad runden
    }
}

class DruckSensor : Sensor
{
    public DruckSensor(string name)
        : base(name, "bar", 0, 10)
    {
    }

    public override void Messen()
    {
        base.Messen();
        Messwert = Math.Round(Messwert, 3);  // 3 Nachkommastellen
    }
}
```

### Polymorphie in Aktion
```csharp
// Eine Liste, verschiedene Typen — alle werden gleich behandelt
List<Sensor> sensoren = new List<Sensor>
{
    new TemperaturSensor("Ofen-1"),      // Kindklasse
    new DruckSensor("Leitung-A"),        // Kindklasse
    new Sensor("Feuchtigkeit", "%", 0, 100)  // Basisklasse
};

// foreach behandelt alle gleich — jeder ruft SEINE Messen()-Version auf
foreach (Sensor s in sensoren)
{
    s.Messen();      // TemperaturSensor rundet auf ganze Grad
    Console.WriteLine(s);  // ruft ToString() auf
}
```

### Alarm-System mit Farbausgabe
```csharp
Console.WriteLine("=== Alarm-Pruefung ===");
foreach (Sensor s in sensoren)
{
    if (s.IstAlarm())
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ALARM: {s.Name} = {s.Messwert} {s.Einheit}");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"OK: {s.Name} = {s.Messwert} {s.Einheit}");
        Console.ResetColor();
    }
}
```

---

## 5. v0.4 — Interface und LINQ

### Interface
```csharp
// Interface — ein "Vertrag": wer ISensor implementiert, MUSS diese Member haben
interface ISensor
{
    string Name { get; set; }
    string Einheit { get; set; }
    double Messwert { get; }
    void Messen();
    bool IstAlarm();
}

// Sensor implementiert das Interface
class Sensor : ISensor
{
    // ... alles bleibt gleich
}
```
- `I` vor dem Namen = C#-Konvention für Interfaces
- Compiler prüft: hat die Klasse alle geforderten Methoden?
- Bei großen Teams: Interfaces sorgen dafür dass alle Sensoren gleich funktionieren

### LINQ — Daten abfragen und filtern
```csharp
// Durchschnitt aller Messwerte
double durchschnitt = sensoren.Average(s => s.Messwert);

// Höchster und niedrigster Wert
double maxWert = sensoren.Max(s => s.Messwert);
double minWert = sensoren.Min(s => s.Messwert);

// Nur Sensoren im Alarm filtern
var alarme = sensoren.Where(s => s.IstAlarm()).ToList();
Console.WriteLine($"Sensoren im Alarm: {alarme.Count}");
```

### Lambda-Expressions
```csharp
// s => s.Messwert  bedeutet:
// "Nimm s, gib s.Messwert zurück"

s => s.Messwert          // Wert zurückgeben
s => s.Messwert > 50     // Bedingung prüfen (true/false)
s => s.IstAlarm()        // Methode aufrufen
s => s.Name              // Property zurückgeben
```
- `=>` = Lambda-Operator ("Fat Arrow")
- Links: Input-Parameter
- Rechts: was damit passiert
- NICHT verwechseln mit `=` (Zuweisung)!

### LINQ Methoden-Übersicht
| Methode | Was sie tut | Beispiel |
|---------|-------------|---------|
| `.Average()` | Durchschnitt | `sensoren.Average(s => s.Messwert)` |
| `.Max()` | Höchster Wert | `sensoren.Max(s => s.Messwert)` |
| `.Min()` | Niedrigster Wert | `sensoren.Min(s => s.Messwert)` |
| `.Where()` | Filtern | `sensoren.Where(s => s.IstAlarm())` |
| `.ToList()` | Ergebnis als Liste | `.Where(...).ToList()` |
| `.Count` | Anzahl Elemente | `alarme.Count` |

### var — Automatische Typ-Erkennung
```csharp
var alarme = sensoren.Where(s => s.IstAlarm()).ToList();
// var wird automatisch zu List<Sensor>
// Spart Tipparbeit bei langen Typnamen
```

---

## 6. Git Workflow

### Grundbefehle
```bash
git init                          # Repository initialisieren
git add .                         # Alle Änderungen vormerken
git commit -m "feat: Beschreibung"  # Commit mit Nachricht
git tag v0.1.0                    # Versionsnummer setzen
git push                          # Auf GitHub hochladen
git push --tags                   # Tags auch hochladen
```

### Commit-Message Konvention
| Prefix | Bedeutung | Beispiel |
|--------|-----------|---------|
| `feat:` | Neue Funktion | `feat: Vererbung mit TemperaturSensor` |
| `fix:` | Bug repariert | `fix: Tippfehler in Messen-Methode` |
| `refactor:` | Code umgebaut | `refactor: Klasse unten statt oben` |
| `docs:` | Dokumentation | `docs: README hinzugefügt` |
| `cleanup:` | Aufräumen | `cleanup: BenchmarkSuite1 entfernt` |

### Semantic Versioning
```
MAJOR.MINOR.PATCH
  1  .  2  .  3

PATCH  → Bugfix
MINOR  → Neue Funktion, aber abwärtskompatibel
MAJOR  → Großer Umbau
0.x.x  → In Entwicklung, noch kein Release
1.0.0  → Erstes fertiges Release
```

### Bisherige Versionen
| Version | Datum | Inhalt |
|---------|-------|--------|
| v0.1.0 | 01.06.2026 | Hello World, erster Sensor |
| v0.2.0 | 02.06.2026 | List + foreach |
| v0.3.0 | 03.06.2026 | Vererbung, override, Alarm-System |
| v0.4.0 | 04.06.2026 | Interface ISensor + LINQ |

---

## 7. Spickzettel: C# Syntax-Referenz

### Datentypen
| Typ | Beschreibung | Beispiel |
|-----|-------------|---------|
| `int` | Ganzzahl | `int x = 42;` |
| `double` | Kommazahl | `double pi = 3.14;` |
| `string` | Text | `string name = "Ofen";` |
| `bool` | Wahr/Falsch | `bool alarm = true;` |

### String-Formatierung
```csharp
$"{variable}"           // Interpolated String
$"{wert:F2}"            // 2 Nachkommastellen: 42,70
$"{wert:F3}"            // 3 Nachkommastellen: 42,700
$"{wert:F0}"            // Keine Nachkomma: 43
$"{name,-15}"           // Linksbündig, 15 Zeichen breit
$"{wert,8:F2}"          // Rechtsbündig, 8 Zeichen, 2 Dezimalen
```

### Zugriffsmodifizierer
| Modifier | Sichtbarkeit |
|----------|-------------|
| `public` | Überall sichtbar |
| `private` | Nur in eigener Klasse |
| `protected` | Eigene Klasse + Kindklassen |

### Schleifen
```csharp
// for — feste Anzahl
for (int i = 0; i < 5; i++) { }

// foreach — über Sammlung
foreach (Sensor s in sensoren) { }

// while — solange Bedingung true
while (bedingung) { }
```

### Konsole
```csharp
Console.WriteLine("Text");           // Ausgabe mit Zeilenumbruch
Console.WriteLine();                 // Leerzeile
Console.ForegroundColor = ConsoleColor.Red;   // Farbe setzen
Console.ResetColor();                // Farbe zurücksetzen
Console.ReadLine();                  // Auf Eingabe warten
Thread.Sleep(500);                   // 500ms Pause
```

---

## 8. Typische Fehler & Lösungen

### CS8803: Anweisungen müssen vor Typdeklarationen stehen
**Problem:** Klasse steht ÜBER dem Hauptprogramm.
**Lösung:** Hauptprogramm OBEN, Klassen UNTEN.

### CS0101: Doppelte Definition
**Problem:** Eine Klasse existiert in zwei Dateien (z.B. Program.cs UND Sensor.cs).
**Lösung:** Extra Datei löschen — alles in eine Datei.

### CS0103: Name im aktuellen Kontext nicht vorhanden
**Problem:** Property nicht deklariert (z.B. `Min` oder `Max` vergessen).
**Lösung:** Property in der Klasse hinzufügen.

### "dotnet" wird nicht erkannt
**Problem:** PATH-Variable fehlt.
**Lösung:** `setx PATH "%PATH%;C:\Program Files\dotnet\x64"` → CMD neu starten.

### "fatal: not in a git directory"
**Problem:** Git vertraut dem Parallels-Netzlaufwerk nicht.
**Lösung:** `git config --global safe.directory *`

### "error: src refspec main does not match any"
**Problem:** Branch heißt `master` statt `main`.
**Lösung:** `git branch -M main`

### "error: failed to push" (Repo existiert schon)
**Lösung:** `git push -u origin main --force`

### Visual Studio startet falsches Projekt
**Problem:** Es gibt mehrere Projekte in der Solution.
**Lösung:** Projektmappen-Explorer → Rechtsklick → "Als Startprojekt festlegen"

---

> *"Ein Ingenieur der in vier Tagen .NET installiert, Git zum Laufen bringt, C# mit Vererbung und LINQ schreibt UND es auf GitHub pusht — der braucht nur jemanden der ihm sagt wo der nächste Knopf ist."*
> — Jackie, Juni 2026
