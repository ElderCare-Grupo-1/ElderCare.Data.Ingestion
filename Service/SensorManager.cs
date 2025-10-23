using Amazon.IotData;
using Amazon.IotData.Model;
using Amazon.Runtime.Endpoints;
using ElderCare.Data.Ingestion.Domain;
using ElderCare.Data.Ingestion.Domain.Models;
using ElderCare.Data.Ingestion.Domain.Models.Abstractions;
using ElderCare.Data.Ingestion.Domain.Models.Enum;
using ElderCare.Data.Ingestion.Repository;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Server;
using MySqlX.XDevAPI;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace ElderCare.Data.Ingestion.Service;

public class SensorManager : IDisposable
{
    public readonly ISensorRepository Repository = new SensorRespository();
    //private readonly DeviceClient _iotClient = DeviceClient.CreateFromConnectionString(
    //    "HostName=EdIoTHubs.azure-devices.net;DeviceId=EdDevice;SharedAccessKey=GniKNTNmVK3sm29ddkcEpYfZpbSnO7BVYVJ++xcwotU=",
    //    TransportType.Mqtt
    //);

    private static readonly string basePath = Path.Combine(AppContext.BaseDirectory, "Certificates");

    private ESituations _situation = ESituations.Normal;
    private int _situationRounds = 0;
    private const int MinRoundsPerSituation = 5;
    private readonly Random _random = new();
    private const int Time = 10000;

    private readonly X509Certificate2 _pfxCert;
    private readonly IMqttClientOptions _options;
    private const string _endpoint = "ayl13rzxyyzr0-ats.iot.us-east-1.amazonaws.com";
    private const int _port = 8883;
    private const string _clientId = "sensores_thing";
    private const string _topic = "topic/sensores_thing";

    private readonly IMqttClient _mqttClient;

    public SensorManager()
    {
        _pfxCert = new X509Certificate2(Path.Combine(basePath, "device.pfx"), "1234");
        _options = new MqttClientOptionsBuilder()
            .WithClientId(_clientId)
            .WithTcpServer(_endpoint, _port)
            .WithTls(new MqttClientOptionsBuilderTlsParameters
            {
                UseTls = true,
                Certificates = [_pfxCert],
                CertificateValidationHandler = _ => true,
                SslProtocol = System.Security.Authentication.SslProtocols.Tls12
            })
            .Build();

        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        _mqttClient.ConnectAsync(_options).GetAwaiter().GetResult();

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

    }
    public async Task GenerateDataAsync()
    {
        Console.WriteLine("Starting GenerateDataAsync...");


        var randomHouse = Localization.GeneratedRandomHouse(AddLocalizations());

        var start = PathHelper.DefineStartingPoint(randomHouse);
        var end = PathHelper.GetRandomToGo(randomHouse, start);
        var path = randomHouse.GetPath(start: start, end: end);

        start = end;

        Console.WriteLine("Path generated:");
        foreach (var localization in path)
        {
            Console.WriteLine($"- {localization.Local}");

        }

        var localizations = path ?? throw new ArgumentNullException(nameof(path));
        Console.WriteLine("Localizations added.");

        var globalSensors = AddGlobalSensors();
        Console.WriteLine("Global sensors added.");

        using var cts = new CancellationTokenSource();

        var token = cts.Token;
        var random = new Random();

        var globalSensorsTask = RunGlobalSensorsAsync(globalSensors, token);
        var localSensorsTask = RunLocalSensorsAsync(randomHouse, start, end, localizations, random, token);

        await Task.WhenAll(globalSensorsTask, localSensorsTask);
        Console.WriteLine("GenerateDataAsync completed.");
    }

    private async Task RunGlobalSensorsAsync(List<GenericSensorBase> globalSensors, CancellationToken token)
    {


        Console.WriteLine("Global sensors task started.");
        while (!token.IsCancellationRequested)
        {
            Parallel.ForEach(globalSensors, g =>
            {
                UpdateSituation();
                var data = g.GenerateValue(_situation);
                //Repository.SaveSensorDataAsync(data, g.SensorId);
                _ = SendToAwsIotCoreAsync(data, g.SensorId, "Global");
                Console.WriteLine($"[GLOBAL] Sensor {g.GetType().Name} with SensorId {g.SensorId} value generated: {data}");
            });

            Thread.Sleep(60000);
        }
        Console.WriteLine("Global sensors task completed.");
    }

    private async Task RunLocalSensorsAsync(
     LinkedList<Localization> randomHouse,
     Localization start,
     Localization end,
     List<Localization> localizations,
     Random random,
     CancellationToken token
 )
    {
        Console.WriteLine("Local sensors task started.");

        var pathIndex = 0;

        while (!token.IsCancellationRequested)
        {
            if (pathIndex >= localizations.Count)
            {
                start = end;
                end = PathHelper.GetRandomToGo(randomHouse, start);
                localizations = randomHouse.GetPath(start: start, end: end) ?? throw new ArgumentNullException(nameof(localizations));
                pathIndex = 0;

                Console.WriteLine("Path generated:");
                Console.WriteLine("-----------------------------------------------");
                foreach (var localization in localizations)
                {
                    Console.WriteLine($"- {localization.Local}");
                }
            }

            var selectedLocalization = localizations[pathIndex];

            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine($"[LOCAL] Processing localization: {selectedLocalization.Local}");

            foreach (var loc in localizations)
            {
                loc.RFID.IsActive = loc == selectedLocalization;
                Console.WriteLine($"RFID for {loc.Local} set to {(loc.RFID.IsActive ? "active" : "inactive")}.");
            }

            var sensorTasks = selectedLocalization.Sensors.Select(sensor =>
            {
                if (sensor is LDR ldrSensor)
                {
                    ldrSensor.GenerateValue(selectedLocalization.Luminosity, _situation);
                    //Repository.SaveSensorDataAsync(ldrSensor.Data, ldrSensor.SensorId);
                    _ = SendToAwsIotCoreAsync(ldrSensor.Data, ldrSensor.SensorId, selectedLocalization.Local.ToString());
                    selectedLocalization.Luminosity = ldrSensor.Data;
                    Console.WriteLine($"[LOCAL] LDR sensor generated for {selectedLocalization.Local} with luminosity {selectedLocalization.Luminosity}.");
                }
                else if (sensor is not MFRC522)
                {
                    var data = sensor.GenerateValue(_situation);
                    //Repository.SaveSensorDataAsync(data, sensor.SensorId);
                    _ = SendToAwsIotCoreAsync(data, sensor.SensorId, selectedLocalization.Local.ToString());
                    Console.WriteLine($"[LOCAL] Sensor {sensor.GetType().Name} value generated for {selectedLocalization.Local}: {data}");
                }

                return Task.CompletedTask;
            });

            await Task.WhenAll(sensorTasks);

            Thread.Sleep(60000);
            pathIndex++;
        }

        Console.WriteLine("Local sensors task completed.");
    }

    private static List<Localization> AddLocalizations()
    {
        return
        [
            new Localization
            {
                Local = ELocalization.Kitchen,
                Luminosity = 0.5,
                RFID = new MFRC522 { IsActive = false },
                Sensors = [new LDR(), new LM35(), new MQ7()]
            },

            new Localization
            {
                Local = ELocalization.LivingRoom,
                Luminosity = 0.5,
                RFID = new MFRC522 { IsActive = false },
                Sensors = [new LDR(), new LM35()]
            },

            new Localization
            {
                Local = ELocalization.Bedroom,
                Luminosity = 0.5,
                RFID = new MFRC522 { IsActive = false },
                Sensors = [new LDR(), new LM35()]
            },

            new Localization
            {
                Local = ELocalization.Garage,
                Luminosity = 0.5,
                RFID = new MFRC522 { IsActive = false },
                Sensors = [new LDR(), new MQ7()]
            },

            new Localization
            {
                Local = ELocalization.Bathroom,
                Luminosity = 0.5,
                RFID = new MFRC522 { IsActive = false },
                Sensors = [new LDR(), new MQ7()]
            },

            new Localization
            {
                Local = ELocalization.Stair,
                Luminosity = 0.5,
                RFID = new MFRC522 { IsActive = false },
                Sensors = [new LDR(), new MQ7()]
            }
        ];
    }

    private static List<GenericSensorBase> AddGlobalSensors()
    {
        return
        [
            new MAX30102(),
            new HRS3300()
        ];
    }

    private async Task SendToAwsIotCoreAsync(object sensorData, int sensorId, string? local)
    {

        var payload = new
        {
            SensorId = sensorId,
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Data = sensorData,
            Local = local ?? "",
        };

        var json = JsonSerializer.Serialize(payload);

        Console.WriteLine(json);


        try
        {
            await PublishMessage(JsonSerializer.Serialize(payload));
            Console.WriteLine($"✔️ [AWS IOT] Dados do sensor {sensorId} enviados com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [AWS IOT] Falha ao enviar dados do sensor {sensorId}: {ex.Message}");
        }
    }

    private ESituations GetRandomSituation(ESituations current)
    {
        var values = Enum.GetValues<ESituations>().Where(s => s != current).ToList();
        return values[_random.Next(values.Count)];
    }

    private async Task PublishMessage(string messagePayload)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(_topic)
            .WithPayload(messagePayload)
            .Build();

        await _mqttClient.PublishAsync(message);
    }

    private void UpdateSituation()
    {
        Console.WriteLine($"Previous situation: {_situation.ToString()}");
        _situationRounds++;
        if (_situationRounds < MinRoundsPerSituation) return;

        var newSituation = GetRandomSituation(_situation);
        _situation = newSituation;
        _situationRounds = 0;
        Console.WriteLine($"Situação alterada para: {_situation.ToString()}");
    }

    public void Dispose()
    {
        _mqttClient.DisconnectAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }
}