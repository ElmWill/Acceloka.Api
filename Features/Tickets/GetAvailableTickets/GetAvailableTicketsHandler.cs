namespace Acceloka.Api.Features.Tickets.GetAvailableTickets;

using Acceloka.Api.Common;
using Acceloka.Api.Infrastructure.Persistence;
using Acceloka.Api.Infrastructure.Persistence.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

public class GetAvailableTicketsHandler
    : IRequestHandler<GetAvailableTicketsQuery, PagedResult<TicketDto>>
{
    private readonly AppDbContext _context;
    private readonly ILogger<GetAvailableTicketsHandler> _logger;

    public GetAvailableTicketsHandler(
        AppDbContext context,
        ILogger<GetAvailableTicketsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<TicketDto>> Handle(
        GetAvailableTicketsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetAvailableTickets started with {@Request}", request);

        if (request.Page <= 0)
        {
            throw new ApiExceptions(
                "Page must be greater than 0",
                StatusCodes.Status400BadRequest);
        }

        if (request.Price.HasValue && request.Price.Value < 0)
        {
            throw new ApiExceptions(
                "Price must be greater than or equal to 0",
                StatusCodes.Status400BadRequest);
        }

        if (request.MinEventDate.HasValue && request.MaxEventDate.HasValue)
        {
            if (request.MinEventDate > request.MaxEventDate)
            {
                throw new ApiExceptions(
                    "MinEventDate cannot be greater than MaxEventDate",
                    StatusCodes.Status400BadRequest);
            }
        }

        IQueryable<Ticket> query = _context.Tickets
            .AsNoTracking()
            .Include(Q => Q.Category)
            .Where(Q => Q.Quota > 0);

        if (!string.IsNullOrWhiteSpace(request.CategoryName))
        {
            query = query.Where(Q =>
                Q.Category.Name.Contains(request.CategoryName));
        }

        if (!string.IsNullOrWhiteSpace(request.TicketCode))
        {
            query = query.Where(Q =>
                Q.Code.Contains(request.TicketCode));
        }

        if (!string.IsNullOrWhiteSpace(request.TicketName))
        {
            query = query.Where(Q =>
                Q.Name.Contains(request.TicketName));
        }

        if (request.Price.HasValue)
        {
            query = query.Where(Q =>
                Q.Price <= request.Price.Value);
        }

        if (request.MinEventDate.HasValue)
        {
            var min = LocalDateTime.FromDateTime(request.MinEventDate.Value);
            query = query.Where(Q => Q.EventDate >= min);
        }

        if (request.MaxEventDate.HasValue)
        {
            var max = LocalDateTime.FromDateTime(request.MaxEventDate.Value);
            query = query.Where(Q => Q.EventDate <= max);
        }

        var totalTickets = await query.CountAsync(cancellationToken);

        var orderBy = request.OrderBy?.ToLower() ?? "code";
        var orderState = request.OrderState?.ToLower() ?? "asc";

        query = orderBy switch
        {
            "categoryname" => orderState == "desc"
                ? query.OrderByDescending(Q => Q.Category.Name)
                : query.OrderBy(Q => Q.Category.Name),

            "ticketname" => orderState == "desc"
                ? query.OrderByDescending(Q => Q.Name)
                : query.OrderBy(Q => Q.Name),

            "price" => orderState == "desc"
                ? query.OrderByDescending(Q => Q.Price)
                : query.OrderBy(Q => Q.Price),

            "eventdate" => orderState == "desc"
                ? query.OrderByDescending(Q => Q.EventDate)
                : query.OrderBy(Q => Q.EventDate),

            _ => orderState == "desc"
                ? query.OrderByDescending(Q => Q.Code)
                : query.OrderBy(Q => Q.Code)
        };

        const int pageSize = 10;

        var tickets = await query
            .Skip((request.Page - 1) * pageSize)
            .Take(pageSize)
            .Select(Q => new TicketDto
            {
                EventDate = Q.EventDate
                    .ToDateTimeUnspecified()
                    .ToString("dd-MM-yyyy HH:mm"),
                Quota = Q.Quota,
                TicketCode = Q.Code,
                TicketName = Q.Name,
                CategoryName = Q.Category.Name,
                Price = Q.Price
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "GetAvailableTickets finished. TotalTickets={TotalTickets}, Returned={ReturnedCount}",
            totalTickets,
            tickets.Count);

        return new PagedResult<TicketDto>
        {
            Tickets = tickets,
            TotalTickets = totalTickets,
            CurrentPage = request.Page,
            TotalPages = (int)Math.Ceiling(totalTickets / (double)pageSize),
            OrderedBy = orderBy,
            OrderState = orderState
        };
    }
}
