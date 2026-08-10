using TwinCAT.Ads;

namespace WcfDemo;

public class AdsService : IDisposable
{
    private readonly AdsClient _client;
    private readonly string _amsNetId = "5.35.203.54.1.1";  // CP6606
    private readonly int _port = 851;                       // TwinCAT 3 PLC Runtime 1

    public bool IsConnected { get; private set; }

    public AdsService()
    {
        _client = new AdsClient();
        try
        {
            _client.Connect(_amsNetId, _port);
            IsConnected = _client.IsConnected;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ADS-Verbindung fehlgeschlagen: {ex.Message}");
            IsConnected = false;
        }
    }

    // Test-Read: liest den TwinCAT-State (funktioniert IMMER wenn Verbindung steht)
    public string GetPlcState()
    {
        if (!IsConnected) return "Nicht verbunden";
        try
        {
            var state = _client.ReadState();
            return $"AdsState: {state.AdsState}, DeviceState: {state.DeviceState}";
        }
        catch (Exception ex)
        {
            return $"Fehler: {ex.Message}";
        }
    }

    // Liest eine BOOL-Variable aus der SPS
    public bool ReadBool(string symbolPath)
    {
        if (!IsConnected) return false;
        try
        {
            return (bool)_client.ReadValue(symbolPath, typeof(bool));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ReadBool({symbolPath}) fehlgeschlagen: {ex.Message}");
            return false;
        }
    }

    // Liest eine REAL-Variable (32 Bit Float) aus der SPS
    public float ReadReal(string symbolPath)
    {
        if (!IsConnected) return 0f;
        try
        {
            return (float)_client.ReadValue(symbolPath, typeof(float));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ReadReal({symbolPath}) fehlgeschlagen: {ex.Message}");
            return 0f;
        }
    }

    // Liest eine INT-Variable (16 Bit) aus der SPS
    public short ReadInt(string symbolPath)
    {
        if (!IsConnected) return 0;
        try
        {
            return (short)_client.ReadValue(symbolPath, typeof(short));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ReadInt({symbolPath}) fehlgeschlagen: {ex.Message}");
            return 0;
        }
    }


    public void Dispose()
    {
        _client?.Dispose();
    }
}