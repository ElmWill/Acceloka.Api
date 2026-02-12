namespace Acceloka.Api.Features.Tickets.EditBookedTicket;

using Acceloka.Api.Common;
using Acceloka.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class EditBookedTicketHandler
    : IRequestHandler<EditBookedTicketCommand, EditBookedTicketResponse>
{
    private readonly AppDbContext _context;
    private readonly ILogger<EditBookedTicketHandler> _logger;
    public EditBookedTicketHandler(
        AppDbContext context,
        ILogger<EditBookedTicketHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EditBookedTicketResponse> Handle(
        EditBookedTicketCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("EditBookedTicket started. BookedTicketId={BookedTicketId}, TicketCode={TicketCode}, Quantity={Quantity}",
            request.BookedTicketId, request.TicketCode, request.Quantity);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
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
                _logger.LogWarning("BookedTicket not found. Id={BookedTicketId}", request.BookedTicketId);
                throw new ApiExceptions("BookedTicketId Not Found", StatusCodes.Status404NotFound);
            }

            var detail = bookedTicket.BookedTicketDetails
                .FirstOrDefault(x => x.Ticket.Code == request.TicketCode);

            if (detail == null)
            {
                _logger.LogWarning("TicketCode not found in booking. TicketCode={TicketCode}, BookedTicketId={BookedTicketId}",
                    request.TicketCode, request.BookedTicketId);
                throw new ApiExceptions("TicketCode is not listed on this booking", StatusCodes.Status404NotFound);
            }

            var currentQuantity = detail.Quantity;
            var newQuantity = request.Quantity;

            _logger.LogInformation("CurrentQuantity={CurrentQuantity}, NewQuantity={NewQuantity}",
                currentQuantity, newQuantity);

            if (currentQuantity == newQuantity)
            {
                _logger.LogInformation("Quantity unchanged. No update needed.");
                await transaction.CommitAsync(cancellationToken);
                return new EditBookedTicketResponse
                {
                    TicketCode = detail.Ticket.Code,
                    TicketName = detail.Ticket.Name,
                    CategoryName = detail.Ticket.Category.Name,
                    NewQuantity = newQuantity
                };
            }

            int diff = newQuantity - currentQuantity;
            _logger.LogInformation("Quantity difference calculated. Diff={Diff}", diff);

            if (diff > 0)
            {
                _logger.LogInformation("Reducing ticket quota. TicketId={TicketId}, ReduceBy={Diff}",
                    detail.Ticket.Id, diff);

                var affectedRows = await _context.Database.ExecuteSqlRawAsync(
                    @"UPDATE ""Tickets""
                  SET ""Quota"" = ""Quota"" - {0}
                  WHERE ""Id"" = {1} AND ""Quota"" >= {0}",
                    diff, detail.Ticket.Id);

                if (affectedRows == 0)
                {
                    _logger.LogWarning("Not enough quota. TicketId={TicketId}, RequestedDiff={Diff}",
                        detail.Ticket.Id, diff);
                    throw new ApiExceptions("Not enough quota", StatusCodes.Status400BadRequest);
                }
            }
            else
            {
                _logger.LogInformation("Increasing ticket quota. TicketId={TicketId}, IncreaseBy={Diff}",
                    detail.Ticket.Id, -diff);

                await _context.Database.ExecuteSqlRawAsync(
                    @"UPDATE ""Tickets""
                  SET ""Quota"" = ""Quota"" + {0}
                  WHERE ""Id"" = {1}",
                    -diff, detail.Ticket.Id);
            }

            await _context.Entry(detail.Ticket).ReloadAsync(cancellationToken);

            detail.Quantity = newQuantity;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("EditBookedTicket succeeded. TicketCode={TicketCode}, NewQuantity={NewQuantity}",
                detail.Ticket.Code, newQuantity);

            return new EditBookedTicketResponse
            {
                TicketCode = detail.Ticket.Code,
                TicketName = detail.Ticket.Name,
                CategoryName = detail.Ticket.Category.Name,
                NewQuantity = newQuantity
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EditBookedTicket failed. BookedTicketId={BookedTicketId}, TicketCode={TicketCode}",
                request.BookedTicketId, request.TicketCode);

            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }


}
