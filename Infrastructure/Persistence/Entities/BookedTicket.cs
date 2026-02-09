using System;
using System.Collections.Generic;
using NodaTime;

namespace Acceloka.Api.Infrastructure.Persistence.Entities;

public partial class BookedTicket
{
    public Guid Id { get; set; }

    public LocalDateTime BookingDate { get; set; }

    public virtual ICollection<BookedTicketDetail> BookedTicketDetails { get; set; } = new List<BookedTicketDetail>();
}
