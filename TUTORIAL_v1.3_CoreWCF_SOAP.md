# TUTORIAL Kapitel v1.3 — CoreWCF: SOAP-Services verstehen

**Anschluss an:** TUTORIAL_v1.2_ADS_Integration.md
**Stand:** August 2026
**Umgebung:** Nativer Windows-PC, Visual Studio / `dotnet` CLI, .NET 9, CoreWCF.Primitives 1.6.0, CoreWCF.Http 1.6.0
**Ziel:** Verstehen, wie ein SOAP/WCF-Service funktioniert und sich von REST unterscheidet — eine Anforderung, die in Stellenprofilen für Enterprise-/Intralogistik-Rollen häufig auftaucht.

> **Warum das Kapitel:** Viele Stellenanzeigen in diesem Bereich nennen WCF explizit als Anforderung. SensorAPI (v1.0–v1.2) ist komplett REST/JSON. Dieses Kapitel baut denselben `GetStatus()`-Gedanken einmal als SOAP-Service nach, damit der Vergleich REST vs. WCF greifbar wird statt nur auswendig gelernt.

---

## Inhaltsverzeichnis

1. [REST vs. SOAP — zwei Philosophien](#1-rest-vs-soap--zwei-philosophien)
2. [ServiceContract statt Controller](#2-servicecontract-statt-controller)
3. [DataContract statt impliziter JSON-Serialisierung](#3-datacontract-statt-impliziter-json-serialisierung)
4. [Program.cs: den Service verdrahten](#4-programcs-den-service-verdrahten)
5. [WSDL statt Swagger/OpenAPI](#5-wsdl-statt-swaggeropenapi)
6. [Testen mit curl — die PowerShell-Falle](#6-testen-mit-curl--die-powershell-falle)
7. [Was du gelernt hast](#7-was-du-gelernt-hast)
8. [Was noch offen ist](#8-was-noch-offen-ist)

---

## 1. REST vs. SOAP — zwei Philosophien

SensorAPI (`SensorController.GetLive()`) ist REST: eine HTTP-Route (`GET /api/sensor/live`), die JSON zurückgibt. Der "Vertrag" existiert nur implizit — man sieht ihn am Code oder an einer nachträglich draufgesetzten Swagger-Doku.

SOAP/WCF dreht das um: der Vertrag ist **zuerst da und maschinenlesbar**. Ein Client kann den Vertrag (WSDL) lesen und weiß exakt, welche Operationen es gibt, welche Parameter sie erwarten und welche Typen zurückkommen — ganz ohne die Doku zu lesen oder den Server-Code zu kennen.

| | REST (SensorAPI) | SOAP (WcfDemo) |
|---|---|---|
| Transport | HTTP, meist JSON | HTTP, immer XML (SOAP-Envelope) |
| Vertrag | implizit, über Routen | explizit, `[ServiceContract]` → WSDL |
| Aufruf | `GET /api/sensor/live` | `POST` mit SOAP-Envelope + `SOAPAction`-Header |
| Typisierung | locker (JSON) | strikt (`[DataContract]`, XML-Schema) |
| Typischer Einsatz | moderne Web-APIs, Microservices | Enterprise-Integration, Industrieanlagen, Altsysteme |

**Warum das in der Praxis relevant ist:** Große, gewachsene Lagerlogistik-Landschaften haben oft SOAP-Services aus den 2000er/2010er-Jahren im Bestand, die nicht einfach auf REST umgeschrieben werden. Wer dort mitarbeitet, muss beides lesen und anbinden können.

---

## 2. ServiceContract statt Controller

### IPalettenStationService.cs

```csharp
using System.ServiceModel;

namespace WcfDemo;

[ServiceContract]
public interface IPalettenStationService
{
    [OperationContract]
    PalettenStationStatus GetStatus();
}
```

`[ServiceContract]` markiert das Interface als WCF-Vertrag — das SOAP-Äquivalent zu dem, was dein `SensorController` implizit über seine `[HttpGet]`-Routen definiert. Der Unterschied: hier ist der Vertrag ein eigenständiges Interface, von der Implementierung getrennt. `[OperationContract]` markiert jede Methode, die Teil des Vertrags sein soll — nur was hier markiert ist, landet später in der WSDL und ist von außen aufrufbar.

Das ist ein Muster, das dir aus C# generell bekannt vorkommen sollte: **Interface + Implementierung trennen**. WCF macht daraus einfach den offiziellen Netzwerk-Vertrag.

### PalettenStationService.cs

```csharp
public class PalettenStationService : IPalettenStationService
{
    public PalettenStationStatus GetStatus()
    {
        return new PalettenStationStatus { ... };
    }
}
```

Ganz normale Interface-Implementierung — nichts WCF-Spezifisches mehr an dieser Stelle. Der nächste Schritt (siehe "Was noch offen ist") ist, hier `GetStatus()` genau wie `SensorController.GetLive()` gegen die bestehende `AdsService`-Logik zu verdrahten, statt Platzhalterdaten zurückzugeben.

---

## 3. DataContract statt impliziter JSON-Serialisierung

### PalettenStationStatus.cs

```csharp
using System.Runtime.Serialization;

[DataContract]
public class PalettenStationStatus
{
    [DataMember]
    public int Schritt { get; set; }

    [DataMember]
    public string SchrittText { get; set; } = string.Empty;
    // ... FolienTemperatur, MotorTemperatur, AlarmAktiv, Zeitstempel
}
```

Bei ASP.NET Core REST reicht ein `new { Schritt = 5, ... }` (anonymer Typ, siehe v1.2) — der JSON-Serializer packt automatisch **alle** öffentlichen Properties ein. Bei WCF ist das Gegenteil der Default: **nichts** wird serialisiert, außer es ist explizit mit `[DataMember]` markiert. `[DataContract]` markiert die Klasse selbst als serialisierbar.

**Merksatz:** REST/JSON ist *opt-out* (alles drin, außer man blendet was aus), WCF/SOAP ist *opt-in* (nichts drin, außer man markiert es explizit). Das ist bewusst so — in Enterprise-Umgebungen will man nie versehentlich ein internes Feld über den Draht schicken, das nicht Teil des offiziellen Vertrags ist.

---

## 4. Program.cs: den Service verdrahten

```csharp
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

var app = builder.Build();

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<PalettenStationService>();
    serviceBuilder.AddServiceEndpoint<PalettenStationService, IPalettenStationService>(
        new BasicHttpBinding(), "/PalettenStationService.svc");
});
```

Drei Zeilen, drei Konzepte:

- **`AddServiceModelServices()`** — das WCF-Laufzeitsystem selbst registrieren. Entspricht `builder.Services.AddControllers()` bei REST.
- **`AddService<PalettenStationService>()`** — *welche* Klasse gehostet wird.
- **`AddServiceEndpoint<Impl, Vertrag>(Binding, Adresse)`** — *wie* sie erreichbar ist. Drei Teile, die man in WCF klassisch das "ABC" nennt: **A**ddress (`/PalettenStationService.svc`), **B**inding (`BasicHttpBinding` — klassisches SOAP 1.1 über HTTP), **C**ontract (`IPalettenStationService`).

`BasicHttpBinding` ist die einfachste, kompatibelste Bindung — kein WS-Security, keine Sessions, einfach SOAP über HTTP. Genau das, was man in einer bestehenden Enterprise-Landschaft am ehesten antrifft.

---

## 5. WSDL statt Swagger/OpenAPI

```csharp
var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
serviceMetadataBehavior.HttpGetEnabled = true;
```

Damit beschreibt der Service sich selbst maschinenlesbar unter `/PalettenStationService.svc?wsdl` — das ist die direkte Entsprechung zu `/swagger` bei REST, nur dass WSDL fester Bestandteil von WCF ist und nicht wie Swagger nachträglich draufgesetzt wurde. Ein Client (egal welche Sprache) kann aus dieser WSDL automatisch einen typisierten Proxy generieren (`svcutil` unter Windows) — man muss nie von Hand XML zusammenbauen, um den Service aufzurufen.

---

## 6. Testen mit curl — die PowerShell-Falle

Ohne generierten Client testet man SOAP von Hand: ein `POST` mit rohem XML-Body und einem `SOAPAction`-Header, der angibt, welche Operation gemeint ist.

```powershell
curl.exe -X POST http://localhost:5289/PalettenStationService.svc `
  -H "Content-Type: text/xml; charset=utf-8" `
  -H "SOAPAction: http://tempuri.org/IPalettenStationService/GetStatus" `
  -d '<?xml version="1.0" encoding="utf-8"?><soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"><soap:Body><GetStatus xmlns="http://tempuri.org/"/></soap:Body></soap:Envelope>'
```

**Stolperstein:** In PowerShell ist `curl` standardmäßig ein Alias für `Invoke-WebRequest` — ein komplett anderer Befehl mit anderer Syntax (Header z.B. als Hashtable über `-Headers`, nicht als wiederholtes `-H`). Das echte `curl.exe` (bei Windows 10/11 immer mitinstalliert, unabhängig vom Alias) über den expliziten Dateinamen `curl.exe` statt `curl` aufrufen umgeht das zuverlässig.

Die `SOAPAction` `http://tempuri.org/IPalettenStationService/GetStatus` ergibt sich aus dem Default-Namespace `tempuri.org` (weil in `[ServiceContract]` kein `Namespace` gesetzt wurde, siehe Kapitel 2) plus Interface- und Methodenname.

---

## 7. Was du gelernt hast

Nach v1.3 kannst du im Interview sagen:

> *"Einen SOAP/WCF-Service baue ich mit CoreWCF auf ASP.NET Core: der Vertrag ist ein Interface mit `[ServiceContract]`/`[OperationContract]`, die Datenklassen sind explizit mit `[DataContract]`/`[DataMember]` markiert — im Gegensatz zu REST/JSON ist das opt-in statt opt-out. Verdrahtet wird über Address/Binding/Contract, üblicherweise `BasicHttpBinding` für klassisches SOAP 1.1. Der Service beschreibt sich selbst über eine WSDL — das WCF-Äquivalent zu Swagger bei REST, nur vertraglich fest eingebaut statt nachträglich ergänzt."*

Drei Konzepte sind jetzt verdrahtet:

1. **Explizite Verträge** — `[ServiceContract]`/`[OperationContract]` statt impliziter Routen
2. **Opt-in-Serialisierung** — `[DataContract]`/`[DataMember]` statt "alles geht automatisch raus"
3. **ABC-Prinzip** — Address, Binding, Contract als die drei Bausteine jedes WCF-Endpunkts

---

## 8. Was noch offen ist

Für kommende Versionen:

- [ ] **`GetStatus()` echt verdrahten** — Platzhalterdaten durch die bestehende `AdsService`-Logik aus SensorAPI ersetzen (gleiche ADS-Verbindung zum CP6606, nur SOAP statt REST als Außenhülle)
- [ ] **Fault Contracts** — WCF-Äquivalent zu HTTP-Statuscodes/Exceptions bei REST, für sauberes Fehlerhandling über SOAP
- [ ] **Client-seitig testen** — `svcutil` gegen die WSDL laufen lassen und einen echten typisierten C#-Client generieren, statt nur curl mit rohem XML
- [ ] **Build tatsächlich verifizieren** — läuft bisher nur gegen manuelle Code-Review (keine Compiler-Verfügbarkeit in der Cloud-Sandbox); auf dem lokalen Windows-PC mit `dotnet run` gegenprüfen

---

> *"REST ist die Sprache, die man heute lernt. SOAP ist die Sprache, die in den Anlagen steht, die schon laufen, bevor man geboren wurde."*
