using ElderCare.Data.Ingestion.Domain.Models.Abstractions;
using ElderCare.Data.Ingestion.Domain.Models.Enum;

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
            var situation = parameters.Length > 0 && parameters[0] is ESituations s ? s : ESituations.Normal;

            double actualValue;
            double percentVariation;

            switch (situation)
            {
                case ESituations.Normal:
                    actualValue = MathHelper.GetUniform(0, 10);
                    percentVariation = MathHelper.GetUniform(-2, 2);
                    break;
                case ESituations.Alert:
                    actualValue = MathHelper.GetUniform(5, 20);
                    percentVariation = MathHelper.GetUniform(0, 5);
                    break;
                case ESituations.Emergency:
                    actualValue = MathHelper.GetUniform(20, 50);
                    percentVariation = MathHelper.GetUniform(5, 10);
                    break;
                case ESituations.Critical:
                    actualValue = MathHelper.GetUniform(50, 100);
                    percentVariation = MathHelper.GetUniform(10, 20);
                    break;
                default:
                    actualValue = MathHelper.GetUniform(0, 10);
                    percentVariation = MathHelper.GetUniform(-2, 2);
                    break;
            }

            // Evento raro pode aumentar ainda mais o valor
            var rareEvent = Random.Shared.NextDouble() < 0.001;
            if (rareEvent)
            {
                percentVariation += MathHelper.GetUniform(10, 50);
            }

            Data = Math.Max(3, Math.Min(actualValue * (1 + percentVariation / 100), 2000));
            return Data;
        }
    }
}
