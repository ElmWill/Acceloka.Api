namespace Acceloka.Api.Features.Tickets.EditBookedTicket
{
    public class EditBookedTicketResponse
    {
        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int NewQuantity { get; set; }
    }
}
