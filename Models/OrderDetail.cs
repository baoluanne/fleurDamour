using System;
using System.Collections.Generic;

namespace fleurDamour.Models;

public partial class OrderDetail
{
    public string? Idorder { get; set; }

    public string? Idproduct { get; set; }

    public string? Quantity { get; set; }

    public string? Price { get; set; }

    public string? TotalPrice { get; set; }

    public virtual Order? IdorderNavigation { get; set; }

    public virtual Product? IdproductNavigation { get; set; }
}
