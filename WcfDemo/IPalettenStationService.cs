using System.ServiceModel;

namespace WcfDemo;

// [ServiceContract] ist der WCF-Vertrag - das SOAP-Aequivalent zu einem Interface,
// das dein REST-Controller (SensorController.cs) implizit über die HTTP-Routen definiert.
// Der Unterschied: hier ist der Vertrag EXPLIZIT und maschinenlesbar (WSDL),
// nicht nur "was auch immer der Controller gerade zurückgibt".
[ServiceContract]
public interface IPalettenStationService
{
    // [OperationContract] markiert eine Methode als Teil des Service-Vertrags.
    // Entspricht in etwa einem [HttpGet]-Endpoint in ASP.NET Core,
    // nur dass der Client hier einen typisierten Proxy generiert bekommt
    // statt selbst JSON zu parsen.
    [OperationContract]
    PalettenStationStatus GetStatus();
}
