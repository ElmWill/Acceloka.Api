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
    public BookTicketHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BookTicketResponse> Handle(
        BookTicketCommand request,
        CancellationToken cancellationToken)
    {
        var date_now = DateTime.UtcNow;
        var bookingDate = LocalDateTime.FromDateTime(date_now);
        var responseItems = new List<BookedTicketResult>();

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        foreach (var item in request.Tickets)
        {
            if (item.Quantity <= 0)
            {
                throw new ApiExceptions("Quantity must be greater than 0",
                    StatusCodes.Status400BadRequest);
            }

            var ticket = await _context.Tickets
                .FromSqlRaw(@"SELECT * FROM ""Tickets"" WHERE ""Code"" = {0} FOR UPDATE", item.TicketCode)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(cancellationToken);

            if (ticket == null)
            {
                throw new ApiExceptions($"Ticket Code {item.TicketCode} is not recognized",
                    StatusCodes.Status404NotFound);
            }

            if (ticket.Quota <= 0)
            {
                throw new ApiExceptions($"{item.TicketCode} is sold out",
                    StatusCodes.Status400BadRequest);
            }

            if (item.Quantity > ticket.Quota)
            {
                throw new ApiExceptions("The quantity you are trying to book is more than the available quota",
                    StatusCodes.Status400BadRequest);
            }

            if (ticket.EventDate <= bookingDate)
            {
                throw new ApiExceptions("Event date has passed",
                    StatusCodes.Status400BadRequest);
            }

            ticket.Quota -= item.Quantity;

            var booked = new BookedTicket
            {
                Id = Guid.NewGuid(),
                BookingDate = bookingDate
            };

            booked.BookedTicketDetails.Add(
                new BookedTicketDetail
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticket.Id,
                    BookedTicketId = booked.Id,
                    Quantity = item.Quantity
                });

            _context.BookedTickets.Add(booked);

            responseItems.Add(new BookedTicketResult
            {
                TicketCode = ticket.Code,
                TicketName = ticket.Name,
                CategoryName = ticket.Category.Name,
                Price = ticket.Price,
                Quantity = item.Quantity
            });
        }
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {

            throw new ApiExceptions("Ticket quota was updated by another user.Please try again.",
        StatusCodes.Status409Conflict);
        }
        
        await transaction.CommitAsync(cancellationToken);

        var totalPerCategory = responseItems
            .GroupBy(Q => Q.CategoryName)
            .Select(G => new TotalPerCategories
            {
                CategoryName = G.Key,
                TotalPrice = G.Sum(Q => Q.Price * Q.Quantity)
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
