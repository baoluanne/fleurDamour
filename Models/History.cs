using System;
using System.Collections.Generic;

namespace fleurDamour.Models;

public partial class History
{
    public int Idhistory { get; set; }

    public int? Uid { get; set; }

    public string? Idorder { get; set; }

    public DateTime? OrderDay { get; set; }

    public string? TotalPrice { get; set; }

    public virtual Order? IdorderNavigation { get; set; }

    public virtual Account? UidNavigation { get; set; }
}
