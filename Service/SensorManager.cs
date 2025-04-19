using ElderCare.Data.Ingestion.Domain.Models;
using ElderCare.Data.Ingestion.Domain.Models.Abstractions;
using ElderCare.Data.Ingestion.Domain.Models.Enum;
using ElderCare.Data.Ingestion.Repository;

namespace ElderCare.Data.Ingestion.Service
{
    public  class SensorManager
    {
        public readonly ISensorRepository _repository = new SensorRespository();
        public async Task GenerateDataAsync()
        {
            Console.WriteLine("Starting GenerateDataAsync...");

            var localizations = AddLocalizations();
            Console.WriteLine("Localizations added.");

            var globalSensors = AddGlobalSensors();
            Console.WriteLine("Global sensors added.");

            using var cts = new CancellationTokenSource();

            var token = cts.Token;
            var random = new Random();

            var globalSensorsTask = RunGlobalSensorsAsync(globalSensors, token);
            var localSensorsTask = RunLocalSensorsAsync(localizations, random, token);

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
                    var data = g.GenerateValue();
                    _repository.SaveSensorDataAsync(data, g.SensorId);
                    Console.WriteLine($"[GLOBAL] Sensor {g.GetType().Name} with SensorId {g.SensorId} value generated: {data}");
                });

                try
                {
                    await Task.Delay(1000, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
            Console.WriteLine("Global sensors task completed.");
        }

        private async Task RunLocalSensorsAsync(List<Localization> localizations, Random random, CancellationToken token)
        {
            Console.WriteLine("Local sensors task started.");
            while (!token.IsCancellationRequested)
            {
                var selectedLocalization = localizations[random.Next(localizations.Count)];

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
                        ldrSensor.GenerateValue(selectedLocalization.Luminosity);
                        _repository.SaveSensorDataAsync(ldrSensor.Data, ldrSensor.SensorId);
                        selectedLocalization.Luminosity = ldrSensor.Data;
                        Console.WriteLine($"[LOCAL] LDR sensor generated for {selectedLocalization.Local} with luminosity {selectedLocalization.Luminosity}.");
                    }
                    else if(sensor is not MFRC522)
                    {
                        var data = sensor.GenerateValue();
                        _repository.SaveSensorDataAsync(data, sensor.SensorId);
                        Console.WriteLine($"[LOCAL] Sensor {sensor.GetType().Name} value generated for {selectedLocalization.Local}: {data}");
                    }
                    return Task.CompletedTask;
                });

                await Task.WhenAll(sensorTasks);

                try
                {
                    await Task.Delay(2000, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
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
                    Sensors = new List<GenericSensorBase> { new MQ7() }
                },

                new Localization
                {
                    Local = ELocalization.Bathroom,
                    Luminosity = 0.5,
                    RFID = new MFRC522 { IsActive = false },
                    Sensors = [new MQ7()]
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
    }
}
