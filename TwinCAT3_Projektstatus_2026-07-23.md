# TwinCAT 3 -- Projektstatus (Update)

## Stand

**Datum:** 2026-07-23

## Erfolgreich abgeschlossen

-   EtherCAT-Kommunikation stabil.
-   PLC-Projekt TestV2 läuft.
-   CASE-Schrittkette vollständig getestet (S0 bis S7 und Rückkehr nach
    S0).

## Wichtige Erkenntnisse

-   %I\*-Variablen sind Hardware-Eingänge.
-   'Werte schreiben' wirkt bei Hardware-Eingängen nur kurz.
-   Zum Testen realer Eingänge muss 'Forcen' verwendet werden.
-   Breakpoints erfolgreich zum Debuggen eingesetzt.

## Nächste Schritte

1.  Diagnosevariablen ergänzen.
2.  CASE-Code aufräumen.
3.  ADS-Anbindung von C#.
4.  HMI/Visualisierung.
