using System;
using System.Collections.Generic;

namespace fleurDamour.Models;

public partial class Category
{
    public string Idcategory { get; set; } = null!;

    public string? NameCategory { get; set; }

    public string? PicCategory { get; set; }

    public virtual ICollection<Product> Idproducts { get; set; } = new List<Product>();
}
