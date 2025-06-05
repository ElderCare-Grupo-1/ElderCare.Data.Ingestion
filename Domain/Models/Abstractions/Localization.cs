using ElderCare.Data.Ingestion.Domain.Models.Enum;

namespace ElderCare.Data.Ingestion.Domain.Models.Abstractions;

public class Localization
{
    public ELocalization Local { get; set; }
    public double Luminosity { get; set; }
    public required List<GenericSensorBase> Sensors { get; set; }
    public required MFRC522 RFID { get; set; }


    public static LinkedList<Localization> GeneratedRandomHouse(List<Localization> locales)
    {

        var random = new Random();
        for (var i = locales.Count - 1; i > 0; i--)
        {
            var j = random.Next(0, i + 1);
            (locales[i], locales[j]) = (locales[j], locales[i]);
        }

        return new LinkedList<Localization>(locales);
    }
}

