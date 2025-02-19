using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using fleurDamour.Models;

namespace fleurDamour.Controllers
{
    public class AccountController : Controller
    {
        private readonly FleurDamourContext db = new();
		private readonly ILogger<AccountController> _logger;
		public AccountController(ILogger<AccountController> logger)
		{
			_logger = logger;
		}

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserName") != null)
            {
                return RedirectToAction("Index", "Home");
            }
			/*ViewData["HideFooter"] = true; */
			return View();
		}

        [HttpPost]
        public async Task<IActionResult> Login(string user, string pass)
        {
            var User = await db.Accounts.SingleOrDefaultAsync(a => a.UserName == user && a.Password == pass);
            if (User == null)
            {
                ViewBag.Error = "Invalid username or password";
                return View();
            }
            HttpContext.Session.SetString("UserName", User.UserName);
            HttpContext.Session.SetString("Role", User.Role);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]       
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string user, string pass, string email, string phone, string address, string accountname, string role, string confirmPassword)
        {
            var User = await db.Accounts.SingleOrDefaultAsync(a => a.UserName == user);
            if (User != null)
            {
                ViewBag.Error = "User already exists";
                return View();
            }
            if (string.IsNullOrEmpty(user))
            {
                ViewBag.Error = "Invalid User name";
                return View();
            }
            else if (string.IsNullOrEmpty(accountname))
            {
                ViewBag.Error = " Invalid Account name ";
                return View();
            }
            else if (string.IsNullOrEmpty(email) || !email.Contains("@") || !email.Contains("."))
            {
                ViewBag.Error = "Invalid email";
                return View();
            }
            else if (string.IsNullOrEmpty(phone) || phone.Length != 10)
            {
                ViewBag.Error = "Invalid phone number";
                return View();
            }
            else if (string.IsNullOrEmpty(address) == null)
            {
                ViewBag.Error = "Invalid address";
                return View();
            }
            else if( string.IsNullOrEmpty(pass) || pass.Length < 8)
            {
                ViewBag.Error = "Password must be at least 8 characters";
                return View();
            }
            else if (pass != confirmPassword )
            {
                ViewBag.Error = "Passwords do not match";
                return View();
            }

            var NewUser = new Account
            {
                UserName = user,
                Password = pass,
                Email = email,
                Phone = phone,
                Address = address,
                AccountName = accountname,
                Role = "User"
            };
            db.Accounts.Add(NewUser);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Message"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (userName == null) return RedirectToAction("Login");

            var user = await db.Accounts.SingleOrDefaultAsync(a => a.UserName == userName);
            if (user == null) return RedirectToAction("Index", "Home");

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(Account updatedAccount)
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (userName == null) return RedirectToAction("Login");

            var user = await db.Accounts.SingleOrDefaultAsync(a => a.UserName == userName);
            if (user == null) return RedirectToAction("Index", "Home");

            user.AccountName = updatedAccount.AccountName;
            user.Email = updatedAccount.Email;
            user.Address = updatedAccount.Address;
            user.Phone = updatedAccount.Phone;
            user.DateOfBirth = updatedAccount.DateOfBirth;

            db.Accounts.Update(user);
            await db.SaveChangesAsync();
            return RedirectToAction("Profile");
        }
    }
}
