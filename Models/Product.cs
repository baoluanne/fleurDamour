using System;
using System.Collections.Generic;

namespace fleurDamour.Models;

public partial class Product
{
    public string Idproduct { get; set; } = null!;

    public string? NameProduct { get; set; }

    public string? InfoProduct { get; set; }

    public string? ImgProduct { get; set; }

    public double? Price { get; set; }

    public int? Quantity { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();

    public virtual ICollection<Category> Idcategories { get; set; } = new List<Category>();
}
