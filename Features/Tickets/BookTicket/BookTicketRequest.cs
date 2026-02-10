namespace Acceloka.Api.Features.Tickets.BookTicket
{
    public class BookTicketRequest
    {
        public List<BookTicketItem> Tickets { get; set; } = new();
    }

    public class BookTicketItem
    {
        public string TicketCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
