namespace Acceloka.Api.Features.Tickets.EditBookedTicket;

using MediatR;

public class EditBookedTicketCommand
    : IRequest<EditBookedTicketResponse>
{
    public Guid BookedTicketId { get; set; }
    public String TicketCode { get; set; } = String.Empty;
    public int Quantity { get; set; }
}
