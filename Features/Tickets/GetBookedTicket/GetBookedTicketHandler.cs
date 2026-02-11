namespace Acceloka.Api.Features.Tickets.GetBookedTicket;

using Acceloka.Api.Common;
using Acceloka.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetBookedTicketHandler
    : IRequestHandler<GetBookedTicketQuery, GetBookedTicketResponse>
{
    public readonly AppDbContext _context;
    public readonly ILogger<GetBookedTicketHandler> _logger;
    public GetBookedTicketHandler(
        AppDbContext context,
        ILogger<GetBookedTicketHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetBookedTicketResponse> Handle(
        GetBookedTicketQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetBookedTicket started. BookedTicketId={BookedTicketId}",
            request.BookedTicketId);

        var bookedTicket = await _context.BookedTickets
            .AsNoTracking()
            .Include(Q => Q.BookedTicketDetails)
                .ThenInclude(x => x.Ticket)
                    .ThenInclude(t => t.Category)
            .FirstOrDefaultAsync(
                Q => Q.Id == request.BookedTicketId,
                cancellationToken);

        if (bookedTicket == null)
        {
            _logger.LogWarning(
                "BookedTicket not found. BookedTicketId={BookedTicketId}",
                request.BookedTicketId);

            throw new ApiExceptions(
                "BookedTicketId Not Found",
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
            })
            .ToList();

        var totalQuantity = bookedTicket.BookedTicketDetails
            .Sum(Q => Q.Quantity);

        _logger.LogInformation(
            "GetBookedTicket finished. BookedTicketId={BookedTicketId}, TotalQuantity={TotalQuantity}",
            bookedTicket.Id,
            totalQuantity);

        return new GetBookedTicketResponse
        {
            BookedTicketId = bookedTicket.Id,
            Categories = grouped,
            TotalQuantity = totalQuantity
        };
    }

}
