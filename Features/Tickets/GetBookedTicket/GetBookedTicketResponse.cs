using NodaTime;

namespace Acceloka.Api.Features.Tickets.GetBookedTicket
{
    public class GetBookedTicketResponse
    {
        public Guid BookedTicketId { get; set; }
        public int TotalQuantity { get; set; }
        public List<CategoryGroupResponse> Categories { get; set; } = new();
    }

    public class CategoryGroupResponse
    {
        public string CategoryName { get; set; } = string.Empty;
        public int TotalQuantityPerCategory { get; set; }
        public List<TicketDetailResponse> Tickets { get; set; } = new();
    }

    public class TicketDetailResponse
    {
        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set; } = string.Empty;
        public string EventDate { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
