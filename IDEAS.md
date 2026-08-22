# IDEAS.md

> **Zweck:** Sammelstelle für alle Ideen die während der Entwicklung auftauchen, aber nicht sofort umgesetzt werden. Vermeidet dass gute Gedanken verschwinden.
> **Stand:** Juli 2026, nach v1.2.0

---

## Zwei parallele Entwicklungstracks

Nach v1.2.0 (ADS-Live-Integration) gibt es zwei sinnvolle Richtungen. Beide führen zu einem stärkeren Portfolio, wobei Track B die Bedienerfreundlichkeit adressiert und Track A die Anlagentiefe.

### Track A — Anlagentechnik vertiefen
- v1.3.0 EL3681 Integration (Spannung/Strom messen)
- v1.4.0 Predictive Maintenance mit Trendlog

Details in `TUTORIAL_v1.0_TwinCAT_Roadmap.md`, Kapitel 18.

### Track B — HMI und Bedienerfreundlichkeit
Der nächste Fokus. Details siehe unten.

---

## Track B — HMI-Entwicklung im Detail

### Ausgangslage

Der aktuelle Endpoint `/api/sensor/live` liefert Rohdaten als JSON:

```json
{"schritt": 5, "folienTemperatur": 175.2, "motorTemperatur": 48.7, "alarmAktiv": false}
```

Ein Werker soll aber nicht JSON lesen. Er soll auf einen Blick verstehen:

- Läuft die Anlage normal?
- Wenn Alarm: welcher, wo, was tun?
- Wo im Prozess sind wir gerade?

Genau das ist die HMI-Aufgabe. Und genau da liegt der Kern der Intralogistik-Branche: **Software für tausende Werker, nicht für Ingenieure**.

---

### Feature-Cluster 1: Live-Visualisierung

**Analogwerte als Skala statt Zahl**
- Balken oder Tacho für FolienTemperatur (0-250 °C), MotorTemperatur (0-150 °C)
- Grenzwerte als Markierungen sichtbar (grüner Bereich: 165-185, rot außerhalb)
- Bei Grenzverletzung: Balken wechselt Farbe
- Aktueller Zahlenwert klein unter dem Balken

**Farbcodierung nach ISO-Konvention**
- Grün: normaler Betrieb
- Gelb: Warnung, Grenzwert nahe
- Rot: Alarm, Grenze überschritten
- Grau: Wert nicht verfügbar (ADS nicht verbunden)

**Schrittkette visualisieren**
- Grafische Darstellung der Anlage statt "Schritt 5"
- Sensor-LEDs an entsprechenden Positionen (Palette am Eingang → LED grün)
- Aktive Schrittstellung hervorgehoben
- Idealerweise ein simples SVG-Schema mit den 10 Zuständen

**Live-Uhrzeit + Anlagenlaufzeit**
- Aktuelle Zeit sichtbar
- Anlage läuft seit: 03:47:12
- Aktuelle Zykluszeit im S5-Prozess

---

### Feature-Cluster 2: Alarm-Handling und Bedienerinteraktion

**Alarm-Popup mit vollem Kontext**
- Wenn `AlarmAktiv = true`: modaler Dialog erscheint
- Anzeige: welcher Alarm, welcher Sensor, welcher Wert
- Zeitstempel wann der Alarm auftrat
- Vorschlag was zu prüfen ist

**Quittierung mit Kommentar**
- Werker gibt Namen ein (später: Login/RFID)
- Dropdown mit vordefinierten Ursachen:
  - Blockade beseitigt
  - Werkzeug getauscht
  - Temperatur außerhalb Toleranz (Heizung)
  - Sensor verschmutzt
  - Sonstiges (Freitext)
- Optional Freitext-Kommentar
- Speicherung als Vorgang mit Ticket-ID (bereits geplant in älterer Roadmap)

**Werker-Identifikation**
- Vorerst: Textfeld für Namen
- Später: Login mit Token
- Ganz später: RFID-Reader über Beckhoff-Klemme?

**Persistierung der Alarm-Historie**
- Zunächst CSV: `zeitstempel;alarm;wert;quittierer;kommentar`
- Später: SQLite oder PostgreSQL via Entity Framework
- Nutzt später v1.4.0 Trendlog-Infrastruktur mit

**Alarm-Historie einsehen**
- Neuer Endpoint `/api/alarms/history`
- Filter: Zeitraum, Sensor, Werker
- Anzeige als Tabelle im Dashboard
- Nützlich für Schichtübergabe und Ursachenanalyse

---

### Feature-Cluster 3: Bedienbarkeit der Anlage vom Dashboard aus

**Write-Endpoint für die SPS**
- `POST /api/sensor/reset` — Schrittkette auf S0 zurücksetzen (nur wenn Alarm quittiert)
- `POST /api/sensor/simulation` — SIM_Aktiv umschalten
- `POST /api/sensor/grenzwerte` — Grenzwerte anpassen (Temperatur-Range zum Beispiel)

**Vor Ausführung: Bestätigung**
- Alle schreibenden Aktionen: "Wollen Sie wirklich?"
- Vermeidet versehentliches Anlagenrücksetzen
- Loggt wer wann was geschaltet hat

**Berechtigungslevel** (später)
- Werker: quittieren, kommentieren
- Instandhaltung: zurücksetzen, Grenzwerte
- Admin: alles inkl. Konfiguration

---

### Feature-Cluster 4: Übergreifende UX-Verbesserungen

**Sound bei Alarm**
- Kurzes akustisches Signal wenn `AlarmAktiv` von false auf true wechselt
- Werker im Nebenraum bekommt's mit

**Toast-Nachrichten**
- Kurze Einblendungen bei erfolgreicher Quittierung
- Bei erfolgreicher Aktion: "Reset erfolgreich"

**Dark Mode toggeln**
- Aktuell nur Dark (industrieller Look)
- Für Büro-Ansicht: Light-Mode als Option

**Responsive Design**
- Aktuell nur Desktop
- Werker haben oft Tablets: Layout muss auch auf 10" funktionieren

**Multi-Language**
- Deutsch (Standard)
- Englisch für internationale Standorte
- Fokus: klare Alarm-Texte in Landessprache

---

### Feature-Cluster 5: Erweiterungen die tiefer gehen

**Digitaler Zwilling der Anlage**
- SVG-Animation die den aktuellen Anlagenzustand zeigt
- Palette bewegt sich synchron zum echten Prozess
- Hubwerk fährt hoch/runter je nach Signal
- Für Interview: absolute Killer-Demo

**Historische Trends**
- Chart.js-Trendkurve für Temperaturen der letzten Stunde/Tag/Woche
- Nutzt v1.4.0 CSV/DB-Infrastruktur
- Werker sieht: "die Motortemperatur klettert seit 2 Stunden"

**Predictive Alerts** (verbindet Track A und B)
- Nicht nur "Alarm jetzt", sondern "Trend deutet auf Ausfall in 20 Min hin"
- Nutzt Diagnose-Regeln aus v1.4.0

**Multi-Anlagen-Ansicht**
- Später: mehrere PalettenStationen im Blick
- Übersichts-Dashboard mit Status pro Anlage
- Drilldown in einzelne Anlage bei Bedarf

---

## Priorisierungs-Vorschlag für nächste Sessions

### v1.3.0 — Live-Visualisierung (Grundstein von Track B)
1. Schrittkette als SVG-Grafik
2. Analogwerte als Balken mit Grenzen
3. Farbcodierung nach Zustand
4. Live-Uhrzeit und Laufzeit

**Warum als erstes:** Reine Frontend-Arbeit, kein neuer SPS-Code nötig. Sofort sichtbarer Fortschritt.

### v1.4.0 — Alarm-Handling
1. Alarm-Popup bei AlarmAktiv=true
2. Quittierung mit Kommentar und Dropdown
3. Werker-Name (Textfeld)
4. CSV-Persistierung der Vorgänge

**Warum als zweites:** Baut auf v1.3.0 Frontend auf, ergänzt um Interaktion.

### v1.5.0 — Write-Endpoints
1. Reset-Endpoint
2. Grenzwert-Anpassung
3. Simulation-Toggle

**Warum drittes:** Braucht ADS-Write (neu), sollte nicht ohne Alarm-Handling passieren.

### v1.6.0+ — Track A wieder aufnehmen
Zurück zu EL3681 und Predictive Maintenance. Bis dahin ist die HMI-Basis gelegt, die neuen Messwerte lassen sich direkt visualisieren.

---

## Kleinere Ideen die nicht verloren gehen sollen

- Test-Suite: automatische Tests für SPS-Logik (später mit TwinCAT-Test-Framework)
- Deployment: Docker-Container für die SensorAPI
- CI/CD: GitHub Actions die bei jedem Push die API bauen
- OpenAPI/Swagger: für die REST-API mit `AddOpenApi()` sowieso schon halb da
- Rate-Limiting: Endpoints vor Missbrauch schützen
- CORS: für Anbindung von externen Frontends
- Logging in Datei/Cloud: statt nur Console.WriteLine
- Metrics und Monitoring: Prometheus + Grafana? (in der Intralogistik-Branche üblich)

---

## Notizen für später

**WCF-Vergleich einbauen** — irgendwo in den Tutorials erklären warum ADS strukturell wie ein WCF-Service funktioniert. Direkter Bezug zu Stellenanzeigen, die WCF/SOAP explizit fordern (in der Enterprise-/Intralogistik-Welt weit verbreitet).

---

> *"Ideen sind flüchtig. Aufschreiben rettet sie. Priorisieren macht sie umsetzbar."*
