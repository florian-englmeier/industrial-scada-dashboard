# TwinCAT 3 – Projektstatus Palettenstation

**Stand:** 2026-08-08  
**Referenzprojekt:** `PalettenStation_V2/TestV2`  
**Zielsystem:** Beckhoff CP6606 / Windows CE7 / ARMV7  
**TwinCAT XAE:** **Build 4022.36**

## Aktueller funktionierender Stand

- CP6606 erreichbar
- ADS-Route funktioniert
- CP6606 AMS Net ID: `5.35.203.54.1.1`
- TwinCAT Runtime im RUN
- EtherCAT-Master in OP
- EtherCAT-Klemmen in OP
- EL1002 reagiert physisch und im TwinCAT-Prozessabbild
- PLC Port 851 wieder erreichbar
- `TestV2` ist das aktuelle Referenzprojekt

## Wichtig: Build 4022.36 verwenden

Das Projekt muss mit TwinCAT XAE Build **4022.36** geladen und übersetzt werden.

Mit Build 4024.75 trat folgendes Verhalten auf:

- EtherCAT scheinbar OP
- EL1002 LED reagierte physisch
- Online-Prozesswert blieb jedoch 0

Nach Laden mit Build 4022.36 und Rebuild funktionierte der EL1002 wieder korrekt.

## I/O-Verknüpfungen

Festgestellt wurde:

`GVL_IO.DI_PaletteEingang` war zuvor mit:

```text
EL1004 Input 1
```

verknüpft.

Die Variable wurde auf:

```text
Klemme 2 (EL1002)
→ Channel 1
→ Input
↔ GVL_IO.DI_PaletteEingang
```

umgelegt.

Weitere I/O-Verknüpfungen sollen anschließend systematisch geprüft und dokumentiert werden.

## PLC-Schrittkette

Die CASE-Schrittkette wurde bereits erfolgreich getestet:

```text
S0 → Bereit
S1 → Förderband
S2 → Palette stoppen
S3 → Hubwerk hoch
S4 → Temperaturprüfung
S5 → Prozess / TON 5 s
S6 → Hubwerk herunter
S7 → Palette verlässt Station
→ zurück zu S0
```

Alarmzustände:

```text
S8 → Alarm setzen
S9 → Alarm / Quittierung
```

## Debugging-Erkenntnisse

### Schreiben vs. Forcen

Hardware-Eingänge `%I*` werden zyklisch vom EtherCAT-Prozessabbild aktualisiert.

- `Werte schreiben` hält deshalb nicht dauerhaft.
- Für einen gezielten Test eines echten Eingangs muss `Forcen` verwendet werden.
- Nach dem Test Force-Werte wieder vollständig aufheben.

### Breakpoints

Breakpoints wurden erfolgreich eingesetzt, u. a. in:

- S3
- S4
- S5
- S6
- S7

Damit konnte der Sprung nach S8 auf:

```text
DI_KollisionFrei = FALSE
```

zurückgeführt werden.

## ADS-Fehler 0x707 – gelöst

Fehler:

```text
System not connected
Ads-Error 0x707
Device is not in a ready state
```

Tatsächliche Ursache:

> Falscher bzw. veralteter ADS-Route-Eintrag für den SOYO-PC auf dem CP6606.

Lösung:

- falsche/alte Routen entfernt
- korrekte Route neu angelegt
- TwinCAT neu gestartet
- PLC Port 851 danach wieder erreichbar

Details siehe:

```text
CP6606_ADS_0x707_Recovery.md
```

## Nächste Schritte

1. Alle realen I/O-Verknüpfungen prüfen und dokumentieren.
2. CASE-Ablauf einmal mit realen bzw. korrekt forcierten I/Os testen.
3. Diagnosevariablen / Fehlercodes ergänzen.
4. ADS-Kommunikation zwischen TwinCAT und C# aufbauen.
5. C#-HMI für aktuellen Schritt, Sensoren, Aktoren, Temperaturen und Alarmzustand erstellen.
