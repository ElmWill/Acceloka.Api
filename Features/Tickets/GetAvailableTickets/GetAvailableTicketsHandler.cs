namespace Acceloka.Api.Features.Tickets.GetAvailableTickets;

using Acceloka.Api.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

public class GetAvailableTicketsHandler 
    : IRequestHandler<GetAvailableTicketsQuery, PagedResult<TicketDto>>
{
    private readonly AppDbContext _context;

    public GetAvailableTicketsHandler(AppDbContext context)
    {
        _context = context; 
    }     
    
    public async Task<PagedResult<TicketDto>> Handle(
        GetAvailableTicketsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Tickets
            .Include(Q => Q.Category)
            .Where(Q => Q.Quota > 0)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.CategoryName))
        {
            query = query.Where(Q =>
            Q.Category.Name.Contains(request.CategoryName));
        }

        if (!string.IsNullOrEmpty(request.TicketCode))
        {
            query = query.Where(Q =>
            Q.Category.Name.Contains(request.TicketCode));
        }

        if (!string.IsNullOrEmpty(request.TicketName))
        {
            query = query.Where(Q =>
            Q.Category.Name.Contains(request.TicketName));
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
        return new PagedResult<TicketDto>
        {
            Tickets = tickets,
            TotalTickets = totalTickets,
            CurrentPage = request.Page,
            TotalPages = (int)Math.Ceiling(totalTickets / (double)pageSize)
        };
    }

}
