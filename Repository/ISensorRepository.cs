using ElderCare.Data.Ingestion.Service.Models.Abstractions;

namespace ElderCare.Data.Ingestion.Repository
{
    public interface ISensorRepository
    {
        Task<int> SaveSensorDataAsync<T>(GenericSensor<T> sensor);

    }
}
