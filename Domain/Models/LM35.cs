using ElderCare.Data.Ingestion.Domain.Models.Abstractions;

namespace ElderCare.Data.Ingestion.Domain.Models
{
    public class LM35 : GenericSensor<double>
    {
        public LM35()
        {
            SensorId = 201;
        }

        public override object GenerateValue(params object[] parameters)
        {
            Data = Random.Shared.NextDouble() < 0.001 ?
                MathHelper.GetUniform(-55, 150) : MathHelper.GetUniform(18, 30);

            return Data;
        }
    }
}
