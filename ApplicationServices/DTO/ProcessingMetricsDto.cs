namespace ApplicationServices.DTO
{
    public record ProcessingMetricsDto(int OrderCount, double AverageHours, double FastestHours, double SlowestHours);
}