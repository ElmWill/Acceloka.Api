namespace Acceloka.Api.Features.Tickets.EditBookedTicket;

using Acceloka.Api.Common;
using Acceloka.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class EditBookedTicketHandler
    : IRequestHandler<EditBookedTicketCommand, EditBookedTicketResponse>
{
    private readonly AppDbContext _context;
    public EditBookedTicketHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EditBookedTicketResponse> Handle(
        EditBookedTicketCommand request,
        CancellationToken cancellationToken)
    {
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
                throw new ApiExceptions("BookedTicketId Not Found", StatusCodes.Status404NotFound);

            var detail = bookedTicket.BookedTicketDetails
                .FirstOrDefault(x => x.Ticket.Code == request.TicketCode);

            if (detail == null)
                throw new ApiExceptions("TicketCode is not listed on this booking", StatusCodes.Status404NotFound);

            var currentQuantity = detail.Quantity;
            var newQuantity = request.Quantity;

            if (newQuantity < 1)
                throw new ApiExceptions("Quantity must be at least 1", StatusCodes.Status400BadRequest);

            if (currentQuantity == newQuantity)
            {
                return new EditBookedTicketResponse
                {
                    TicketCode = detail.Ticket.Code,
                    TicketName = detail.Ticket.Name,
                    CategoryName = detail.Ticket.Category.Name,
                    NewQuantity = newQuantity
                };
            }

            int diff = newQuantity - currentQuantity;

            if (diff > 0)
            {
                var affectedRows = await _context.Database.ExecuteSqlRawAsync(
                    @"UPDATE ""Tickets""
                  SET ""Quota"" = ""Quota"" - {0}
                  WHERE ""Id"" = {1} AND ""Quota"" >= {0}",
                    diff, detail.Ticket.Id);

                if (affectedRows == 0)
                    throw new ApiExceptions("Not enough quota", StatusCodes.Status400BadRequest);
            }
            else
            {
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

            return new EditBookedTicketResponse
            {
                TicketCode = detail.Ticket.Code,
                TicketName = detail.Ticket.Name,
                CategoryName = detail.Ticket.Category.Name,
                NewQuantity = newQuantity
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

}
