using ElderCare.Data.Ingestion.Domain.Models.Abstractions;

namespace ElderCare.Data.Ingestion.Domain.Models
{
    public class MAX30102 : GenericSensor<double>
    {
        private double _previousSp02 = 95.0;

        public MAX30102()
        {
            SensorId = 101;
        }

        public override object GenerateValue(params object[] parameters)
        {
            var (dcR, dcIr) = GenerateDcrIr();
            var (acR, acIr) = GenerateAcrAndIr();

            var spO2 = acR / dcR / (acIr / dcIr) * 100;

            if (spO2 > 100)
                spO2 = 100;
            if (spO2 < 0)
                spO2 = 0;

            Data = Math.Round(SmoothSpO2(spO2), 2);
            _previousSp02 = Data;


            return Data;
        }

        private static (double DcR, double DcIr) GenerateDcrIr()
        {
            const int sampleRate = 20;

            var adcFullScale = Random.Shared.Next(2048, 16384);
            var ledR = Random.Shared.Next(1, 50);
            var ledIr = Random.Shared.Next(1, 50);
            var pulseWidth = Random.Shared.Next(50, 400);
            var ratioLedR = ledR > 0 ? (double)ledR / 50 : 0;
            var ratioLedIr = ledIr > 0 ? (double)ledIr / 50 : 0;

            var dcR = ratioLedR * (adcFullScale / 2048.0) * (100.0 / pulseWidth) * sampleRate;
            var dcIr = ratioLedIr * (adcFullScale / 2048.0) * (100.0 / pulseWidth) * sampleRate;

            dcR = Math.Max(dcR, 0.1);
            dcIr = Math.Max(dcIr, 0.1);

            return (dcR, dcIr);
        }



        private static (double AcR, double AcIr) GenerateAcrAndIr()
        {
            const int sampleRate = 50;

            var adcFullScale = Random.Shared.Next(2048, 16384);
            var ledR = Random.Shared.Next(1, 50);
            var ledIr = Random.Shared.Next(1, 50);
            var pulseWidth = Random.Shared.Next(50, 400);

            var ratioLedR = ledR > 0 ? (double)ledR / 50 : 0;
            var ratioLedIr = ledIr > 0 ? (double)ledIr / 50 : 0;

            var acR = ratioLedR * (adcFullScale / 2048.0) * (100.0 / pulseWidth) * sampleRate * MathHelper.GetUniform(0.5, 1.5);
            var acIr = ratioLedIr * (adcFullScale / 2048.0) * (100.0 / pulseWidth) * sampleRate * MathHelper.GetUniform(0.5, 1.5);

            return (acR, acIr);
        }

        private double SmoothSpO2(double currentSpO2, double alpha = 0.1)
        {
            return alpha * currentSpO2 + (1 - alpha) * _previousSp02;
        }

    }
}
