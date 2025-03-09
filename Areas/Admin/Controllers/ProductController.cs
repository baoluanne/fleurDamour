using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using fleurDamour;
using fleurDamour.Models;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections;
using Microsoft.JSInterop;

namespace fleurDamour.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProductController : Controller
	{

		FleurDamourContext db = new();

		private readonly ILogger<ProductController> _logger;

		public ProductController(ILogger<ProductController> logger)
		{
			_logger = logger;
		}

		[CheckAdminRole]
		public IActionResult Index()
		{
			return View(db.Products.ToList());
		}

		[CheckAdminRole]
		[HttpPost]
		public IActionResult Create(string Idproduct, string NameProduct, string ImgProduct, int Quantity,  double Price, string InfoProduct, string Idcategory)
		{

			
			if (Idproduct != null)
			{
				var product = db.Products.SingleOrDefault(s => s.Idproduct == Idproduct);
				if (product == null)
				{
					product = new Product();
					if (Idproduct != null) { product.Idproduct = Idproduct; }
					if (NameProduct != null) { product.NameProduct = NameProduct; }
					if (ImgProduct != null) { product.ImgProduct = ImgProduct; }
					if (Price != null) { product.Price = Price; }
					if (InfoProduct != null) { product.InfoProduct = InfoProduct; }
					if(Idcategory != null) { product.Idcategories.SingleOrDefault(s => s.Idcategory == Idcategory); }
					db.Products.Add(product);
					db.SaveChanges();
				}
			}
			return RedirectToAction("Index", "Product");
		}



        [CheckAdminRole]
        [HttpPost]
        public IActionResult Edit(string Idproduct, string NameProduct, string ImgProduct, double Price,
    int Quantity, string InfoProduct, string Idcategory)
        {
            // Check if Idproduct is valid
            if (string.IsNullOrEmpty(Idproduct))
            {
                TempData["Error"] = "Product not found for editing";
                return RedirectToAction("Index", "Product");
            }

            // Retrieve the product with its current categories
            var product = db.Products
                .Include(p => p.Idcategories)
                .SingleOrDefault(u => u.Idproduct == Idproduct);

            if (product == null)
            {
                TempData["Error"] = "Product does not exist";
                return RedirectToAction("Index", "Product");
            }

            try
            {
                // Update properties if new values are provided
                if (!string.IsNullOrEmpty(NameProduct))
                    product.NameProduct = NameProduct;

                if (!string.IsNullOrEmpty(ImgProduct))
                    product.ImgProduct = ImgProduct;

                if (Price > 0) // Validate price
                    product.Price = Price;

                if (Quantity >= 0) // Validate quantity
                    product.Quantity = Quantity;

                if (!string.IsNullOrEmpty(InfoProduct))
                    product.InfoProduct = InfoProduct;

                // Handle category update
                if (!string.IsNullOrEmpty(Idcategory))
                {
                    var category = db.Categories.SingleOrDefault(c => c.Idcategory == Idcategory);
                    if (category == null)
                    {
                        TempData["Error"] = "Category does not exist";
                        return RedirectToAction("Index", "Product");
                    }

                    // Remove existing categories if any
                    if (product.Idcategories.Any())
                    {
                        product.Idcategories.Clear();
                    }

                    // Add new category
                    product.Idcategories.Add(category);
                }

                db.SaveChanges();
                TempData["Success"] = "Product updated successfully";
                return RedirectToAction("Index", "Product");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating product: {ex.Message}";
                return RedirectToAction("Index", "Product");
            }
        }


        [CheckAdminRole]
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction("Index", "Product");
            }

            var product = db.Products
                .Include(p => p.Comments)
                .ThenInclude(c => c.CommentDetails)
                .Include(p => p.ShoppingCarts)
                .Include(p => p.Idcategories)
                .SingleOrDefault(u => u.Idproduct == id);

            if (product == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction("Index", "Index");
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var shoppingCarts = db.ShoppingCarts.Where(sc => sc.Idproduct == id);
                    db.ShoppingCarts.RemoveRange(shoppingCarts);

                    var commentDetails = db.CommentDetails
                        .Where(cd => db.Comments.Any(c => c.Idcomments == cd.Idcomments && c.Idproduct == id));
                    db.CommentDetails.RemoveRange(commentDetails);

                    var comments = db.Comments.Where(c => c.Idproduct == id);
                    db.Comments.RemoveRange(comments);

                    foreach (var category in product.Idcategories.ToList())
                    {
                        category.Idproducts.Remove(product);
                    }

                    db.Products.Remove(product);

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Delete successfully";
                    return RedirectToAction("Index", "Product");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = $"Error: {ex.Message}";
                    return RedirectToAction("Index", "Product");
                }
            }
        }
    }
}

