using ElderCare.Data.Ingestion.Domain.Models.Abstractions;

namespace ElderCare.Data.Ingestion.Domain.Models
{
    public class MQ7 : GenericSensor<double>
    {

        public MQ7()
        {
            SensorId = 203;
        }

        public override object GenerateValue(params object[] parameters)
        {
            var actualValue = MathHelper.GetUniform(0, 10);

            var rareEvent = Random.Shared.Next() < 0.001;

            var percentVariation =
                rareEvent ?
                    MathHelper.GetUniform(-10, -3) : MathHelper.GetUniform(-2, 2);

            Data = Math.Max(20, Math.Min(actualValue * (1 + percentVariation / 100), 2000));

            return Data;
        }
    }
}
