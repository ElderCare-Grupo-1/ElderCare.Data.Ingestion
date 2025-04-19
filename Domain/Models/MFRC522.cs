using ElderCare.Data.Ingestion.Domain.Models.Abstractions;

namespace ElderCare.Data.Ingestion.Domain.Models
{
    public class MFRC522 : GenericSensor<bool>
    {
        public bool IsActive { get; set; }

        public MFRC522()
        {
            SensorId = 102;
        }

        public override object GenerateValue(params object[] parameters)
        {
            IsActive = !IsActive;
            return IsActive;
        }
    }
}
