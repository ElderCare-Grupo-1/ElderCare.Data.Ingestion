namespace ElderCare.Data.Ingestion.Domain
{
    public class ResultData<T>(T? data, string? errorMessage)
    {
        public T? Data { get; set; } = data;
        public DateTime GenerationDate { get; set; } = DateTime.UtcNow;
        public string? ErrorMessage { get; set; } = errorMessage;
    }
}
