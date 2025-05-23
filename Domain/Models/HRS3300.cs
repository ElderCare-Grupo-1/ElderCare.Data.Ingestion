﻿using ElderCare.Data.Ingestion.Domain.Models.Abstractions;
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
            GenerateHeartBeat();
            PreviousData = Data;

            return Data;
        }


        private void GenerateHeartBeat()
        {
            Trend = (ETrend)Random.Shared.Next(0, 2);

            var variation = Trend switch
            {
                0 => Random.Shared.Next(3, 7),
                (ETrend)1 => Random.Shared.Next(-7, -3),
                _ => Random.Shared.Next(-5, 5)
            };

            Data = PreviousData + variation;

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
