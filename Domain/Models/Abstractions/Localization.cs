using ElderCare.Data.Ingestion.Domain.Models.Enum;

namespace ElderCare.Data.Ingestion.Domain.Models.Abstractions
{
    public class Localization
    {
        public ELocalization Local { get; set; }
        public double Luminosity { get; set; }
        public List<GenericSensorBase> Sensors { get; set; }
        public MFRC522 RFID { get; set; }
    }
}
