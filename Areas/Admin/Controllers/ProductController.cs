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
        public IActionResult Create(string Idproduct, string NameProduct, string ImgProduct, int Quantity, double Price, string InfoProduct, string Idcategory)
        {
            if (string.IsNullOrEmpty(Idproduct) || string.IsNullOrEmpty(NameProduct) || Price <= 0 || Quantity <= 0)
            {
                TempData["Error"] = "Please provide valid product details";
                return RedirectToAction("Index", "Product");
            }

            var product = db.Products.SingleOrDefault(s => s.Idproduct == Idproduct);
            if (product == null)
            {
                product = new Product
                {
                    Idproduct = Idproduct,
                    NameProduct = NameProduct,
                    ImgProduct = ImgProduct,
                    Price = Price,
                    Quantity = Quantity,
                    InfoProduct = InfoProduct
                };

                // Handle category
                if (!string.IsNullOrEmpty(Idcategory))
                {
                    var category = db.Categories.SingleOrDefault(c => c.Idcategory == Idcategory);
                    if (category != null)
                    {
                        product.Idcategories.Add(category);
                    }
                }

                db.Products.Add(product);
                db.SaveChanges();
                TempData["Success"] = "Product created successfully";
            }
            else
            {
                TempData["Error"] = "Product with this ID already exists";
            }

            return RedirectToAction("Index", "Product");
        }




        [CheckAdminRole]
        [HttpPost]
        public IActionResult Edit(string Idproduct, string NameProduct, string ImgProduct, double Price, int Quantity, string InfoProduct, string Idcategory)
        {
            if (string.IsNullOrEmpty(Idproduct))
            {
                TempData["Error"] = "Product not found for editing";
                return RedirectToAction("Index", "Product");
            }

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

                if (Price > 0)
                    product.Price = Price;

                if (Quantity >= 0)
                    product.Quantity = Quantity;

                if (!string.IsNullOrEmpty(InfoProduct))
                    product.InfoProduct = InfoProduct;

                if (!string.IsNullOrEmpty(Idcategory))
                {
                    var category = db.Categories.SingleOrDefault(c => c.Idcategory == Idcategory);
                    if (category == null)
                    {
                        TempData["Error"] = "Category does not exist";
                        return RedirectToAction("Index", "Product");
                    }

                    product.Idcategories.Clear();
                    product.Idcategories.Add(category);
                }

                db.Products.Update(product);
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
                .Include(p => p.ShoppingCarts)
                .Include(p => p.Idcategories)
                .SingleOrDefault(u => u.Idproduct == id);

            if (product == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction("Index", "Product");
            }

            try
            {
                // Begin transaction to ensure data integrity
                using (var transaction = db.Database.BeginTransaction())
                {
                    var shoppingCarts = db.ShoppingCarts.Where(sc => sc.Idproduct == id);
                    db.ShoppingCarts.RemoveRange(shoppingCarts);

                    var comments = db.Comments.Where(c => c.Idproduct == id);
                    db.Comments.RemoveRange(comments);

                    foreach (var category in product.Idcategories.ToList())
                    {
                        category.Idproducts.Remove(product);
                    }

                    db.Products.Remove(product);
                    db.SaveChanges();
                    transaction.Commit();
                }

                TempData["Success"] = "Product deleted successfully";
                return RedirectToAction("Index", "Product");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting product: {ex.Message}";
                return RedirectToAction("Index", "Product");
            }
        }

    }
}

