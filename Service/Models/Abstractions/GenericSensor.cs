using ElderCare.Data.Ingestion.Service.Models.Enum;

namespace ElderCare.Data.Ingestion.Service.Models.Abstractions;
public abstract class GenericSensor<T>
{
    public int SensorId { get; set; }
    public T Data { get; set; }
    public EUnit Unit { get; set; }


    public abstract T GenerateValue();
}
