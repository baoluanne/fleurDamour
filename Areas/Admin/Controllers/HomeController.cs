using Microsoft.AspNetCore.Mvc;
using System.Linq;
using fleurDamour.Models;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace fleurDamour.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly FleurDamourContext db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, FleurDamourContext context)
        {
            _logger = logger;
            db = context;
        }

        [CheckAdminRole]
        public IActionResult Index()
        {
            return View();
        }

        [CheckAdminRole]
        public IActionResult UserManagement()
        {
            return View(db.Accounts.ToList());
        }

        [CheckAdminRole]
        [HttpPost]
        public async Task<IActionResult> Create(IFormFile PicAccount, int Uid, string UserName, string Password,
            string AccountName, string Role, string Email)
        {
            // Check if Uid already exists or if required fields are missing
            if (db.Accounts.Any(u => u.Uid == Uid))
            {
                TempData["Error"] = "User ID already exists";
                return RedirectToAction("UserManagement");
            }

            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
            {
                TempData["Error"] = "Username and Password are required";
                return RedirectToAction("UserManagement");
            }

            var user = new Account
            {
                Uid = Uid,
                UserName = UserName,
                Password = Password, // Consider hashing the password in production
                AccountName = AccountName,
                Role = Role ?? "User", // Default to "User" if Role is null
                Email = Email,
                LogDate = DateTime.Now
            };

            // Handle profile picture upload
            if (PicAccount != null && PicAccount.Length > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "image", "avatar");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var newFileName = $"{UserName}{Path.GetExtension(PicAccount.FileName)}";
                var fullPath = Path.Combine(uploadPath, newFileName);

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await PicAccount.CopyToAsync(fileStream);
                }
                user.PicAccount = $"/img/image/avatar/{newFileName}";
            }
            else
            {
                user.PicAccount = "/img/image/avatar/default.png"; // Default avatar
            }

            db.Accounts.Add(user);
            db.SaveChanges();

            TempData["Success"] = "User created successfully";
            return RedirectToAction("UserManagement");
        }

        [CheckAdminRole]
        [HttpPost]
        public async Task<IActionResult> Edit(string UserName, int Uid, IFormFile PicAccount,
            string AccountName, string Role, string Email)
        {
            var user = db.Accounts.SingleOrDefault(u => u.Uid == Uid);
            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction("UserManagement");
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(AccountName)) user.AccountName = AccountName;
            if (!string.IsNullOrEmpty(Role)) user.Role = Role;
            if (!string.IsNullOrEmpty(Email)) user.Email = Email;

            // Handle profile picture update
            if (PicAccount != null && PicAccount.Length > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "image", "avatar");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Delete old avatar if exists
                var existingFiles = Directory.GetFiles(uploadPath, $"{Uid}.*");
                foreach (var filePath in existingFiles)
                {
                    System.IO.File.Delete(filePath);
                }

                var newFileName = $"{Uid}{Path.GetExtension(PicAccount.FileName)}";
                var fullPath = Path.Combine(uploadPath, newFileName);

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await PicAccount.CopyToAsync(fileStream);
                }
                user.PicAccount = $"/img/image/avatar/{newFileName}";
            }

            db.Accounts.Update(user);
            db.SaveChanges();

            TempData["Success"] = "User updated successfully";
            return RedirectToAction("UserManagement");
        }

        [CheckAdminRole]
        [HttpPost]
        public IActionResult Delete(int Uid)
        {
            var user = db.Accounts
                .Include(u => u.Comments)
                .ThenInclude(c => c.CommentDetails)
                .Include(u => u.ShoppingCarts)
                .Include(u => u.Orders)
                .SingleOrDefault(u => u.Uid == Uid);

            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction("UserManagement");
            }

            try
            {
                // Remove related data
                db.CommentDetails.RemoveRange(db.CommentDetails.Where(cd => cd.Uid == Uid ||
                    db.Comments.Any(c => c.Idcomments == cd.Idcomments && c.Uid == Uid)));
                db.Comments.RemoveRange(db.Comments.Where(c => c.Uid == Uid));
                db.ShoppingCarts.RemoveRange(db.ShoppingCarts.Where(sc => sc.Uid == Uid));
                db.Orders.RemoveRange(db.Orders.Where(o => o.Uid == Uid));

                // Remove user
                db.Accounts.Remove(user);
                db.SaveChanges();

                TempData["Success"] = "User deleted successfully";

                // Check if the deleted user is the current logged-in user
                string currentUserName = HttpContext.Session.GetString("UserName");
                if (currentUserName == user.UserName)
                {
                    return RedirectToAction("Logout", "Account", new { Area = "User" });
                }

                return RedirectToAction("UserManagement");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting user: {ex.Message}";
                return RedirectToAction("UserManagement");
            }
        }
    }
}