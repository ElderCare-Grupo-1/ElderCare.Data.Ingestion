using System.Data;
using Dapper;
using ElderCare.Data.Ingestion.Service.Models.Abstractions;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace ElderCare.Data.Ingestion.Repository
{
    public class SensorRespository : ISensorRepository
    {
        private readonly IDbConnection _connection = new MySqlConnection()
        {
            ConnectionString = "Server=localhost;Port=3306;Database=elder_care;Uid=root;Pwd=#G567%^1w0;",
        };

        public async Task<int> SaveSensorDataAsync<T>(GenericSensor<T> sensor)
        {
            try
            {
                OpenConnection();

                const string query = "INSERT INTO raw_data(value, timestamp, sensor_idsensor) VALUES (@Data,@Timestamp, @SensorId)";
                
                var parameters = new { sensor.Data, Timestamp = DateTime.Now,  sensor.SensorId };

                var result = await _connection.ExecuteAsync(query, parameters);

                Console.WriteLine($"Sensor data saved successfully: {JsonConvert.SerializeObject(sensor)}");
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                CloseConnection();
            }
        }

        private void OpenConnection()
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        private void CloseConnection()
        {
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
        }
    }
}
