namespace Acceloka.Api.Features.Tickets.BookTicket
{
    public class BookTicketResponse
    {
        public List<BookedTicketResult> Items { get; set; } = new();
        public List<TotalPerCategories> TotalPerCategories { get; set; } = new();
        public decimal TotalPrice { get; set; }
    }

    public class TotalPerCategories
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
    }

    public class BookedTicketResult
    {
        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
