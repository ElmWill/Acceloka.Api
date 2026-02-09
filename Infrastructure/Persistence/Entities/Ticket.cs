using System;
using System.Collections.Generic;
using NodaTime;

namespace Acceloka.Api.Infrastructure.Persistence.Entities;

public partial class Ticket
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public LocalDateTime EventDate { get; set; }

    public decimal Price { get; set; }

    public int Quota { get; set; }

    public virtual ICollection<BookedTicketDetail> BookedTicketDetails { get; set; } = new List<BookedTicketDetail>();

    public virtual Category Category { get; set; } = null!;
}
