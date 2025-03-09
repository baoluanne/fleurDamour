using System;
using System.Collections.Generic;

namespace fleurDamour.Models;

public partial class OrderDetail
{
    public string? Idorder { get; set; }

    public string? Idproduct { get; set; }

    public int? Quantity { get; set; }

    public double? Price { get; set; }

    public double? TotalPrice { get; set; }

    public virtual Order? IdorderNavigation { get; set; }

    public virtual Product? IdproductNavigation { get; set; }
}
