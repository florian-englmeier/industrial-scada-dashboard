using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using WcfDemo;

var builder = WebApplication.CreateBuilder(args);

// CoreWCF-Dienste registrieren - das Aequivalent zu builder.Services.AddControllers()
// in eurer SensorAPI, nur fuer das SOAP-Programmiermodell statt REST.
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

// CORS: der Live-Dashboard-Client (dashboard.html) laeuft im Browser unter einer
// ANDEREN Origin als der Service (eigener lokaler Static-Server statt localhost:5289).
// Ohne CORS wuerde der Browser den fetch()-Aufruf blockieren, bevor er den Service
// ueberhaupt erreicht - das ist reine Browser-Sicherheitspolitik, kein WCF-Thema.
// AllowAnyOrigin() ist fuer diese lokale Demo ok; in einer echten Produktivumgebung
// wuerde man hier die konkrete erlaubte Origin eintragen statt "alle erlauben".
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Muss VOR UseServiceModel stehen: die CORS-Middleware beantwortet den
// Preflight-OPTIONS-Request (den der Browser wegen Content-Type "text/xml" und
// dem SOAPAction-Header automatisch vorausschickt) direkt selbst, ohne dass
// die Anfrage ueberhaupt bei WCF ankommt.
app.UseCors();

// Den Service unter einer Adresse verfuegbar machen und die Bindung festlegen.
// BasicHttpBinding = klassisches SOAP 1.1 ueber HTTP, das haeufigste WCF-Binding
// und das, was am ehesten in bestehenden Enterprise-Systemen auftaucht.
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<PalettenStationService>();
    serviceBuilder.AddServiceEndpoint<PalettenStationService, IPalettenStationService>(
        new BasicHttpBinding(), "/PalettenStationService.svc");
});

// WSDL-Metadaten einschalten, damit der Service unter ?wsdl seinen eigenen
// Vertrag maschinenlesbar beschreibt - das ist der Kernunterschied zu REST,
// wo es kein eingebautes Vertrags-Dokument gibt (OpenAPI/Swagger ist der
// nachtraeglich draufgesetzte Ersatz dafuer).
var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
serviceMetadataBehavior.HttpGetEnabled = true;

app.Run();
