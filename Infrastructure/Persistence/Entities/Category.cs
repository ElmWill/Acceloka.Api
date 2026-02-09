using System;
using System.Collections.Generic;

namespace Acceloka.Api.Infrastructure.Persistence.Entities;

public partial class Category
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
