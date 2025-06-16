using ElderCare.Data.Ingestion.Domain.Models.Abstractions;
using ElderCare.Data.Ingestion.Domain.Models.Enum;

namespace ElderCare.Data.Ingestion.Domain.Models
{
    public class LDR : GenericSensor<double>
    {
        public LDR()
        {
            SensorId = 202;
        }

        private double GenerateLuminosity(double startLuminosity, ESituations situation = ESituations.Normal)
        {
            var variation = situation switch
            {
                ESituations.Normal => MathHelper.GetUniform(-0.1, 0.1),
                ESituations.Alert => MathHelper.GetUniform(-0.3, 0.0) 
                ,
                ESituations.Emergency => MathHelper.GetUniform(-0.6, -0.2) 
                ,
                ESituations.Critical => MathHelper.GetUniform(-0.9, -0.5) 
                ,
                _ => MathHelper.GetUniform(-0.1, 0.1)
            };

            Data = Math.Max(0, Math.Min(1, startLuminosity + variation));
            return Data;
        }

        public override object GenerateValue(params object[] parameters)
        {
            var startLuminosity = (double)parameters[0];
            var situation = parameters.Length > 1 && parameters[1] is ESituations s ? s : ESituations.Normal;
            return GenerateLuminosity(startLuminosity, situation);
        }
    }
}
