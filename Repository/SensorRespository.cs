using Dapper;
using MySql.Data.MySqlClient;
using System.Data;

namespace ElderCare.Data.Ingestion.Repository
{
    public class SensorRespository : ISensorRepository
    {
        public async Task<int> SaveSensorDataAsync<T>(T data, int sensorId)
        {
            try
            {
                await using var connection = new MySqlConnection("Server=localhost;Port=3306;Database=elder_care;Uid=root;Pwd=#G567%^1w0;");

                await connection.OpenAsync();

                const string query = "INSERT INTO raw_data(value, timestamp, sensor_idsensor) VALUES (@Data, @Timestamp, @SensorId)";

                var parameters = new { Data = data, Timestamp = DateTime.Now, SensorId = sensorId };

                var result = await connection.ExecuteAsync(query, parameters);

                Console.WriteLine($"Sensor data saved successfully: {(data, sensorId).ToString()}");
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
