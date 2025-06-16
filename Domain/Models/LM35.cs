using ElderCare.Data.Ingestion.Domain.Models.Abstractions;
using ElderCare.Data.Ingestion.Domain.Models.Enum;

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
            var situation = parameters.Length > 0 && parameters[0] is ESituations s ? s : ESituations.Normal;

            switch (situation)
            {
                case ESituations.Normal:
                    Data = MathHelper.GetUniform(18, 30);
                    break;
                case ESituations.Alert:
                    Data = MathHelper.GetUniform(30, 38);
                    break;
                case ESituations.Emergency:
                    Data = MathHelper.GetUniform(38, 45);
                    break;
                case ESituations.Critical:
                    Data = MathHelper.GetUniform(45, 60);
                    break;
                default:
                    Data = MathHelper.GetUniform(18, 30);
                    break;
            }

            if (Random.Shared.NextDouble() < 0.001)
                Data = MathHelper.GetUniform(-55, 150);

            return Data;
        }
    }
}
