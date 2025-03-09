using fleurDamour.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace fleurDamour.Controllers
{
    public class ProductController : Controller
    {
        private readonly FleurDamourContext _db;

        public ProductController(FleurDamourContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            var product = _db.Products.ToList();
            return View(product);
        }
        public IActionResult Category(string id)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var category = _db.Categories.SingleOrDefault(c => c.Idcategory == id);
            if (category == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(category);
        }
        public IActionResult Product(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index", "Home");

            var product = _db.Products
                .Include(p => p.Idcategories)
                .SingleOrDefault(p => p.Idproduct == id);
            if (product == null) return RedirectToAction("Index", "Home");

            return View(product);
        }

        [HttpPost]
        public IActionResult Comment(string Idproduct, string StrComments)
        {
            string userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName)) return RedirectToAction("Login", "Account");

            var user = _db.Accounts.SingleOrDefault(a => a.UserName == userName);
            if (user == null) return RedirectToAction("Product", new { id = Idproduct });

            string commentId = $"{user.UserName}_{Idproduct}_{DateTime.Now.Ticks}";
            var comment = new Comment
            {
                Idcomments = commentId,
                Uid = user.Uid,
                Idproduct = Idproduct,
                StrComments = StrComments,
                DateComments = DateTime.Now
            };
            _db.Comments.Add(comment);
            _db.SaveChanges();

            return RedirectToAction("Product", new { id = Idproduct });
        }

        public IActionResult RemoveComment(string productID, string commentID)
        {
            string userName = HttpContext.Session.GetString("UserName");
            var user = _db.Accounts.SingleOrDefault(a => a.UserName == userName);
            if (user == null) return RedirectToAction("Product", new { id = productID });

            var comment = _db.Comments
                .SingleOrDefault(c => c.Idcomments == commentID && c.Uid == user.Uid);
            if (comment != null)
            {
                _db.Comments.Remove(comment);
                _db.SaveChanges();
            }
            return RedirectToAction("Product", new { id = productID });
        }

        [HttpPost]
        public IActionResult AddToCart(string id, int quantity)
        {
            string userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
                return Json(new { success = false, message = "Please log in." });

            var user = _db.Accounts.SingleOrDefault(a => a.UserName == userName);
            var product = _db.Products.SingleOrDefault(p => p.Idproduct == id);
            if (user == null || product == null || quantity > product.Quantity)
                return Json(new { success = false, message = "Invalid request or insufficient stock." });

            var cartItem = _db.ShoppingCarts
                .SingleOrDefault(sc => sc.Uid == user.Uid && sc.Idproduct == id);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                if (cartItem.Quantity > product.Quantity)
                    return Json(new { success = false, message = "Exceeds available stock." });
                _db.ShoppingCarts.Update(cartItem);
            }
            else
            {
                _db.ShoppingCarts.Add(new ShoppingCart
                {
                    Uid = user.Uid,
                    Idproduct = id,
                    Quantity = quantity,
                    AddDate = DateTime.Now
                });
            }
            _db.SaveChanges();

            return Json(new { success = true, message = "Added to cart." });
        }

        [HttpPost]
        public IActionResult BuyNow(string id, int quantity)
        {
            string userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
                return Json(new { success = false, message = "Please log in." });

            var user = _db.Accounts.SingleOrDefault(a => a.UserName == userName);
            var product = _db.Products.SingleOrDefault(p => p.Idproduct == id);
            if (user == null || product == null || quantity > product.Quantity)
                return Json(new { success = false, message = "Invalid request or insufficient stock." });

            string orderId = Guid.NewGuid().ToString();
            var order = new Order
            {
                Idorder = orderId,
                Uid = user.Uid,
                OrderDay = DateTime.Now,
                TotalPrice = product.Price * quantity
            };
            _db.Orders.Add(order);
            _db.SaveChanges();

            return Json(new { success = true, redirectUrl = Url.Action("Checkout", "Order", new { orderId, productId = id, quantity }) });
        }

        public IActionResult ShoppingCart()
        {
            string userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName)) return RedirectToAction("Login", "Account");

            var user = _db.Accounts.SingleOrDefault(a => a.UserName == userName);
            var cartItems = _db.ShoppingCarts
                .Where(sc => sc.Uid == user.Uid)
                .ToList();
            foreach (var item in cartItems)
            {
                item.IdproductNavigation = _db.Products.SingleOrDefault(p => p.Idproduct == item.Idproduct);
            }

            return View(cartItems);
        }
    }
}