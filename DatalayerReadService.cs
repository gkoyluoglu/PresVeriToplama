using Datalayer;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using PresVeriToplama.Common;

namespace PresVeriToplama;

public class DatalayerReadService : BackgroundService
{
    private readonly ILogger<DatalayerReadService> _logger;
    private DatalayerSystem _system;
    private readonly string? _bucket;
    private readonly string? _logDataBucket;
    private readonly string? _organization;
    private readonly InfluxDBClient _influxDbClient;
    private readonly IClient _client;
    private readonly string? _rotusTipi;
    Random rnd = new Random();

    private int _tabakAl = 0;
    private int _tabakDustu = 0;
    private int _fire = 0;
    private int _uretim = 0;

    public DatalayerReadService(ILogger<DatalayerReadService> logger, IConfiguration conf)
    {
        _logger = logger;
        _system = new DatalayerSystem();
        _system.Start(startBroker: false);

        _bucket = conf.GetValue<string>("InfluxDB:Bucket");
        _logDataBucket = conf.GetValue<string>("InfluxDB:LogBucket");
        _organization = conf.GetValue<string>("InfluxDB:Organization");

        var token = conf.GetValue<string>("InfluxDB:Token");
        var url = conf.GetValue<string>("InfluxDB:URL");
        var portInflux = conf.GetValue<string>("InfluxDB:Port");

        var ipCore = conf.GetValue<string>("ctrlXCore:ip");
        var portCore = conf.GetValue<int>("ctrlXCore:port");

        _rotusTipi = conf.GetValue<string>("pres:rotusTipi");

        var remote = new Remote(ip: ipCore, sslPort: portCore).ToString();

        _client = _system.Factory.CreateClient(remote);
        _influxDbClient = new InfluxDBClient("http://" + url + ":" + portInflux, token);
        if (!_client.IsConnected)
        {
            // Initially exit and retry after app restart-delay (see snapcraft.yaml)
            Console.WriteLine($"Client is not connected -> exit");
            return;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                ReadDatalayer();
            }

            await Task.Delay(3000, stoppingToken);
        }
    }

    private async void ReadDatalayer()
    {
        var currentDate = DateTime.Now;
        var writeApi = _influxDbClient.GetWriteApiAsync();
        var prevTabakAl = _tabakAl;
        _tabakDustu++;
        _tabakAl = (rnd.Next(100) < 98 ? _tabakAl + 1 : _tabakAl);
        _uretim = (prevTabakAl < _tabakAl ? (rnd.Next(100) < 95 ? _uretim + 1 : _uretim) : _uretim);
        _fire = _tabakAl - _uretim;

        Console.WriteLine($"Tabak Dustu:{_tabakDustu}\n" +
                          $"Tabak Al: {_tabakAl}\n" +
                          $"Uretim: {_uretim}\n" +
                          $"Fire Rutus: {_fire}\n" +
                          $"Fire Pres: {_tabakDustu - _tabakAl}\n");

        var pres = new PresUretimVerisi(_tabakAl, _tabakDustu , currentDate);
        var rotus = new YuRoUretimVerisi(_uretim, _fire, currentDate);

        var veri = new List<IKaydedilcekVeri>();
        veri.Add(pres);
        veri.Add(rotus);

        await writeApi.WriteMeasurementsAsync(veri, WritePrecision.Ms, _bucket, _organization);
    }
}