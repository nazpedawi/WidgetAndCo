using ApplicationServices.DTO;
using ApplicationServices.Exceptions;
using DataLayer.Repo.Interfaces;
using MediatR;

namespace ApplicationServices.Queries.Handlers
{
    public class GetProcessingMetricsQueryHandler(IReadOrderRepository orders)
        : IRequestHandler<GetProcessingMetricsQuery, ProcessingMetricsDto>
    {
        public async Task<ProcessingMetricsDto> Handle(GetProcessingMetricsQuery request, CancellationToken cancellationToken)
        {
            if (request.From > request.To)
                throw new ValidationException("'From' must be before 'To'.");

            var hours = (await orders.GetShippedBetweenAsync(request.From, request.To))
                .Select(o => o.GetProcessingTime())
                .Where(t => t.HasValue)
                .Select(t => t!.Value.TotalHours)
                .ToList();

            if (hours.Count == 0)
                return new ProcessingMetricsDto(0, 0, 0, 0);

            return new ProcessingMetricsDto(hours.Count, hours.Average(), hours.Min(), hours.Max());
        }
    }
}