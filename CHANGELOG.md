# Changelog — TwinCAT 3 / Industrial SCADA Dashboard

Laufendes Projekt-Tagebuch, neuester Eintrag zuerst. Ersetzt die frühere Praxis, für jeden Tag eine eigene Datei
(`TwinCAT3_Projektstatus_YYYY-MM-DD.md`) anzulegen — das führte dazu, dass der Dateiname irgendwann nicht mehr zum
tatsächlichen Änderungsdatum passte. Hier steht das Datum direkt am Eintrag, nicht im Dateinamen, kann also nie veralten.

---

## 2026-08-10

**Referenzprojekt:** `PalettenStation_V2/TestV2` · **Zielsystem:** Beckhoff CP6606 / Windows CE7 / ARMV7 · **TwinCAT XAE:** Build 4022.36

### Neu

- **`PalettenStationService.GetStatus()` von Platzhalterdaten auf echte Live-ADS-Anbindung umgestellt.** `AdsService.cs` 1:1 aus `SensorAPI` übernommen (gleiche Verbindungslogik zum CP6606, nur Namespace angepasst), per Konstruktor-Injection (`AddSingleton<AdsService>()`) in `PalettenStationService` eingebunden. Der SOAP-Service liest jetzt live `GVL_IO.Schritt`, `GVL_IO.AI_FolienTemperatur`, `GVL_IO.AI_MotorTemperatur`, `GVL_IO.AlarmAktiv` — dieselbe Datenquelle wie SensorAPI, nur SOAP statt REST als Außenhülle. Zusätzliches Feld `AdsVerbunden` (bool) im DataContract ergänzt, damit "SPS nicht erreichbar" im Dashboard sichtbar von einer echten Null-Messung unterschieden werden kann (Silent-Bug-Vermeidung, wie schon bei SensorAPI).
- **Schrittketten-Anzeige im Dashboard operatorfreundlich gemacht.** Statt nur `S0`, `S1`, ... zeigt jede Kachel jetzt zusätzlich eine kurze Klartext-Beschreibung (Bereit, Transport, Stopp, Heben, Temp.-Check, Bearbeitung, Senken, Entladen, ALARM, Warten).
- **v1.3 committet, getaggt und nach GitHub gepusht.** Commit `e574c11` (CoreWCF + Live-ADS + Dashboard), dazu ein separater Folge-Commit `5bfe910` (siehe Stolperstein unten), Tag `v1.3` zeigt final auf `5bfe910`. Beim Aufräumen des Repos außerdem versehentlich angelegte `dotnet new`-Scaffold-Dateien auf Repo-Root-Ebene entfernt (Program.cs, Properties/, appsettings*.json, witron-prep.csproj, obj/).
- README.md und `TUTORIAL_v1.2_ADS_Integration.md` auf den aktuellen Stand gebracht (WCF/CoreWCF ergänzt, Versionshistorie korrigiert, offener TODO-Punkt "WCF-Vergleich" abgehakt).
- Öffentliche Projektübersicht unter `docs/index.html` angelegt, vorgesehen für GitHub Pages — schlanker Auszug ohne interne Debugging-Details, für externe Betrachter.

### Stolperstein: `.gitignore` durch fehlenden Zeilenumbruch beschädigt (stiller Bug)

Beim Ergänzen eines neuen Eintrags in `.gitignore` endete die Datei zuvor ohne Zeilenumbruch am Dateiende. Der neue Eintrag wurde deshalb direkt an die letzte bestehende Zeile angehängt, ohne Zeilenumbruch dazwischen:

```
Port_*_boot.tizipSESSION_HANDOFF.md
SESSION_HANDOFF.md
```

Ergebnis: die Ignore-Regel `Port_*_boot.tizip` (für TwinCAT-Boot-Dateien) existierte de facto nicht mehr, weil das entstandene Muster auf keine reale Datei mehr passte — ohne dass Git dabei einen Fehler geworfen hätte. Erst ein gezielter Blick in `git diff` vor dem Taggen deckte es auf. Fix in eigenem, inhaltlich getrenntem Commit (`5bfe910`); der bereits lokal gesetzte Tag `v1.3` musste danach neu gesetzt werden (`git tag -d v1.3` + neu erstellen), da er sonst noch auf den fehlerhaften Stand gezeigt hätte — unproblematisch, weil der Tag zu diesem Zeitpunkt noch nicht gepusht war.

**Lehre:** das wiederkehrende Projekt-Thema "stille Bugs vermeiden" (siehe `AdsVerbunden`-Feld) gilt auch für Konfigurationsdateien wie `.gitignore` — ein `git diff` vor jedem Tag/Push lohnt sich, gerade bei Textdateien ohne Kompilierfehler-Sicherheitsnetz.

### Nächste Schritte

1. Geforcte Werte (`AI_FolienTemperatur`, `AI_MotorTemperatur`, `GVL_SIM_*`) wieder lösen (`Strg+F8`), bevor an realer Hardware weitergearbeitet wird.
2. Alle realen I/O-Verknüpfungen weiterhin systematisch prüfen und dokumentieren.
3. Offen, keine Festlegung nötig — zwei mögliche Richtungen: (a) EL3681-Hardwareintegration für echte Folien-/Motortemperatur-Messung, oder (b) Dashboard/Doku weiter verfeinern.

---

## 2026-08-09

**Referenzprojekt:** `PalettenStation_V2/TestV2` · **Zielsystem:** Beckhoff CP6606 / Windows CE7 / ARMV7 · **TwinCAT XAE:** Build 4022.36

### Neu

- Simulationsschalter `GVL_IO.SIM_Aktiv : BOOL := FALSE` implementiert. `MAIN` erweitert: acht bereits deklarierte, bis dahin ungenutzte lokale `bXxx`-Variablen werden zu Beginn jedes Zyklus je nach `SIM_Aktiv` entweder aus `GVL_IO.DI_*` (Hardware, Default) oder `GVL_Simulation.GVL_SIM_*` (Simulation) befüllt. Die komplette Schrittkette liest seither diese lokalen Variablen statt direkt der Hardware-Eingänge.
- Komplette Schrittkette einmal vollständig über Simulation durchgespielt: S0 → S1 → S2 → S3 → S4 → S8 (Alarm, da `AI_FolienTemperatur = 0` nicht simuliert und damit außerhalb 165–185) → S9 → zurück zu S0 (nach Force von `AI_FolienTemperatur = 175`, `AI_MotorTemperatur = 50`, `GVL_SIM_WerkerQuittierung = TRUE`).
- Bestätigt: `AI_FolienTemperatur`/`AI_MotorTemperatur` sind bewusst **nicht** Teil des Simulationsschalters — die EL3681 ist ja weiterhin nur für v1.4 geplant. Für Tests über S4 hinaus müssen die beiden Analogwerte einzeln geforct werden.
- Python-ADS-Sanity-Check erfolgreich: `ads_read_test.py` liest live `GVL_IO`-Werte via `pyads` direkt vom CP6606 (AMS Net ID `5.35.203.54.1.1`, Port 851) — Nachweis, dass ADS auch außerhalb von XAE und C# funktioniert.
- **CoreWCF-Demo-Service (`WcfDemo`) end-to-end verifiziert.** `PalettenStationService.GetStatus()` läuft unter `http://localhost:5289/PalettenStationService.svc` (BasicHttpBinding) und liefert per curl-Test einen sauberen SOAP-Envelope zurück — `GetStatusResponse`/`GetStatusResult` mit allen sechs `[DataMember]`-Feldern korrekt serialisiert.
- **Live-Dashboard für den SOAP-Service gebaut und verifiziert (`dashboard.html`).** Browserseite pollt `GetStatus()` automatisch (alle 2s), zeigt Schrittkette S0–S9, Alarm-Status und Analogwerte mit Trendpfeilen/Flash-Animation bei Änderung, plus aufklappbare rohe SOAP-Anfrage/Antwort. Ursprünglich über einen separaten lokalen Static-Server ausgeliefert — dafür musste CORS in `Program.cs` ergänzt werden (`AddCors` + `UseCors()`, vor `UseServiceModel`).
- **Architektur vereinfacht: `dashboard.html` nach `WcfDemo/wwwroot/` verschoben, `app.UseStaticFiles()` in `Program.cs` ergänzt.** Dashboard und SOAP-Service laufen jetzt aus demselben Kestrel-Prozess unter derselben Origin — exakt dasselbe Muster wie `SensorAPI/wwwroot/index.html`. Der separate Python-Server ist damit nicht mehr nötig.

### Stolperstein: externe Datei-Änderungen bei offenem XAE-Projekt

Wenn `.TcPOU`/`.TcGVL`-Dateien außerhalb von XAE geändert werden, während sie in XAE bereits geöffnet sind, merkt XAE die externe Änderung nicht automatisch und arbeitet mit der alten, im Speicher gehaltenen Version weiter. Ein anschließendes Speichern in XAE schreibt dann versehentlich wieder den alten Stand zurück, obwohl die Datei auf der Festplatte kurzzeitig aktueller war. Fix: betroffene Datei in XAE schließen und neu öffnen, bevor mit einer extern eingespielten Änderung weitergearbeitet wird — sonst debuggt man scheinbar korrekten Code, der in Wahrheit nie geladen wurde.

### Stolperstein: SOAP-Test per curl in PowerShell

Drei unabhängige Fallen auf dem Weg zum ersten erfolgreichen curl-Test gegen den CoreWCF-Service, der Reihe nach aufgetreten:

1. `curl` ist in PowerShell standardmäßig ein Alias für `Invoke-WebRequest` (andere Syntax, z.B. `-Headers`-Hashtable statt wiederholtem `-H`). Fix: `curl.exe` explizit aufrufen.
2. Multi-Zeilen-Paste in Windows Terminal löst einen Sicherheits-Bestätigungsdialog aus (`multiLinePasteWarning`) — kein Fehler, einfach bestätigen.
3. In *Windows PowerShell* (5.1, nicht PowerShell 7) gibt es den Encoding-Wert `utf8NoBOM` nicht, und `-Encoding UTF8` schreibt dort immer mit BOM. Sauberster Fix am Ende: den SOAP-Request-XML-Body direkt in einem Editor als Datei `request.xml` anlegen (UTF-8 ohne BOM) statt ihn per PowerShell-Heredoc zu erzeugen, und dann `curl.exe --data-binary "@request.xml"` verwenden. Zusätzlicher Stolperstein: PowerShell startete im geschützten Ordner `C:\WINDOWS\system32`, wo Schreibzugriff verweigert wird — einfach vorher in einen normalen Ordner wechseln (`cd`).

### Stolperstein: Live-Dashboard gegen SOAP-Service (CORS + lokale Static-Server-Kette)

1. `dashboard.html` **nicht** per Doppelklick (`file:///...`) öffnen — Firefox blockiert `fetch()`-Aufrufe von `file://`-Seiten zu `http://`-Adressen grundsätzlich, unabhängig von CORS. Fix: Datei über einen lokalen Static-Server ausliefern (`python -m http.server 8080`).
2. Beide Server müssen parallel in **getrennten** Terminal-Fenstern laufen. Wurde eines der beiden Fenster geschlossen bzw. wiederverwendet, brach die jeweils andere Verbindung ab.
3. Eigentliche CORS-Fehlermeldung erst über die Browser-Konsole (F12) sichtbar geworden: `OPTIONS`-Preflight-Request landete mit Status 400 und ohne `Access-Control-Allow-Origin`-Header direkt bei CoreWCF statt bei der CORS-Middleware — Ursache war, dass die lokale `Program.cs` noch die alte Version ganz ohne `AddCors`/`UseCors` war. Nach manuellem Nachtragen und komplettem Neustart von `dotnet run` (Hot-Reload reicht bei Middleware-Pipeline-Änderungen nicht zuverlässig) lief es.

**Lehre:** Bei "es tut nicht" im Browser lohnt sich fast immer zuerst ein Blick in die Konsole (F12) — die generische `fetch()`-Fehlermeldung ("NetworkError...") ist für Diagnosezwecke nutzlos, die Konsole zeigt den echten Grund.

### Nächste Schritte

1. Geforcte Werte wieder lösen (`Strg+F8`), bevor an realer Hardware weitergearbeitet wird.
2. Alle realen I/O-Verknüpfungen weiterhin systematisch prüfen und dokumentieren.
3. `PalettenStationService.GetStatus()` von Platzhalterdaten auf echte `AdsService`-Logik umstellen — nächster konkreter Schritt für die Bewerbung.

---

## 2026-08-08

**Referenzprojekt:** `PalettenStation_V2/TestV2` · **Zielsystem:** Beckhoff CP6606 / Windows CE7 / ARMV7 · **TwinCAT XAE:** Build 4022.36

### Neu

- **EL1002-Verknüpfung korrigiert.** `GVL_IO.DI_PaletteEingang` war zuvor auf `EL1004 Input 1` gemappt — umgelegt auf `Klemme 2 (EL1002) → Channel 1 → Input`. EL1002 reagiert danach korrekt im Prozessabbild.
- **ADS-Fehler 0x707 gelöst.** Ursache: veralteter/falscher Route-Eintrag für den SOYO-PC auf dem CP6606. Falsche Routen entfernt, korrekte Route neu angelegt, TwinCAT neu gestartet — PLC Port 851 danach wieder erreichbar. Details in `CP6606_ADS_0x707_Recovery.md`.
- **CASE-Schrittkette vollständig dokumentiert** (S0 Bereit → S1 Förderband → S2 Stopp → S3 Hub hoch → S4 Temp-Check → S5 Prozess/TON 5s → S6 Hub runter → S7 Abtransport → S0; Fehlerzweig S8/S9).

### Stolperstein: Build 4022.36 vs. 4024.75

Mit Build 4024.75 trat folgendes stilles Fehlerbild auf: EtherCAT scheinbar in OP, EL1002-LED reagierte physisch — Online-Prozesswert blieb jedoch dauerhaft 0. Nach Wechsel auf Build **4022.36** und Rebuild funktionierte der EL1002 wieder korrekt. Ursache vermutlich Version-Mismatch zwischen XAE und der Runtime 4022.29 auf dem CP6606.

**Lehre:** Wenn Hardware-LEDs reagieren, aber der Online-Wert in TwinCAT trotzdem 0 bleibt — zuerst XAE-Build-Version prüfen. Stilles Fehlerbild, kein Fehlerdialog.

### Stolperstein: Schreiben vs. Forcen bei Hardware-Eingängen

`%I*`-Variablen (Hardware-Eingänge) werden zyklisch vom EtherCAT-Prozessabbild überschrieben. "Werte schreiben" hält deshalb nicht dauerhaft. Für gezielten Test eines echten Eingangs muss **Forcen** verwendet werden — und nach dem Test vollständig wieder aufgehoben werden.

### Debugging: Breakpoints zur Sprungursache

Breakpoints in S3–S7 eingesetzt; damit konnte der unerwartete Sprung nach S8 auf `DI_KollisionFrei = FALSE` zurückgeführt werden.

### Nächste Schritte

1. Alle realen I/O-Verknüpfungen systematisch prüfen und dokumentieren.
2. CASE-Ablauf mit realen bzw. korrekt forcierten I/Os testen.
3. Diagnosevariablen / Fehlercodes ergänzen.
4. ADS-Kommunikation zwischen TwinCAT und C# aufbauen.
5. C#-HMI für Schritt, Sensoren, Aktoren, Temperaturen und Alarmzustand erstellen.

---

## 2026-07-23

### Erfolgreich abgeschlossen

- EtherCAT-Kommunikation stabil.
- PLC-Projekt TestV2 läuft.
- CASE-Schrittkette vollständig getestet (S0 bis S7 und Rückkehr nach S0).

### Wichtige Erkenntnisse

- `%I*`-Variablen sind Hardware-Eingänge.
- "Werte schreiben" wirkt bei Hardware-Eingängen nur kurz.
- Zum Testen realer Eingänge muss "Forcen" verwendet werden.
- Breakpoints erfolgreich zum Debuggen eingesetzt.

### Nächste Schritte

1. Diagnosevariablen ergänzen.
2. CASE-Code aufräumen.
3. ADS-Anbindung von C#.
4. HMI/Visualisierung.
