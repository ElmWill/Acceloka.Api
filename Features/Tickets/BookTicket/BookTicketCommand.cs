namespace Acceloka.Api.Features.Tickets.BookTicket;
using MediatR;

public class BookTicketCommand : IRequest<BookTicketResponse>
{
    public List<BookTicketItem> Tickets { get; set; } = new();
}
