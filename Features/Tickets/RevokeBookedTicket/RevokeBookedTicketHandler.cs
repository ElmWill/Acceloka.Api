namespace Acceloka.Api.Features.Tickets.RevokeBookedTicket;

using Acceloka.Api.Common;
using Acceloka.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class RevokeBookedTicketHandler
    : IRequestHandler<RevokeBookedTicketCommand, RevokeBookedTicketResponse>
{
    private readonly AppDbContext _context;
    private readonly ILogger<RevokeBookedTicketHandler> _logger;
    public RevokeBookedTicketHandler(
        AppDbContext context,
        ILogger<RevokeBookedTicketHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RevokeBookedTicketResponse> Handle(
        RevokeBookedTicketCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "RevokeBookedTicket started. BookedTicketId={BookedTicketId}, TicketCode={TicketCode}, Quantity={Quantity}",
            request.BookedTicketId, request.TicketCode, request.Quantity);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var bookedTicket = await _context.BookedTickets
                .Include(Q => Q.BookedTicketDetails)
                .ThenInclude(x => x.Ticket)
                .ThenInclude(t => t.Category)
                .FirstOrDefaultAsync(
                    c => c.Id == request.BookedTicketId, cancellationToken);

            var detail = bookedTicket.BookedTicketDetails
                .FirstOrDefault(x => x.Ticket.Code == request.TicketCode);

            await _context.Database.ExecuteSqlRawAsync(
                @"UPDATE ""Tickets""
              SET ""Quota"" = ""Quota"" + {0}
              WHERE ""Id"" = {1}",
                request.Quantity, detail.Ticket.Id);

            await _context.Entry(detail.Ticket).ReloadAsync(cancellationToken);

            detail.Quantity -= request.Quantity;

            _logger.LogInformation(
                "Updated detail quantity. TicketCode={TicketCode}, RemainingQuantity={RemainingQuantity}",
                detail.Ticket.Code, detail.Quantity);

            if (detail.Quantity == 0)
            {
                _logger.LogInformation(
                    "BookedTicketDetail removed because quantity is zero. TicketCode={TicketCode}",
                    detail.Ticket.Code);

                _context.BookedTicketDetails.Remove(detail);
            }

            var hasRemainingDetails = bookedTicket.BookedTicketDetails
                .Where(Q => Q != detail)
                .Any(Q => Q.Quantity > 0);

            if (!hasRemainingDetails && detail.Quantity == 0)
            {
                _logger.LogInformation(
                    "BookedTicket removed because no remaining ticket details. BookedTicketId={BookedTicketId}",
                    bookedTicket.Id);

                _context.BookedTickets.Remove(bookedTicket);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "RevokeBookedTicket succeeded. BookedTicketId={BookedTicketId}, TicketCode={TicketCode}, RemainingQuantity={RemainingQuantity}",
                request.BookedTicketId, request.TicketCode, detail.Quantity);

            return new RevokeBookedTicketResponse
            {
                TicketCode = detail.Ticket.Code,
                TicketName = detail.Ticket.Name,
                CategoryName = detail.Ticket.Category.Name,
                RemainingQuantity = detail.Quantity,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "RevokeBookedTicket failed. BookedTicketId={BookedTicketId}, TicketCode={TicketCode}",
                request.BookedTicketId, request.TicketCode);

            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
