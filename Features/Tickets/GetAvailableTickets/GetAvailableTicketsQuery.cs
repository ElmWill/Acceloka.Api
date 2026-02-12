namespace Acceloka.Api.Features.Tickets.GetAvailableTickets;

using MediatR;
using NodaTime;

public class GetAvailableTicketsQuery : IRequest<PagedResult<TicketDto>>
{
    public string? CategoryName { get; set; }
    public string? TicketCode { get; set; }
    public string? TicketName { get; set; }
    public decimal? Price { get; set; }
    public LocalDateTime? MinEventDate { get; set; }
    public LocalDateTime? MaxEventDate { get; set; }
    public string? OrderBy { get; set; }
    public string? OrderState { get; set; }
    public int Page { get; set; } = 1;
}