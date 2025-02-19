using System;
using System.Collections.Generic;

namespace fleurDamour.Models;

public partial class Order
{
    public string Idorder { get; set; } = null!;

    public int? Uid { get; set; }

    public DateTime? OrderDay { get; set; }

    public string? TotalPrice { get; set; }

    public virtual Account? UidNavigation { get; set; }

    public virtual ICollection<Account> Uids { get; set; } = new List<Account>();
}
