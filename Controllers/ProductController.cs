using fleurDamour.Models;
using Microsoft.AspNetCore.Mvc;

namespace fleurDamour.Controllers
{
    public class ProductController : Controller
    {
        FleurDamourContext db = new();
        public IActionResult Index()
        {
            return View(db.Products.ToList());
        }
        public IActionResult Category(string id)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var category = db.Categories.SingleOrDefault(c => c.Idcategory == id);
            if (category == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(category);
        }
    }
}
