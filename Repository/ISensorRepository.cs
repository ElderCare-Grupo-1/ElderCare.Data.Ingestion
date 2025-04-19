namespace ElderCare.Data.Ingestion.Repository
{
    public interface ISensorRepository
    {
        Task<int> SaveSensorDataAsync<T>(T data, int sensorId);

    }
}
