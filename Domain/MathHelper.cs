namespace ElderCare.Data.Ingestion.Domain
{
    public static class MathHelper
    {
        public static double GetUniform(double a, double b)
        {
            return a + (b - a) * Random.Shared.NextDouble();
        }
    }
}
