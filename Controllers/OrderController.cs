using fleurDamour.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace fleurDamour.Controllers
{
    public class OrderController : Controller
    {
        private readonly FleurDamourContext _db;

        public OrderController(FleurDamourContext db)
        {
            _db = db;
        }

        public IActionResult Checkout(string orderId, string productId, int quantity)
        {
            var order = _db.Orders.SingleOrDefault(o => o.Idorder == orderId);
            var product = _db.Products.SingleOrDefault(p => p.Idproduct == productId);
            var user = _db.Accounts.SingleOrDefault(a => a.Uid == order.Uid);

            if (order == null || product == null || user == null)
                return NotFound();

            ViewBag.Order = order;
            ViewBag.Product = product;
            ViewBag.Quantity = quantity;
            ViewBag.User = user;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmCheckout(string orderId, string productId, int quantity, string newAddress, string newPhone, string newReceiver)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Idorder == orderId);
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Idproduct == productId);
            var user = await _db.Accounts.FirstOrDefaultAsync(a => a.Uid == order.Uid);

            if (order == null || product == null || user == null || product.Quantity < quantity)
                return BadRequest("Invalid order, user, or insufficient stock.");

            // Cập nhật thông tin người nhận nếu có thay đổi
            if (!string.IsNullOrWhiteSpace(newAddress)) user.Address = newAddress;
            if (!string.IsNullOrWhiteSpace(newPhone)) user.Phone = newPhone;
            if (!string.IsNullOrWhiteSpace(newReceiver)) user.AccountName = newReceiver;
            _db.Accounts.Update(user);

            // Giảm số lượng sản phẩm
            product.Quantity -= quantity;
            _db.Products.Update(product);

            // Thêm OrderDetail bằng SQL trực tiếp
            await _db.Database.ExecuteSqlRawAsync(
                "INSERT INTO OrderDetails (Idorder, Idproduct, Quantity, Price, TotalPrice) VALUES (@p0, @p1, @p2, @p3, @p4)",
                orderId, productId, quantity, product.Price, product.Price * quantity
            );

            await _db.SaveChangesAsync();

            // Trả về JSON với thông báo thành công
            return Json(new { success = true, message = "Đơn hàng đã được xác nhận thành công!" });
        }



    }
}
