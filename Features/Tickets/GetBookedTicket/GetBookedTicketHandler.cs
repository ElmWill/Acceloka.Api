namespace Acceloka.Api.Features.Tickets.GetBookedTicket;

using Acceloka.Api.Common;
using Acceloka.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetBookedTicketHandler
    : IRequestHandler<GetBookedTicketQuery, GetBookedTicketResponse>
{
    public readonly AppDbContext _context;
    public GetBookedTicketHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GetBookedTicketResponse> Handle(
        GetBookedTicketQuery request,
        CancellationToken cancellationToken)
    {
        var bookedTicket = await _context.BookedTickets
            .Include(Q => Q.BookedTicketDetails)
            .ThenInclude(x => x.Ticket)
            .ThenInclude(t => t.Category)
            .FirstOrDefaultAsync(
            c => c.Id == request.BookedTicketId,
            cancellationToken);

        if (bookedTicket == null)
        {
            throw new ApiExceptions("BookedTicketId Not Found",
                StatusCodes.Status404NotFound);
        }

        var grouped = bookedTicket.BookedTicketDetails
            .GroupBy(x => x.Ticket.Category.Name)
            .Select(g => new CategoryGroupResponse
            {
                CategoryName = g.Key,
                TotalQuantityPerCategory = g.Sum(x => x.Quantity),
                Tickets = g.Select(x => new TicketDetailResponse
                {
                    TicketCode = x.Ticket.Code,
                    TicketName = x.Ticket.Name,
                    EventDate = x.Ticket.EventDate
                    .ToDateTimeUnspecified()
                    .ToString("dd-MM-yyyy HH:mm"),
                    Quantity = x.Quantity
                }).ToList()
            }).ToList();

        return new GetBookedTicketResponse
        {
            BookedTicketId = bookedTicket.Id,
            Categories = grouped,
            TotalQuantity = bookedTicket.BookedTicketDetails.Sum(Q => Q.Quantity)
        };
    }
}
