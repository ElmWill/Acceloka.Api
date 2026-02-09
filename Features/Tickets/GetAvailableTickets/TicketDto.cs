namespace Acceloka.Api.Features.Tickets.GetAvailableTickets
{
    public class TicketDto
    {
        public string EventDate { get; set; } = string.Empty;
        public int Quota { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set;} = string.Empty;
        public string CategoryName { get; set;} = string.Empty;
        public decimal Price { get; set; }
    }
    
    public class PagedResult<T>
    {
        public List<T> Tickets { get; set; } = new();
        public int TotalTickets { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
