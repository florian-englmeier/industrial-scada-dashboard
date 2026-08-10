namespace WcfDemo;

// Die eigentliche Implementierung des Vertrags. Fuers erste Kennenlernen
// bewusst mit Platzhalterdaten statt echter ADS-Anbindung - der Punkt hier
// ist WCF selbst zu verstehen, nicht nochmal die SPS-Anbindung zu wiederholen.
// Sobald der Service laeuft, koennen wir GetStatus() gegen eure bestehende
// AdsService-Logik aus SensorAPI verdrahten.
public class PalettenStationService : IPalettenStationService
{
    public PalettenStationStatus GetStatus()
    {
        return new PalettenStationStatus
        {
            Schritt = 5,
            SchrittText = "S5 - Bearbeitung laeuft",
            FolienTemperatur = 175.2,
            MotorTemperatur = 48.7,
            AlarmAktiv = false,
            Zeitstempel = DateTime.Now.ToString("HH:mm:ss")
        };
    }
}
