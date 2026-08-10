using System.Runtime.Serialization;

namespace WcfDemo;

// [DataContract] ist das WCF-Aequivalent zu einer C#-Klasse, die ASP.NET Core
// automatisch nach JSON serialisiert (wie euer anonymer Typ in SensorController.GetLive()).
// Bei WCF ist die Serialisierung nach XML/SOAP explizit über [DataMember] markiert -
// nur was hier steht, geht ueber den Draht. Absichtlich dieselben Felder wie euer
// bestehender /api/sensor/live-Endpoint, damit der Vergleich REST vs. WCF greifbar ist.
[DataContract]
public class PalettenStationStatus
{
    [DataMember]
    public int Schritt { get; set; }

    [DataMember]
    public string SchrittText { get; set; } = string.Empty;

    [DataMember]
    public double FolienTemperatur { get; set; }

    [DataMember]
    public double MotorTemperatur { get; set; }

    [DataMember]
    public bool AlarmAktiv { get; set; }

    [DataMember]
    public string Zeitstempel { get; set; } = string.Empty;
}
