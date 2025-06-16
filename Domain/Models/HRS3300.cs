using ElderCare.Data.Ingestion.Domain.Models.Abstractions;
using ElderCare.Data.Ingestion.Domain.Models.Enum;

namespace ElderCare.Data.Ingestion.Domain.Models
{
    public class HRS3300 : GenericSensor<int>
    {
        private int PreviousData { get; set; } = Random.Shared.Next(75, 110);
        private ETrend Trend { get; set; }

        public HRS3300()
        {
            SensorId = 103;
        }

        public override object GenerateValue(params object[] parameters)
        {
            var situation = parameters.Length > 0 && parameters[0] is ESituations s ? s : ESituations.Normal;
            GenerateHeartBeat(situation);
            PreviousData = Data;

            return Data;
        }

        private void GenerateHeartBeat(ESituations situation = ESituations.Normal)
        {
            int variation;
            switch (situation)
            {
                case ESituations.Normal:
                    Trend = (ETrend)Random.Shared.Next(0, 2);
                    variation = Trend switch
                    {
                        0 => Random.Shared.Next(3, 7),
                        (ETrend)1 => Random.Shared.Next(-7, -3),
                        _ => Random.Shared.Next(-5, 5)
                    };
                    break;
                case ESituations.Alert:
                    Trend = ETrend.Up;
                    variation = Random.Shared.Next(5, 12);
                    break;
                case ESituations.Emergency:
                    Trend = ETrend.Up;
                    variation = Random.Shared.Next(15, 30);
                    break;
                case ESituations.Critical:
                    Trend = (ETrend)Random.Shared.Next(0, 2);
                    variation = Trend == ETrend.Up
                        ? Random.Shared.Next(30, 60)
                        : Random.Shared.Next(-30, -10);
                    break;
                default:
                    Trend = (ETrend)Random.Shared.Next(0, 2);
                    variation = Random.Shared.Next(-5, 5);
                    break;
            }

            Data = PreviousData + variation;

            if (Data > 220) Data = 220;
            if (Data < 30) Data = 30;

            Trend = Data switch
            {
                >= 185 => ETrend.Down,
                <= 55 => ETrend.Up,
                > 95 and < 130 => (ETrend)Random.Shared.Next(0, 1),
                _ => (ETrend)Random.Shared.Next(0, 2)
            };
        }


    }
}
