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
- **CoreWCF-Demo-Service (`WcfDemo`) end-to-end verifiziert.** `PalettenStationService.GetStatus()` läuft unter `http://localhost:5289/PalettenStationService.svc` (BasicHttpBinding) und liefert per curl-Test einen sauberen SOAP-Envelope zurück — `GetStatusResponse`/`GetStatusResult` mit allen sechs `[DataMember]`-Feldern (`Schritt`, `SchrittText`, `FolienTemperatur`, `MotorTemperatur`, `AlarmAktiv`, `Zeitstempel`) korrekt serialisiert. Details und Konzepte in `TUTORIAL_v1.3_CoreWCF_SOAP.md`.
- **Live-Dashboard für den SOAP-Service gebaut und verifiziert (`dashboard.html`).** Browserseite pollt `GetStatus()` automatisch (Standard alle 2s), zeigt Schrittkette S0–S9, Alarm-Status und Analogwerte mit Trendpfeilen/Flash-Animation bei Änderung, plus aufklappbare rohe SOAP-Anfrage/Antwort. Ursprünglich über einen separaten lokalen Static-Server (`python -m http.server`) ausgeliefert — dafür musste CORS in `Program.cs` ergänzt werden (`AddCors` + `UseCors()`, vor `UseServiceModel`).
- **Architektur vereinfacht: `dashboard.html` nach `WcfDemo/wwwroot/` verschoben, `app.UseStaticFiles()` in `Program.cs` ergänzt** (nach `UseCors()`, vor `UseServiceModel`). Dashboard und SOAP-Service laufen jetzt aus demselben Kestrel-Prozess unter derselben Origin (`http://localhost:5289/dashboard.html`) — exakt dasselbe Muster wie `SensorAPI/wwwroot/index.html`. Der separate Python-Server ist damit für dieses Setup nicht mehr nötig. CORS bleibt trotzdem im Code, falls das Dashboard mal von einer anderen Origin aus aufgerufen wird.

## Stolperstein: externe Datei-Änderungen bei offenem XAE-Projekt

Wenn `.TcPOU`/`.TcGVL`-Dateien außerhalb von XAE geändert werden, während sie in XAE bereits geöffnet sind, merkt XAE die externe Änderung nicht automatisch und arbeitet mit der alten, im Speicher gehaltenen Version weiter. Ein anschließendes Speichern in XAE schreibt dann versehentlich wieder den alten Stand zurück, obwohl die Datei auf der Festplatte kurzzeitig aktueller war. Fix: betroffene Datei in XAE schließen und neu öffnen, bevor mit einer extern eingespielten Änderung weitergearbeitet wird — sonst debuggt man scheinbar korrekten Code, der in Wahrheit nie geladen wurde.

## Stolperstein: SOAP-Test per curl in PowerShell

Drei unabhängige Fallen auf dem Weg zum ersten erfolgreichen curl-Test gegen den CoreWCF-Service, der Reihe nach aufgetreten:

1. `curl` ist in PowerShell standardmäßig ein Alias für `Invoke-WebRequest` (andere Syntax, z.B. `-Headers`-Hashtable statt wiederholtem `-H`). Fix: `curl.exe` explizit aufrufen.
2. Multi-Zeilen-Paste in Windows Terminal löst einen Sicherheits-Bestätigungsdialog aus (`multiLinePasteWarning`) — kein Fehler, einfach bestätigen.
3. In *Windows PowerShell* (5.1, nicht PowerShell 7) gibt es den Encoding-Wert `utf8NoBOM` nicht, und `-Encoding UTF8` schreibt dort immer mit BOM. Sauberster Fix am Ende: den SOAP-Request-XML-Body direkt in einem Editor als Datei `request.xml` anlegen (UTF-8 ohne BOM — moderner Windows-Notepad macht das seit Windows 10 automatisch) statt ihn per PowerShell-Heredoc zu erzeugen, und dann `curl.exe --data-binary "@request.xml"` verwenden. Zusätzlicher Stolperstein dabei: PowerShell startete im geschützten Ordner `C:\WINDOWS\system32`, wo Schreibzugriff verweigert wird — einfach vorher in einen normalen Ordner wechseln (`cd`).

## Stolperstein: Live-Dashboard gegen SOAP-Service (CORS + lokale Static-Server-Kette)

Mehrschichtiges Debugging, bis das browserbasierte Live-Dashboard wirklich Daten zeigte:

1. `dashboard.html` **nicht** per Doppelklick (`file:///...`) öffnen — Firefox blockiert `fetch()`-Aufrufe von `file://`-Seiten zu `http://`-Adressen grundsätzlich, unabhängig von CORS. Fix: Datei über einen lokalen Static-Server ausliefern (`python -m http.server 8080`) und über `http://localhost:8080/...` öffnen.
2. Beide Server müssen parallel in **getrennten** Terminal-Fenstern laufen — der WcfDemo-Service (`dotnet run`, Port 5289) und der Static-Server fürs Dashboard (Port 8080). Wurde eines der beiden Fenster geschlossen bzw. wiederverwendet, brach die jeweils andere Verbindung ab.
3. Eigentliche CORS-Fehlermeldung erst über die Browser-Konsole (F12) sichtbar geworden: `OPTIONS`-Preflight-Request landete mit Status 400 und ohne `Access-Control-Allow-Origin`-Header direkt bei CoreWCF statt bei der CORS-Middleware — Ursache war, dass die lokale `Program.cs` noch die alte Version ganz ohne `AddCors`/`UseCors` war (die Ergänzung aus der gelieferten Datei war nie tatsächlich übernommen worden). Nach manuellem Nachtragen der beiden CORS-Blöcke und komplettem Neustart von `dotnet run` (Hot-Reload reicht bei Middleware-Pipeline-Änderungen nicht zuverlässig) lief es.

**Lehre:** Bei "es tut nicht" im Browser lohnt sich fast immer zuerst ein Blick in die Konsole (F12) — die generische `fetch()`-Fehlermeldung ("NetworkError...") ist für Diagnosezwecke nutzlos, die Konsole zeigt den echten Grund.

**Nachtrag (10.08.):** Nach der erfolgreichen Fehlersuche wurde die Architektur bewusst vereinfacht — `dashboard.html` liegt jetzt in `WcfDemo/wwwroot/`, `app.UseStaticFiles()` liefert es über denselben Prozess wie den SOAP-Service aus. Der Zwei-Server-Aufbau (Python + dotnet) war nur nötig, solange das Dashboard extern lag; für den Dauerbetrieb ist ein einzelner Prozess robuster und näher am bestehenden `SensorAPI`-Muster.

## Nächste Schritte

1. Geforcte Werte (`AI_FolienTemperatur`, `AI_MotorTemperatur`, `GVL_SIM_*`) wieder lösen (`Strg+F8`), bevor an realer Hardware weitergearbeitet wird.
2. Alle realen I/O-Verknüpfungen weiterhin systematisch prüfen und dokumentieren (offen seit 08.08.).
3. `AI_FolienTemperatur`/`AI_MotorTemperatur` später in die Simulation aufnehmen, sobald die EL3681-Integration (v1.3) ansteht.
4. ~~Fokus wechselt jetzt auf WCF (CoreWCF)~~ → **erledigt**, siehe oben und `TUTORIAL_v1.3_CoreWCF_SOAP.md`.
5. ~~Live-Dashboard für den SOAP-Service~~ → **erledigt**, `dashboard.html` läuft verifiziert gegen den laufenden Service, jetzt aus `wwwroot` über denselben Prozess.
6. `PalettenStationService.GetStatus()` von Platzhalterdaten auf echte `AdsService`-Logik umstellen (gleiche ADS-Verbindung zum CP6606 wie in SensorAPI, nur SOAP statt REST als Außenhülle) — nächster konkreter Schritt für die WITRON-Bewerbung.
7. Alternativ: EL3681-Hardwareintegration für echte Folien-/Motortemperatur-Messung angehen (siehe Punkt 3).
