using System;
using System.Collections.Generic;

namespace fleurDamour.Models;

public partial class CommentDetail
{
    public string CommentDetailsId { get; set; } = null!;

    public string? Idcomments { get; set; }

    public int? Uid { get; set; }

    public string? StrCommentDetail { get; set; }

    public DateTime? StrDateCommentDetail { get; set; }

    public virtual Comment? IdcommentsNavigation { get; set; }

    public virtual Account? UidNavigation { get; set; }
}
