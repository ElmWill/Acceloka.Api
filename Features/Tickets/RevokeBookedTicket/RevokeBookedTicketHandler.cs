namespace Acceloka.Api.Features.Tickets.RevokeBookedTicket;

using Acceloka.Api.Common;
using Acceloka.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class RevokeBookedTicketHandler
    : IRequestHandler<RevokeBookedTicketCommand, RevokeBookedTicketResponse>
{
    private readonly AppDbContext _context;
    public RevokeBookedTicketHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RevokeBookedTicketResponse> Handle(
        RevokeBookedTicketCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var bookedTicket = await _context.BookedTickets
            .Include(Q => Q.BookedTicketDetails)
            .ThenInclude(x => x.Ticket)
            .ThenInclude(t => t.Category)
            .FirstOrDefaultAsync(
            c => c.Id == request.BookedTicketId, cancellationToken);

        if (bookedTicket == null)
        {
            throw new ApiExceptions(
                "BookedTicketId Not Found",
                StatusCodes.Status404NotFound);
        }

        var detail = bookedTicket.BookedTicketDetails
            .FirstOrDefault(x => 
            x.Ticket.Code == request.TicketCode);

        if (detail == null)
        {
            throw new ApiExceptions(
                "TicketCode is not listed on this booking",
                StatusCodes.Status404NotFound);
        }

        if (request.Quantity > detail.Quantity)
        {
            throw new ApiExceptions(
                "Quantity is more than what was booked",
                StatusCodes.Status400BadRequest);
        }

        await _context.Database.ExecuteSqlRawAsync(
            @"UPDATE ""Tickets""
            SET ""Quota"" = ""Quota"" + {0}
            WHERE ""Id"" = {1}",
            request.Quantity, detail.Ticket.Id);

        detail.Quantity -= request.Quantity;

        if (detail.Quantity == 0)
        {
            _context.BookedTicketDetails.Remove(detail);
        }

        if (!bookedTicket.BookedTicketDetails
            .Any(Q => Q.Quantity > 0))
        {
            _context.BookedTickets.Remove(bookedTicket);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new RevokeBookedTicketResponse
        {
            TicketCode = detail.Ticket.Code,
            TicketName = detail.Ticket.Name,
            CategoryName = detail.Ticket.Category.Name,
            RemainingQuantity = detail.Quantity,
        };
    }
}
