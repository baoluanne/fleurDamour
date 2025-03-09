using System;
using System.Collections.Generic;

namespace fleurDamour.Models;

public partial class Comment
{
    public string Idcomments { get; set; } = null!;

    public int? Uid { get; set; }

    public string? Idproduct { get; set; }

    public string? StrComments { get; set; }

    public DateTime? DateComments { get; set; }

    public virtual ICollection<CommentDetail> CommentDetails { get; set; } = new List<CommentDetail>();

    public virtual Product? IdproductNavigation { get; set; }

    public virtual Account? UidNavigation { get; set; }
}
