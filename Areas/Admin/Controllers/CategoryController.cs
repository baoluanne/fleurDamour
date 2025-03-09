using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using fleurDamour;
using fleurDamour.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace fleurDamour.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {

        FleurDamourContext db = new();

        private readonly ILogger<Category> _logger;

        public CategoryController(ILogger<Category> logger)
        {
            _logger = logger;
        }

        [CheckAdminRole]
        public IActionResult Index()
        {
            return View(db.Categories.ToList());
        }
        [CheckAdminRole]
        [HttpPost]
        public IActionResult Create(string Idcategory, string NameCategory, string PicCategory)
        {

            if (Idcategory != null)
            {
                var cate = db.Categories.SingleOrDefault(s => s.Idcategory == Idcategory);
                if (cate == null)
                {
                    cate = new Category();
                    if (Idcategory != null) { cate.Idcategory = Idcategory; }
                    if (NameCategory != null) { cate.NameCategory = NameCategory; }
                    if (PicCategory != null) { cate.PicCategory = PicCategory; }
                    db.Categories.Add(cate);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Category");
        }

        [CheckAdminRole]
        [HttpPost]
        public IActionResult Edit(string Idcategory, string NameCategory, string PicCategory)
        {
            var cate = db.Categories.SingleOrDefault(s => s.Idcategory == Idcategory);
            if (cate != null)
            {
                if (Idcategory != null) { cate.Idcategory = Idcategory; }
                if (NameCategory != null) { cate.NameCategory = NameCategory; }
                if (PicCategory != null) { cate.PicCategory = PicCategory; }
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Category");
        }


        [CheckAdminRole]
        public IActionResult Delete(string id)
        {
            var cate = db.Categories
                .Include(c => c.Idproducts) 
                .SingleOrDefault(s => s.Idcategory == id);

            if (cate != null)
            {
                cate.Idproducts.Clear(); 
                db.Categories.Remove(cate); 
                db.SaveChanges(); 
            }

            return RedirectToAction("Index", "Category");
        }
    }
}

