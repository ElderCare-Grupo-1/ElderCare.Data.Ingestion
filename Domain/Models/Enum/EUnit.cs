using System.ComponentModel;

namespace ElderCare.Data.Ingestion.Domain.Models.Enum
{
    public enum EUnit
    {
        [Description("%")]
        Percentage,
        [Description("°C")]
        Celsius,
        [Description("BPM")]
        BeatsPerMinute,
        None
    }
}
