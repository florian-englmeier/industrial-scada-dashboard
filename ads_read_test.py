"""
ADS-Sanity-Check fuer die PalettenStation (CP6606 / CP-23CB36)
================================================================

Ziel: von Python aus (deine Sprache) direkt SPS-Variablen ueber ADS lesen,
ohne XAE. Gleiches Protokoll wie die C#-SensorAPI (AdsService.cs), nur
mit pyads statt Beckhoff.TwinCAT.Ads.

WICHTIG: Dieses Skript muss auf deinem Dev-PC "SOYO" laufen (nicht hier
in der Cloud-Sandbox) -- nur dort laeuft der TwinCAT-Router mit der
eingetragenen Route zum CP-23CB36. Aus der Cloud-Sandbox kann ich dein
lokales 192.168.1.x-Netz nicht erreichen.

Voraussetzung:
    pip install pyads

Variablen stammen aus GVL_IO.TcGVL (PallettenStation_v2, aktueller Stand
v0.10.3, laeuft laut Commit-Log real auf dem CP6606).
"""

import pyads

# CP-23CB36 -- siehe CP-Reset.md
AMS_NET_ID = "5.35.203.54.1.1"
ADS_PORT = 851  # Port der PLC-Runtime (nicht der Router-Port!)


def main() -> None:
    plc = pyads.Connection(AMS_NET_ID, ADS_PORT)

    with plc:  # oeffnet automatisch, schliesst automatisch (auch bei Exception)
        # -- Schrittkette & Alarm (INT/BOOL) --
        schritt = plc.read_by_name("GVL_IO.Schritt", pyads.PLCTYPE_INT)
        alarm_aktiv = plc.read_by_name("GVL_IO.AlarmAktiv", pyads.PLCTYPE_BOOL)

        # -- Analoge Eingaenge (REAL = 32-Bit-Float) --
        temp_folie = plc.read_by_name("GVL_IO.AI_FolienTemperatur", pyads.PLCTYPE_REAL)
        temp_motor = plc.read_by_name("GVL_IO.AI_MotorTemperatur", pyads.PLCTYPE_REAL)

        # -- ein paar digitale Eingaenge zum Gegenchecken --
        nothalt_frei = plc.read_by_name("GVL_IO.DI_NotHaltFrei", pyads.PLCTYPE_BOOL)
        schutzhaube_zu = plc.read_by_name("GVL_IO.DI_SchutzhaubeZu", pyads.PLCTYPE_BOOL)

        print(f"Schritt:            {schritt}")
        print(f"AlarmAktiv:         {alarm_aktiv}")
        print(f"AI_FolienTemperatur:{temp_folie:.2f}")
        print(f"AI_MotorTemperatur: {temp_motor:.2f}")
        print(f"DI_NotHaltFrei:     {nothalt_frei}")
        print(f"DI_SchutzhaubeZu:   {schutzhaube_zu}")


if __name__ == "__main__":
    main()
