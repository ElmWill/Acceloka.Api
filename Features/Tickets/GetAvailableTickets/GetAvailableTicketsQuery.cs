namespace Acceloka.Api.Features.Tickets.GetAvailableTickets;

using MediatR;

public class GetAvailableTicketsQuery : IRequest<PagedResult<TicketDto>>
{
    public string? CategoryName { get; set; }
    public string? TicketCode { get; set; }
    public string? TicketName { get; set; }
    public decimal? Price { get; set; }
    public DateTime? MinEventDate { get; set; }
    public DateTime? MaxEventDate { get; set; }
    public string? OrderBy { get; set; }
    public string? OrderState { get; set; }
    public int Page { get; set; } = 1;
}