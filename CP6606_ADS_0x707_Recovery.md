# CP6606 – ADS 0x707 / Port 851 Recovery

**Projekt:** TwinCAT 3 – Palettenstation  
**Zielsystem:** Beckhoff CP6606 / Windows CE7 / ARMV7  
**TwinCAT XAE:** Build 4022.36  
**PLC Runtime:** Port 851  
**Datum:** 2026-08-08

## Fehlerbild

Beim Einloggen auf die PLC erschien:

```text
System not connected (Ads-Error 0x707 : Device is not in a ready state.)
```

Zusätzlich:

```text
Einloggen für Applikation 'Port_851' fehlgeschlagen.
```

Dabei waren gleichzeitig folgende Punkte in Ordnung:

- CP6606 erreichbar
- Ping erfolgreich
- FTP-Zugriff erfolgreich
- TwinCAT-Symbol am CP6606 grün
- EtherCAT-Master in OP
- alle EtherCAT-Klemmen in OP
- WcState = 0
- ADS-, IO- und PLC-Lizenzen gültig
- AMS Net ID des CP6606: `5.35.203.54.1.1`
- TwinCAT-Projekt mit Build 4022.36 geladen

## Tatsächliche Ursache

Die ADS-Routing-Konfiguration auf dem CP6606 enthielt einen falschen bzw. veralteten Route-Eintrag für den SOYO-Windows-PC.

Dadurch war die Rückroute vom CP6606 zum Engineering-PC nicht korrekt.

Obwohl Ping, FTP, EtherCAT und Teile der ADS-Kommunikation funktionierten, schlug der PLC-Login auf Port 851 mit ADS-Fehler 0x707 fehl.

## Lösung

1. ADS-Routen auf dem CP6606 kontrollieren.
2. Falschen/veralteten SOYO-Route-Eintrag entfernen.
3. Nicht mehr benötigte alte Routen entfernen.
4. Die korrekte ADS-Route zwischen Engineering-PC und CP6606 neu anlegen.
5. Prüfen:
   - korrekte IP-Adresse des SOYO-PCs
   - korrekte AMS Net ID des SOYO-PCs
   - CP6606 AMS Net ID: `5.35.203.54.1.1`
6. TwinCAT System neu starten.
7. CP6606 wieder in RUN bringen.
8. PLC → Einloggen.
9. Port 851 ist wieder erreichbar.

## Wichtige Erkenntnis

Bei ADS-Fehler `0x707` auf dem CP6606 nicht sofort:

- PLC-Projekt löschen
- EtherCAT neu scannen
- Lizenzen verdächtigen
- Klemmen neu konfigurieren

Stattdessen zuerst die ADS-Routen auf beiden Seiten prüfen, insbesondere die **Rückroute vom CP6606 zum Engineering-PC**.

## Zusätzliche Erkenntnisse

### Build-Version

Das Projekt muss mit:

```text
TwinCAT 3.1 Build 4022.36
```

geladen werden.

Mit Build 4024.75 war zwar eine Online-Verbindung möglich, aber der EL1002-Prozesswert wurde nicht korrekt aktualisiert.

Nach Laden und Neuübersetzen mit Build 4022.36 funktionierte:

```text
24 V → EL1002 → EtherCAT → TwinCAT
```

wieder korrekt.

### PLC Boot-Verzeichnis auf CP6606

```text
/TwinCAT/3.1/Boot/PLC/
```

Typische Dateien der ersten PLC-Runtime:

```text
Port_851.app
Port_851.bootdata
Port_851.bootdata-old
Port_851.cid
Port_851.crc
Port_851.occ
Port_851.oce
Port_851.ocm
Port_851_act.tizip
Port_851_boot.tizip
```

### Lokale Boot-Dateien im XAE-Projekt

Nach Rebuild mit Build 4022.36:

```text
TestV2/_Boot/TwinCAT CE7 (ARMV7)/Plc/
```

mit u. a.:

```text
Port_851/
Port_851.app
Port_851.autostart
Port_851.cid
Port_851.crc
Port_851.occ
Port_851.ocm
Port_851_boot.tizip
```

## Kurzdiagnose für das nächste Mal

Wenn erneut `ADS 0x707` auftritt:

```text
1. Ist CP6606 erreichbar?
2. Ping OK?
3. FTP OK?
4. TwinCAT grün?
5. EtherCAT OP?
6. Build wirklich 4022.36?
7. ADS-Route PC → CP korrekt?
8. ADS-Rückroute CP → PC korrekt?
9. Erst danach Port 851 / PLC Boot untersuchen.
```

**Merksatz:**  
> Wenn EtherCAT läuft, Ping und FTP funktionieren, aber PLC Port 851 mit 0x707 scheitert, zuerst die ADS-Routen in beide Richtungen prüfen.
