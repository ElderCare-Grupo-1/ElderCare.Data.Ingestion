using ElderCare.Data.Ingestion.Repository;
using ElderCare.Data.Ingestion.Service.Models;
using ElderCare.Data.Ingestion.Service.Models.Abstractions;

var max30102 = new MAX30102();
var hrs3300 = new HRS3300();
Console.WriteLine("Starting the data ingestion process");

ISensorRepository _repository = new SensorRespository();

while (true)
{
    await IngestAsync(max30102, _repository, TimeSpan.FromSeconds(1));
    await IngestAsync(hrs3300, _repository, TimeSpan.FromSeconds(1));
    
}

static async Task IngestAsync<T>(GenericSensor<T> sensor, ISensorRepository repository, TimeSpan interval)
{
    sensor.GenerateValue();
    await repository.SaveSensorDataAsync(sensor);
    await Task.Delay(interval);
}