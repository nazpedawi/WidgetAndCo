using ApplicationServices.DTO;
using MediatR;

namespace ApplicationServices.Queries
{
    public record GetProcessingMetricsQuery(DateTime From, DateTime To) : IRequest<ProcessingMetricsDto>;
}