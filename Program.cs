using ElderCare.Data.Ingestion.Service;

Console.WriteLine("Starting the data ingestion process");
var sensorManager = new SensorManager();

await sensorManager.GenerateDataAsync();
sensorManager.Dispose();
Console.ReadLine();

