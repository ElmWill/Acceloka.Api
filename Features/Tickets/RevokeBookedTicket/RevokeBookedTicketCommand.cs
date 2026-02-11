namespace Acceloka.Api.Features.Tickets.RevokeBookedTicket;

using MediatR;

public class RevokeBookedTicketCommand : IRequest<RevokeBookedTicketResponse>
{
    public Guid BookedTicketId { get; set; }
    public String TicketCode { get; set; } = String.Empty;
    public int Quantity { get; set; }
}
