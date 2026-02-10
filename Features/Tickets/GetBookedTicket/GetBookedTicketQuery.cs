namespace Acceloka.Api.Features.Tickets.GetBookedTicket;

using MediatR;

public class GetBookedTicketQuery : IRequest<GetBookedTicketResponse>
{
    public Guid BookedTicketId { get; set; }
}
