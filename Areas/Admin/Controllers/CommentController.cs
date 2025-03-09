using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using fleurDamour.Models;
using System.Linq;

namespace fleurDamour.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class CommentController : Controller
    {
        FleurDamourContext db = new();

        private readonly ILogger<Comment> _logger;

        public CommentController(ILogger<Comment> logger)
        {
            _logger = logger;
        }

        [CheckAdminRole]
        public IActionResult Index()
        {
            return View(db.Comments.ToList());
        }

        [CheckAdminRole]
        public IActionResult Delete(string id)
        {
			var comment = db.Comments.SingleOrDefault(s => s.Idcomments == id);
			if (comment != null)
			{
				db.CommentDetails.RemoveRange(db.CommentDetails.Where(s => s.Idcomments == id));
				db.Comments.Remove(comment);
				db.SaveChanges();
			}
			return RedirectToAction("Index", "Comment");
		}
	}
}
