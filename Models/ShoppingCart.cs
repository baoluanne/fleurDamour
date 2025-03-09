using System;
using System.Collections.Generic;

namespace fleurDamour.Models;

public partial class ShoppingCart
{
    public int Uid { get; set; }

    public string Idproduct { get; set; } = null!;

    public int? Quantity { get; set; }

    public DateTime? AddDate { get; set; }

    public virtual Product IdproductNavigation { get; set; } = null!;

    public virtual Account UidNavigation { get; set; } = null!;
}
