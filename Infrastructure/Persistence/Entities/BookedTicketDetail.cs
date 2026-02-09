using System;
using System.Collections.Generic;

namespace Acceloka.Api.Infrastructure.Persistence.Entities;

public partial class BookedTicketDetail
{
    public Guid Id { get; set; }

    public Guid BookedTicketId { get; set; }

    public Guid TicketId { get; set; }

    public int Quantity { get; set; }

    public virtual BookedTicket BookedTicket { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;
}
