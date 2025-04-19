using ElderCare.Data.Ingestion.Domain.Models.Abstractions;

namespace ElderCare.Data.Ingestion.Domain.Models
{
    public class LDR : GenericSensor<double>
    {
        public LDR()
        {
            SensorId = 202;
        }

        private double GenerateLuminosity(double startLuminosity)
        {
            Data =
                Math.Max(0,
                    Math.Min(1,
                        MathHelper.GetUniform(-0.1, 0.1) + startLuminosity
                    )
                );

            return Data;
        }


        public override object GenerateValue(params object[] parameters)
        {
            return GenerateLuminosity((double)parameters[0]);
        }
    }
}
