# TwinCAT-3-Lagerlogistikprojekt

**Stand:** Juli 2026, nach Release v1.2.0 **Verwandte Dokumente:** `TUTORIAL_v1.2_ADS_Integration.md` — konkrete Umsetzung der ADS-Schnittstelle

## 1\. Projektziel

Ziel ist der Aufbau einer realitätsnahen Lagerlogistik-Simulation mit TwinCAT 3 und vorhandener Beckhoff-Hardware.

Das System soll typische Funktionen einer automatisierten Förder- und Lageranlage abbilden:

- Erkennen einer Palette über digitale Sensoren  
- Ansteuern von Fördertechnik und Hubeinheiten  
- Abbilden eines definierten Prozessablaufs  
- Überwachen von Temperaturen und elektrischen Zuständen  
- Erzeugen und Quittieren von Alarmen  
- Datenaustausch mit einer in C\# entwickelten Benutzeroberfläche  
- Spätere Erweiterung um Visualisierung, Diagnose und Messdatenauswertung

---

## 2\. Grundidee der Anlage

Eine Palette wird durch eine kleine simulierte Förder- oder Lageranlage transportiert.

Dabei werden verschiedene Stationen durchlaufen:

1. Palette wird eingelegt.  
2. Ein Sensor erkennt die Palette.  
3. Der Förderantrieb wird freigegeben.  
4. Die Palette fährt zur nächsten Station.  
5. Ein weiterer Sensor bestätigt die Position.  
6. Eine Hubeinheit oder ein anderer Aktor wird angesteuert.  
7. Prozesswerte wie Temperatur, Spannung oder Strom werden überwacht.  
8. Bei Grenzwertverletzungen wird ein Alarm erzeugt.  
9. Ein Werker quittiert den Alarm über die Bedienoberfläche.  
10. Der Prozess wird fortgesetzt oder in einen sicheren Zustand gebracht.

Die reale Mechanik kann zunächst vollständig durch SPS-Variablen simuliert werden.

---

## 3\. Vorhandene und installierte Komponenten

### 3.1 Installierte Steuerungs- und Bedienhardware

- Beckhoff CP6606 Panel-PC beziehungsweise Bedienpanel  
- TwinCAT 3 Engineering  
- EtherCAT-Koppler EK1100  
- EtherCAT-Endklemme  
- DIN-Tragschiene TS35  
- 24-V-DC-Netzteil  
- Entwicklungsrechner (nativer Windows-PC) mit Visual Studio 2022  
- C\#-Benutzeroberfläche (SensorAPI)

### 3.2 Digitale Eingangsklemmen

Digitale Eingangsklemmen werden für simulierte oder reale Sensorsignale verwendet.

Mögliche Signale:

- Palette am Eingang vorhanden  
- Palette an Station 1  
- Palette an Station 2  
- Hubwerk oben  
- Hubwerk unten  
- Schutztür geschlossen  
- Not-Halt freigegeben  
- Störung quittiert

Beispiel:

- EL1002 als zweikanalige digitale Eingangsklemme

Ein Eingang wird aktiv, wenn gegenüber dem gemeinsamen 0-V-Potential ein gültiges 24-V-Signal anliegt.

### 3.3 Digitale Ausgangsklemmen

Digitale Ausgänge können folgende Aktoren oder Signallampen simulieren beziehungsweise ansteuern:

- Fördermotor  
- Hubeinheit aufwärts  
- Hubeinheit abwärts  
- Stopper öffnen  
- Stopper schließen  
- Warnleuchte  
- Summer  
- Betriebsanzeige

Reale induktive Lasten dürfen nicht ohne geeignete Schutzbeschaltung und Leistungsanpassung direkt angeschlossen werden.

### 3.4 Analoge Ein- und Ausgänge

Analoge Kanäle können für folgende Werte genutzt werden:

- Temperatur einer Druck- oder Folienstation  
- Druck  
- Weg  
- Geschwindigkeit  
- Sollwert eines Antriebs  
- Belastung oder Prozesssignal

Bei der Inbetriebnahme müssen Signalart und Messbereich geprüft werden, zum Beispiel:

- 0 bis 10 V  
- ±10 V  
- 0 bis 20 mA  
- 4 bis 20 mA  
- Widerstandssensor oder Thermoelement

### 3.5 Elektrische Messtechnik (EL3681)

Die **Beckhoff EL3681** (Digitalmultimeter-Klemme) hängt bereits physisch am EtherCAT-Bus und wartet auf ihre Integration in v1.3.0.

- Spannungsmessung DC/AC bis 300 V  
- Strommessung DC/AC bis 10 A  
- Widerstandsmessung  
- Autoranging und Effektivwerte  
- Anbindung wie normale EtherCAT-Klemme

---

## 4\. EL3681 als sinnvoller Projektbaustein

Die **Beckhoff EL3681** soll im Projekt als möglicher Baustein für die elektrische Zustandsüberwachung berücksichtigt werden.

Frühere Bezeichnung im Gespräch: EL6381. Gemeint ist sehr wahrscheinlich die EL3681.

### Mögliche Aufgaben im Projekt

- Messen einer Gleich- oder Wechselspannung  
- Überwachen einer Versorgungsspannung  
- Erfassen eines Stromwerts über einen geeigneten Messaufbau  
- Erkennen von Unterspannung oder Überspannung  
- Erkennen ungewöhnlicher elektrischer Lastzustände  
- Diagnose eines simulierten Motors oder Verbrauchers  
- Erzeugen von Warnungen und Alarmen  
- Aufzeichnen elektrischer Messwerte für spätere Analysen

### Beispielhafte Alarmbedingungen

Versorgungsspannung zu niedrig

Versorgungsspannung zu hoch

Motorstrom zu hoch

Motorstrom trotz eingeschaltetem Ausgang zu niedrig

Elektrischer Verbraucher nicht angeschlossen

Unplausibler Messwert

### Beispiel für eine Diagnose

Wenn der Fördermotor eingeschaltet ist, wird ein bestimmter Strombereich erwartet.

Motor AUS:

Strom ungefähr 0 A

Motor EIN:

Strom innerhalb eines definierten Normalbereichs

Motor EIN und Strom zu niedrig:

Möglicherweise Leitungsbruch oder Motor nicht angeschlossen

Motor EIN und Strom zu hoch:

Möglicherweise Blockade oder Überlast

Die EL3681 kann damit später ein gutes Bindeglied zwischen SPS-Programmierung, Messtechnik, Zustandsüberwachung und Datenanalyse bilden.

---

## 5\. Elektrischer Grundaufbau

### 5.1 Versorgung

Für den Aufbau wird eine 24-V-DC-Versorgung verwendet.

Wichtig:

- `+24 V` ist das positive Versorgungspotential.  
- `0 V` ist das Bezugspotential.  
- `0 V` ist nicht automatisch Schutzleiter.  
- Schutzleiter, Funktionserde und 0 V müssen entsprechend dem Aufbau korrekt behandelt werden.  
- Vor dem Anschließen einer Klemme muss deren Datenblatt geprüft werden.

### 5.2 Digitale Eingangssimulation

Ein digitaler Eingang kann für Testzwecke mit 24 V beschaltet werden.

Voraussetzung:

- Das 0-V-Potential der Signalquelle und der Eingangsklemme muss gemeinsam verbunden sein.  
- Die zulässige Eingangsspannung darf nicht überschritten werden.  
- Es muss eindeutig bekannt sein, welcher Anschluss Signal und welcher Anschluss Versorgung beziehungsweise Masse ist.

Für erste Softwaretests können Eingangsvariablen auch intern simuliert werden. Das direkte Schreiben eines physisch verknüpften Eingangssignals ist jedoch nicht immer möglich, weil das Prozessabbild bei jedem Buszyklus mit dem realen Eingangswert überschrieben wird.

---

## 6\. TwinCAT-Projektstruktur (Ziel-Struktur)

Empfohlene Struktur:

PLC

├── DUTs

│   ├── E\_Anlagenzustand

│   ├── E\_Prozessschritt

│   ├── ST\_Eingaenge

│   ├── ST\_Ausgaenge

│   ├── ST\_Analogwerte

│   └── ST\_Alarm

│

├── GVLs

│   ├── GVL\_IO

│   ├── GVL\_Process

│   ├── GVL\_Alarm

│   └── GVL\_HMI

│

├── POUs

│   ├── MAIN

│   ├── FB\_Lagerprozess

│   ├── FB\_AlarmManager

│   ├── FB\_Simulation

│   └── FB\_Diagnose

│

└── VISUs

    └── optionale TwinCAT-Visualisierung

**Aktuelle Umsetzung (v1.2.0):** Es existieren `GVL_IO`, `GVL_Simulation` und `MAIN` mit einer CASE-Schrittkette (siehe Kapitel 13).

---

## 7\. Globale Variablen

### 7.1 Physische beziehungsweise simulierte Eingänge

VAR\_GLOBAL

    bNotHaltFrei        : BOOL;

    bPaletteEingang     : BOOL;

    bPaletteStation1    : BOOL;

    bPaletteStation2    : BOOL;

    bHubOben            : BOOL;

    bHubUnten           : BOOL;

    bQuittierung        : BOOL;

END\_VAR

### 7.2 Ausgänge

VAR\_GLOBAL

    bFoerdererEin       : BOOL;

    bHubAuf             : BOOL;

    bHubAb              : BOOL;

    bWarnleuchte        : BOOL;

    bSummer             : BOOL;

END\_VAR

### 7.3 Prozesswerte

VAR\_GLOBAL

    rTemperatur         : REAL;

    rSpannung           : REAL;

    rStrom              : REAL;

    rTemperaturGrenze   : REAL := 80.0;

    rSpannungMin        : REAL := 22.0;

    rSpannungMax        : REAL := 26.0;

END\_VAR

### 7.4 Status und Alarm

VAR\_GLOBAL

    bAnlageBereit       : BOOL;

    bAutomatikAktiv     : BOOL;

    bStoerung           : BOOL;

    bAlarmTemperatur    : BOOL;

    bAlarmUnterspannung : BOOL;

    bAlarmUeberspannung : BOOL;

    nProzessschritt     : INT;

    sStatusText         : STRING(255);

    sAlarmText          : STRING(255);

END\_VAR

---

## 8\. Trennung zwischen realen und simulierten Signalen

Für die Entwicklung ist eine Umschaltung zwischen Hardwarebetrieb und Simulation sinnvoll.

VAR\_GLOBAL

    bSimulationAktiv : BOOL := TRUE;

END\_VAR

Beispiel:

IF bSimulationAktiv THEN

    bPaletteEingangIntern := bPaletteEingangSimulation;

ELSE

    bPaletteEingangIntern := bPaletteEingangHardware;

END\_IF

Vorteile:

- SPS-Logik kann ohne angeschlossene Sensoren getestet werden.  
- Physische Eingänge werden nicht ständig manuell überschrieben.  
- Die C\#-Oberfläche kann gezielt Sensorsignale simulieren.  
- Später kann auf reale Hardware umgeschaltet werden.

**Aktueller Status (v1.2.0):** Umgesetzt über `GVL_IO.SIM_Aktiv` mit dedizierter `GVL_Simulation`.

---

## 9\. Erster einfacher Prozessablauf

Ein übersichtlicher Ablauf kann zunächst mit einer Schrittkette umgesetzt werden.

**Hinweis:** Das folgende Beispiel ist ein vereinfachtes Lehrbeispiel aus der Konzeptphase und nutzt die klassische SPS-Konvention mit 10er-Schritten (0, 10, 20, 30 ...), die Lücken für spätere Zwischenschritte offenlässt. Die tatsächlich umgesetzte PalettenStation verwendet eine durchnummerierte Schrittkette **S0–S9** (siehe Kapitel 13 / `MAIN.PRG`). Die Nummerierung unterscheidet sich bewusst — es handelt sich um zwei unterschiedliche Stände, nicht um einen Fehler.

CASE nProzessschritt OF

    0:

        sStatusText := 'Anlage wartet';

        bFoerdererEin := FALSE;

        bHubAuf := FALSE;

        bHubAb := FALSE;

        IF bNotHaltFrei AND bPaletteEingang THEN

            nProzessschritt := 10;

        END\_IF

    10:

        sStatusText := 'Palette wird transportiert';

        bFoerdererEin := TRUE;

        IF bPaletteStation1 THEN

            bFoerdererEin := FALSE;

            nProzessschritt := 20;

        END\_IF

    20:

        sStatusText := 'Hub fährt nach oben';

        bHubAuf := TRUE;

        IF bHubOben THEN

            bHubAuf := FALSE;

            nProzessschritt := 30;

        END\_IF

    30:

        sStatusText := 'Prozess abgeschlossen';

        IF NOT bPaletteEingang THEN

            nProzessschritt := 0;

        END\_IF

ELSE

    nProzessschritt := 0;

END\_CASE

**Aktueller Status (v1.2.0):** Ist als Schrittkette S0–S9 in `MAIN` umgesetzt, inklusive Fehlerzweig S8/S9 mit Quittierung. Siehe Grafcet in Kapitel 13\.

---

## 10\. Alarmüberwachung

Beispielhafte Alarmbedingungen:

bAlarmTemperatur := rTemperatur \> rTemperaturGrenze;

bAlarmUnterspannung := rSpannung \< rSpannungMin;

bAlarmUeberspannung := rSpannung \> rSpannungMax;

bStoerung :=

    bAlarmTemperatur

    OR bAlarmUnterspannung

    OR bAlarmUeberspannung;

Ausgabe eines Alarmtexts:

IF bAlarmTemperatur THEN

    sAlarmText := 'Temperaturgrenze überschritten';

ELSIF bAlarmUnterspannung THEN

    sAlarmText := 'Versorgungsspannung zu niedrig';

ELSIF bAlarmUeberspannung THEN

    sAlarmText := 'Versorgungsspannung zu hoch';

ELSE

    sAlarmText := '';

END\_IF

Die Quittierung sollte einen anstehenden physikalischen Fehler nicht einfach löschen. Erst wenn die Fehlerursache nicht mehr vorhanden ist, darf der Alarm zurückgesetzt werden.

---

## 11\. Verbindung zur C\#-Benutzeroberfläche

Die vorhandene Visual-Studio-Anwendung soll als Bedien- und Diagnoseschnittstelle erhalten bleiben.

Mögliche Kommunikation:

- Beckhoff ADS  
- ADS.NET  
- Lesen und Schreiben von SPS-Variablen  
- Anzeige von Zuständen und Prozessschritten  
- Simulation von Sensoren  
- Eingabe von Grenzwerten  
- Alarmanzeige und Quittierung  
- Protokollierung von Ereignissen  
- Spätere Speicherung in einer Datenbank

**Aktueller Status (v1.2.0):** Umgesetzt als `AdsService.cs` in der SensorAPI mit `ReadBool`, `ReadReal`, `ReadInt`. Endpoint `/api/sensor/live` liest 4 SPS-Variablen live. Details in `TUTORIAL_v1.2_ADS_Integration.md`.

### Mögliche HMI-Elemente

- Start  
- Stopp  
- Reset  
- Automatikbetrieb  
- Simulationsbetrieb  
- Palette einlegen  
- Sensor Station 1  
- Sensor Station 2  
- Hub oben  
- Hub unten  
- Temperaturvorgabe  
- Spannungsanzeige  
- Stromanzeige  
- Alarmübersicht  
- Quittiertaste  
- Prozessschrittanzeige  
- Statusprotokoll

---

## 12\. Verknüpfung der Hardware mit SPS-Variablen

Die EtherCAT-Kanäle werden im TwinCAT-I/O-Baum mit SPS-Variablen verknüpft.

Beispiel:

EL1002 Kanal 1

    ↔ GVL\_IO.bPaletteEingangHardware

Digitaler Ausgang Kanal 1

    ↔ GVL\_IO.bFoerdererEinHardware

Eine Verknüpfung kann über den jeweiligen Prozessdatenkanal im I/O-Baum hergestellt oder wieder entfernt werden.

Beim Testen ist zu beachten:

- Physische Eingänge werden durch die Hardware aktualisiert.  
- Manuell geschriebene Werte können deshalb sofort wieder überschrieben werden.  
- Für Tests ist eine getrennte Simulationsvariable sauberer.  
- Ausgänge dürfen nur geschaltet werden, wenn die angeschlossene Last dafür geeignet ist.

---

## 13\. Aktueller Arbeitsstand (Stand v1.2.0, Juli 2026\)

### Was läuft ✅

**Hardware:**

- Beckhoff CP6606 mit TwinCAT 3 Runtime (Windows CE) — grünes Icon, Bootprojekt aktiv  
- EK1100 EtherCAT-Koppler mit DI/DO/AI/AO-Klemmen, LEDs grün  
- EL3681 Digitalmultimeter-Klemme physisch am Bus, wartet auf Integration  
- Native Windows-PC mit TwinCAT XAE (Parallels-VM-Ansatz verworfen wegen Realtime-Treiber-Konflikt)  
- ADS-Route zwischen PC und CP6606 stabil (AMS Net ID `5.35.203.54.1.1`, Port 851\)

**TwinCAT-Projekt "PalettenStation":**

- `PalettenstationPLC` läuft im Bootprojekt-Autostart  
- `GVL_IO` mit allen digitalen Eingängen (bNotHaltFrei, bSchutzhaubeZu, bPaletteEingang, ...) und analogen Werten (AI\_FolienTemperatur, AI\_MotorTemperatur)  
- `GVL_Simulation` mit SIM\_\*-Variablen für alle Sensoren  
- Umschaltung Hardware/Simulation über `GVL_IO.SIM_Aktiv`  
- `MAIN` mit CASE-Schrittkette S0–S9:

S0 Bereit  →  S1 Transport  →  S2 Stopp  →  S3 Hub hoch  →  S4 Temp OK?

                                                                  ↓

              S7 Abtransport  ←  S6 Hub runter  ←  S5 Prozess 5s

                     ↓

                 (zurück zu S0)

Fehlerzweig:      S8 Alarm  →  S9 Warten auf Quittierung  →  S0

**C\#-Seite (SensorAPI):**

- `AdsService.cs` als Singleton in ASP.NET Core registriert  
- Beckhoff.TwinCAT.Ads NuGet 7.0.292  
- Methoden `ReadBool`, `ReadReal`, `ReadInt` mit Guard Clauses und Exception Handling  
- Endpoint `GET /api/sensor/ads-status` (Verbindungsstatus)  
- Endpoint `GET /api/sensor/live` (4 SPS-Variablen live als JSON: Schritt, FolienTemp, MotorTemp, AlarmAktiv)

### Was noch offen ist ⬜

- EL3681 in TwinCAT-Konfiguration einbinden  
- Elektrische Messwerte in GVL\_IO ergänzen (AI\_Motorspannung, AI\_Motorstrom)  
- Schrittkette um Diagnose-Regeln für elektrische Werte erweitern  
- Live-Werte im bestehenden HTML-Dashboard anzeigen (bisher nur JSON im Browser)  
- Write-Endpoint für Alarm-Quittierung aus dem Dashboard  
- CSV-Trendlog für Messwerte  
- Bulk-Read und ADS-Notifications für bessere Performance

---

## 14\. Langfristige Erweiterungen

- Zustandsautomat mit ENUM statt numerischer Schrittwerte  
- Funktionsbausteine für Förderer, Hubwerk und Sensoren  
- Zeitüberwachung einzelner Bewegungen  
- Fehler bei nicht erreichtem Endschalter  
- Motorstromüberwachung  
- Predictive-Maintenance-Ansatz  
- Datenbankanbindung  
- Trendanzeige für Temperatur, Spannung und Strom  
- Protokollierung von Alarmen und Quittierungen  
- Benutzer- und Rollenverwaltung  
- Digitaler Zwilling der Anlage  
- Automatisierte Tests der SPS-Logik  
- Dokumentation der Ein- und Ausgangsbelegung

---

## 15\. Strategische Roadmap (Grundprinzip)

Die weitere Entwicklung sollte bewusst in klar getrennten Stufen erfolgen. Dadurch lassen sich Fehler leichter zuordnen und das Projekt bleibt übersichtlich.

### Phase 1: Hardware und Simulation sauber trennen ✅

**Status: erledigt in v1.0.**

Für jedes relevante Eingangssignal werden drei Ebenen vorgesehen:

bPaletteEingangHardware

bPaletteEingangSimulation

bPaletteEingang

Die SPS entscheidet abhängig vom Betriebsmodus, welches Signal verwendet wird.

### Phase 2: Einen kleinen vollständigen Prozess umsetzen ✅

**Status: erledigt in v1.0 als S0–S9-Kette.**

### Phase 3: Zustandsmaschine einführen ⬜

Der Prozess sollte nicht dauerhaft nur über viele einzelne BOOL-Verknüpfungen gesteuert werden. Empfohlen wird ein eigener Datentyp:

TYPE E\_ProcessState :

(

    Idle,

    Transport,

    Lifting,

    Complete,

    Fault

);

END\_TYPE

**Status: geplant. Aktuell wird noch mit numerischem `Schritt : INT` gearbeitet. Refactoring auf ENUM in einer späteren Version.**

### Phase 4: C\#-Schnittstelle anbinden ✅

**Status: erledigt in v1.2.0 als AdsService mit Live-Endpoint.**

### Phase 5: Reale Hardware schrittweise integrieren ⬜

**Status: teilweise begonnen — Hardwareverknüpfungen sind gemappt, echte Sensoren an den DIs noch nicht angeschlossen.**

### Phase 6: Alarm- und Diagnosefunktionen ergänzen ⬜

**Status: teilweise — S8/S9 Alarmzweig existiert, aber noch keine Zeitüberwachungen, keine Plausibilitätsprüfungen.**

### Phase 7: EL3681 integrieren ⬜

**Status: EL3681 hängt physisch am Bus, Integration ist Ziel von v1.3.0.** Details in Kapitel 18\.

---

## 16\. Ursprüngliche nächste Arbeitsschritte (v1.0-Planung)

Die konkret empfohlene Reihenfolge lautete:

1. ✅ `GVL_IO` in Hardware- und Simulationssignale aufteilen.  
2. ✅ `bSimulationAktiv` einführen.  
3. ⬜ `E_ProcessState` als ENUM anlegen. *(numerisch umgesetzt, ENUM-Refactoring später)*  
4. ⬜ `FB_Lagerprozess` erstellen. *(aktuell noch alles in MAIN)*  
5. ✅ Ablauf `Idle → Transport → Lifting → Complete` umsetzen (als S0–S9).  
6. ✅ Prozess vollständig im Online-Modus getestet.  
7. ✅ **C\#-Oberfläche über ADS anbinden.** *(erledigt v1.2.0)*  
8. ⬜ Einen realen digitalen Eingang integrieren.  
9. ⬜ Weitere Eingänge und Ausgänge ergänzen.  
10. ⬜ Alarmmanager als eigener FB aufbauen.  
11. ⬜ **EL3681 für Spannungs-, Strom- und Zustandsüberwachung integrieren.** *(nächster Schwerpunkt in v1.3.0)*  
12. ⬜ Datenaufzeichnung und spätere Analyse ergänzen.

### Strategische Leitlinie

Die Reihenfolge lautet bewusst:

Simulation

→ stabiler SPS-Prozess

→ C\#-Schnittstelle       ← wir sind hier (v1.2.0)

→ reale Hardware

→ Diagnose               ← Ziel v1.3.0 (EL3681)

→ EL3681

→ Datenanalyse           ← Ziel v1.4.0 (Predictive Maintenance)

Dadurch bleibt jede Projektphase einzeln testbar und nachvollziehbar.

---

## 17\. Version-History

| Version | Datum | Meilenstein |
| :---- | :---- | :---- |
| v1.0 | Juli 2026 | Roadmap \+ TwinCAT Setup \+ Schrittkette S0-S9 \+ Simulationsumschaltung |
| v1.1 | Juli 2026 | AdsService-Anfang, PaletteStatus, ADS-Router-Troubleshooting |
| **v1.2** | **Juli 2026** | **ADS-Integration mit Live-PLC-Daten (AdsService, /api/sensor/live)** |
| v1.3 | *geplant* | EL3681 Integration (Spannungs-/Strommessung) |
| v1.4 | *geplant* | Predictive Maintenance Feature (Trendlog \+ Diagnose-Regeln) |

---

## 18\. Konkrete Roadmap für die nächsten Releases

### v1.3.0 — EL3681 Integration

**Ziel:** Echte elektrische Messwerte des simulierten Fördermotors in die SPS und ins Dashboard bringen.

**Aufgabenpaket:**

1. **TwinCAT XAE:** EL3681 in der EtherCAT-Konfiguration finden und Kanäle aktivieren  
     
   - Klemme ist bereits physisch am Bus  
   - Prüfen ob EtherCAT-Scan sie erkannt hat  
   - Wenn nicht: erneuter Bus-Scan

   

2. **GVL\_IO erweitern:**  
     
   VAR\_GLOBAL  
     
       AI\_Motorspannung : REAL;   // V, aus EL3681  
     
       AI\_Motorstrom    : REAL;   // A, aus EL3681  
     
   END\_VAR  
     
3. **Mapping im I/O-Baum:** EL3681-Kanäle ↔ GVL\_IO-Variablen verknüpfen  
     
4. **Skalierung prüfen:** Rohwert der Klemme in physikalischen Wert umrechnen (V bzw. A). Datenblatt EL3681 konsultieren.  
     
5. **GVL\_Simulation ergänzen:** `SIM_Motorspannung`, `SIM_Motorstrom` — analog zu bestehenden Sim-Werten  
     
6. **Umschaltung in MAIN:**  
     
   IF GVL\_IO.SIM\_Aktiv THEN  
     
       AI\_Motorspannung := GVL\_Simulation.SIM\_Motorspannung;  
     
       AI\_Motorstrom    := GVL\_Simulation.SIM\_Motorstrom;  
     
   ELSE  
     
       // echte EL3681-Werte werden bereits durch das Mapping befüllt  
     
   END\_IF  
     
7. **SensorAPI erweitern:** Neuer Endpoint `GET /api/sensor/electrical`  
     
   \[HttpGet("electrical")\]  
     
   public ActionResult GetElectrical()  
     
   {  
     
       return Ok(new  
     
       {  
     
           Spannung \= Math.Round(\_ads.ReadReal("GVL\_IO.AI\_Motorspannung"), 2),  
     
           Strom \= Math.Round(\_ads.ReadReal("GVL\_IO.AI\_Motorstrom"), 3),  
     
           Zeitstempel \= DateTime.Now.ToString("HH:mm:ss")  
     
       });  
     
   }  
     
8. **Testen:** Erst im Simulationsmodus (Werte forcen in GVL\_Simulation), dann mit realer Messung

**Strategischer Wert:** Das ist der Übergang von "SPS mit Simulation" zu "SPS mit echter Messtechnik". Damit ist das Projekt technisch vollständig für Industrieautomation.

---

### v1.4.0 — Predictive Maintenance Feature

**Ziel:** Aus den elektrischen Messwerten Zustandsdiagnose ableiten und in einem Trendlog historisieren.

**Aufgabenpaket:**

1. **Diagnose-Regeln in der Schrittkette:**  
     
   // Beispielhafte Regel: Motor läuft, aber kein Strom fließt  
     
   IF GVL\_IO.DO\_Foerderband  
     
      AND GVL\_IO.AI\_Motorstrom \< 0.1  
     
   THEN  
     
       GVL\_IO.DiagnoseText := 'Verdacht: Leitungsbruch oder Motor nicht angeschlossen';  
     
       GVL\_IO.AlarmAktiv := TRUE;  
     
   END\_IF  
     
2. **Weitere Regeln:**  
     
   - Motor EIN und Strom zu hoch → Blockade oder Überlast  
   - Versorgungsspannung außerhalb Toleranz → Netzstörung  
   - Strom-Zeitverlauf zeigt Anstieg → beginnender Verschleiß

   

3. **CSV-Logging in der SensorAPI:**  
     
   - Bei jedem Endpoint-Call `/api/sensor/electrical` einen Eintrag in `messdaten.csv`  
   - Format: `Zeitstempel;Spannung;Strom;DiagnoseText`  
   - Wiederverwendung des CSV-Codes aus dem TUTORIAL Abschnitt 6 (v0.5)

   

4. **Trend-Endpoint:** `GET /api/sensor/trend?minutes=60` liefert die letzten N Minuten aus dem CSV zurück  
     
5. **Dashboard-Erweiterung:** Chart.js-basierte Trendkurve für Spannung und Strom

**Strategischer Wert:** Das ist der Punkt an dem das Projekt **wirklich** die Sprache der Intralogistik-Branche spricht. Predictive Maintenance ist eines der zentralen Schlagworte dieser Branche.

---

### Nach v1.4 (offene Themen)

- **Write-Endpoint für die SPS:** Alarme aus dem Dashboard quittieren, Simulationswerte setzen  
- **Bulk-Read via SumReader:** Alle 4-6 Variablen in einem Roundtrip statt einzeln  
- **ADS-Notifications:** Push statt Poll — SPS meldet Änderungen aktiv an die API  
- **Entity Framework:** Ablösung des CSV-Logs durch echte Datenbankpersistenz  
- **ENUM-Refactoring:** `E_ProcessState` einführen (Phase 3 der ursprünglichen Roadmap)  
- **Funktionsbausteine:** `FB_Lagerprozess`, `FB_AlarmManager` — MAIN entschlacken

---

*"Simulation → Prozess → C\#-Schnittstelle → reale Messtechnik → Diagnose → Datenanalyse. Jede Phase einzeln testbar, jede Phase ein Meilenstein."*  
