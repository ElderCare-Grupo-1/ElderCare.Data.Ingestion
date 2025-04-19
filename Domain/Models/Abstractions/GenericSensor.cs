namespace ElderCare.Data.Ingestion.Domain.Models.Abstractions;
public abstract class GenericSensorBase
{
    public int SensorId { get; set; }
    public abstract object GenerateValue(params object[] parameters);
}

public abstract class GenericSensor<T> : GenericSensorBase
{
    public T Data { get; set; }
    public abstract override object GenerateValue(params object[] parameters);
}
