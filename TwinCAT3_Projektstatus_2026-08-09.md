# TwinCAT 3 – Projektstatus Palettenstation (Update)

**Stand:** 2026-08-09
**Bezug:** `TwinCAT3_Projektstatus_2026-08-08.md`
**Referenzprojekt:** `PalettenStation_V2/TestV2`
**Zielsystem:** Beckhoff CP6606 / Windows CE7 / ARMV7
**TwinCAT XAE:** Build 4022.36

## Neu seit letztem Stand

- Simulationsschalter `GVL_IO.SIM_Aktiv : BOOL := FALSE` implementiert — der in `TwinCAT3_Lagerlogistik_Projekt.md` (Kap. 16, Phase 1) als "wichtigster nächster Schritt" vorgemerkte Punkt.
- `MAIN` erweitert: acht bereits deklarierte, bis dahin ungenutzte lokale `bXxx`-Variablen werden zu Beginn jedes Zyklus je nach `SIM_Aktiv` entweder aus `GVL_IO.DI_*` (Hardware, Default) oder `GVL_Simulation.GVL_SIM_*` (Simulation) befüllt. Die komplette Schrittkette liest seither diese lokalen Variablen statt direkt der Hardware-Eingänge.
- Komplette Schrittkette einmal vollständig über Simulation durchgespielt: S0 → S1 → S2 → S3 → S4 → S8 (Alarm, da `AI_FolienTemperatur = 0` nicht simuliert und damit außerhalb 165–185) → S9 → zurück zu S0 (nach Force von `AI_FolienTemperatur = 175`, `AI_MotorTemperatur = 50`, `GVL_SIM_WerkerQuittierung = TRUE`).
- Bestätigt: `AI_FolienTemperatur`/`AI_MotorTemperatur` sind bewusst **nicht** Teil des Simulationsschalters — die EL3681 ist ja weiterhin nur für v1.3 geplant. Für Tests über S4 hinaus müssen die beiden Analogwerte einzeln geforct werden.
- Python-ADS-Sanity-Check erfolgreich: `ads_read_test.py` liest live `GVL_IO`-Werte via `pyads` direkt vom CP6606 (AMS Net ID `5.35.203.54.1.1`, Port 851) — Nachweis, dass ADS auch außerhalb von XAE und C# funktioniert.

## Stolperstein: externe Datei-Änderungen bei offenem XAE-Projekt

Wenn `.TcPOU`/`.TcGVL`-Dateien außerhalb von XAE geändert werden, während sie in XAE bereits geöffnet sind, merkt XAE die externe Änderung nicht automatisch und arbeitet mit der alten, im Speicher gehaltenen Version weiter. Ein anschließendes Speichern in XAE schreibt dann versehentlich wieder den alten Stand zurück, obwohl die Datei auf der Festplatte kurzzeitig aktueller war. Fix: betroffene Datei in XAE schließen und neu öffnen, bevor mit einer extern eingespielten Änderung weitergearbeitet wird — sonst debuggt man scheinbar korrekten Code, der in Wahrheit nie geladen wurde.

## Nächste Schritte

1. Geforcte Werte (`AI_FolienTemperatur`, `AI_MotorTemperatur`, `GVL_SIM_*`) wieder lösen (`Strg+F8`), bevor an realer Hardware weitergearbeitet wird.
2. Alle realen I/O-Verknüpfungen weiterhin systematisch prüfen und dokumentieren (offen seit 08.08.).
3. `AI_FolienTemperatur`/`AI_MotorTemperatur` später in die Simulation aufnehmen, sobald die EL3681-Integration (v1.3) ansteht.
4. Fokus wechselt jetzt auf **WCF (CoreWCF)** — die einzige explizite Lücke gegenüber dem Anforderungsprofil der WITRON-Stellenanzeige (siehe `Stellenanzeige-WITRON-Service.md` im Claude-Projekt "Witron Stelle"). Erster minimaler CoreWCF-Demo-Service in Arbeit, noch nicht verifiziert (kein Compiler-Zugriff in der Cloud-Sandbox — wird gemeinsam über Build-Output getestet).
