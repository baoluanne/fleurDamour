using System;
using System.Collections.Generic;

namespace fleurDamour.Models;

public partial class Account
{
    public int Uid { get; set; }

    public string? UserName { get; set; }

    public string? AccountName { get; set; }

    public string? PicAccount { get; set; }

    public string? Password { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Role { get; set; }

    public DateTime? LogDate { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();

    public virtual ICollection<Order> Idorders { get; set; } = new List<Order>();
}
