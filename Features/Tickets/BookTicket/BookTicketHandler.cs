namespace Acceloka.Api.Features.Tickets.BookTicket;

using Acceloka.Api.Common;
using Acceloka.Api.Infrastructure.Persistence;
using Acceloka.Api.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NodaTime;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class BookTicketHandler : IRequestHandler<BookTicketCommand, BookTicketResponse>
{
    private readonly AppDbContext _context;
    private readonly ILogger<BookTicketHandler> _logger;
    public BookTicketHandler(
        AppDbContext context,
        ILogger<BookTicketHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BookTicketResponse> Handle(
        BookTicketCommand request,
        CancellationToken cancellationToken)
    {
        var bookingDate = LocalDateTime.FromDateTime(DateTime.UtcNow);
        var responseItems = new List<BookedTicketResult>();

        var booked = new BookedTicket
        {
            Id = Guid.NewGuid(),
            BookingDate = bookingDate,
            BookedTicketDetails = new List<BookedTicketDetail>()
        };

        _logger.LogInformation(
            "Booking Started... BookingId{BookingId}, TicketsCount={Count}",
            booked.Id,
            request.Tickets.Count);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            _context.BookedTickets.Add(booked);

            foreach (var item in request.Tickets)
            {
                var affectedRows = await _context.Database.ExecuteSqlRawAsync(
                    @"UPDATE ""Tickets""
                  SET ""Quota"" = ""Quota"" - {0}
                  WHERE ""Code"" = {1} AND ""Quota"" >= {0}",
                    item.Quantity, item.TicketCode);

                if (affectedRows == 0)
                {
                    _logger.LogWarning(
                       "Quota not enough or ticket not found. Code={Code}, Quantity={Quantity}",
                       item.TicketCode,
                       item.Quantity);
                    throw new ApiExceptions("Not enough quota or ticket not found", StatusCodes.Status400BadRequest);
                }

                var ticket = await _context.Tickets
                    .Include(t => t.Category)
                    .FirstOrDefaultAsync(t => t.Code == item.TicketCode, cancellationToken);

                if (ticket == null)
                {
                    throw new ApiExceptions(
                    $"Ticket not found",
                    StatusCodes.Status404NotFound);
                }

                if (ticket.EventDate <= bookingDate)
                {
                    _logger.LogWarning(
                        "Event date passed. Code={Code}, EventDate={EventDate}",
                        item.TicketCode,
                        ticket.EventDate);
                    throw new ApiExceptions("Event date has passed", StatusCodes.Status400BadRequest);
                }

                booked.BookedTicketDetails.Add(new BookedTicketDetail
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticket.Id,
                    BookedTicketId = booked.Id,
                    Quantity = item.Quantity
                });

                responseItems.Add(new BookedTicketResult
                {
                    TicketCode = ticket.Code,
                    TicketName = ticket.Name,
                    CategoryName = ticket.Category.Name,
                    Price = ticket.Price,
                    Quantity = item.Quantity
                });

                _logger.LogInformation(
                    "Ticket added to booking. Code={Code}, BookingId={BookingId}",
                    item.TicketCode,
                    booked.Id);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Booking completed successfully. BookingId={BookingId}, TotalItems={Items}",
                booked.Id,
                responseItems.Count);
        }
        catch (ApiExceptions ex)
        {
            _logger.LogWarning(ex,
                "Error during booking. BookingId={BookingId}",
                booked.Id);

            if (transaction != null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            
            throw;
        }

        var totalPerCategory = responseItems
            .GroupBy(Q => Q.CategoryName)
            .Select(g => new TotalPerCategories
            {
                CategoryName = g.Key,
                TotalPrice = g.Sum(x => x.Price * x.Quantity)
            }).ToList();

        var totalPrice = responseItems.Sum(Q => Q.Price * Q.Quantity);

        return new BookTicketResponse
        {
            Items = responseItems,
            TotalPerCategories = totalPerCategory,
            TotalPrice = totalPrice
        };
    }

}
