using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using fleurDamour.Models;
using fleurDamour.Utilities;
using System.Security.Claims;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace fleurDamour.Controllers
{
    public class AccountController : Controller
    {
        private readonly FleurDamourContext db;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string hunterApiKey = "099450bfef29521c183dbdaf3a31b8703da073c9";

        public AccountController(FleurDamourContext context, ILogger<AccountController> logger, IConfiguration configuration)
        {
            db = context;
            _logger = logger;
            _configuration = configuration;
            hunterApiKey = configuration["HunterIO:ApiKey"];
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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string user, string pass)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) )
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }
            string HashPass = Calculator.SHA256Hash(pass);
            var userAccount = db.Accounts.SingleOrDefault(m => m.UserName == user && (m.Password == HashPass || m.Password == pass));
           
            if (userAccount == null)
            {
                _logger.LogWarning("Login failed for username: {User}", user);
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            HttpContext.Session.SetString("UserName", userAccount.UserName);
            HttpContext.Session.SetString("Role", userAccount.Role);
            _logger.LogInformation("User {User} logged in successfully.", user);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult LoginWithGoogle(string returnUrl = "/")
        {
            if (HttpContext.Session.GetString("UserName") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleCallback", new { returnUrl }) };
            return Challenge(properties, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync("Cookies");
            if (!result.Succeeded)
            {
                _logger.LogError("Google login failed: {Reason}", result.Failure?.Message);
                ViewBag.Error = "Google login failed.";
                return View("Login");
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogError("Google login failed: Email not provided.");
                ViewBag.Error = "Google login failed: Email not provided.";
                return View("Login");
            }

            var user = await db.Accounts.SingleOrDefaultAsync(a => a.Email == email);
            if (user == null)
            {
                user = new Account
                {
                    UserName = email.Split('@')[0],
                    Email = email,
                    AccountName = name ?? "Google User",
                    Role = "User",
                    Password = Calculator.SHA256Hash(Guid.NewGuid().ToString()),
                    PicAccount = "/img/image/avatar/avatar.png",
                    Phone = "N/A",
                    Address = "N/A"
                };
                db.Accounts.Add(user);
                await db.SaveChangesAsync();
                _logger.LogInformation("New user created via Google login: {Email}", email);
            }

            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("Role", user.Role);
            _logger.LogInformation("User {Email} logged in via Google successfully.", email);

            return LocalRedirect(returnUrl);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserName") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string user, string pass, string email, string phone, string address, string accountname, string confirmPassword, string codeVerify)
        {
            if (HttpContext.Session.GetString("UserName") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra email đã tồn tại
            var existingEmail = await db.Accounts.SingleOrDefaultAsync(a => a.Email == email);
            if (existingEmail != null)
            {
                ViewBag.Error = "This email is already registered.";
                return View();
            }

            // Các kiểm tra khác
            var existingUser = await db.Accounts.SingleOrDefaultAsync(a => a.UserName == user);
            if (string.IsNullOrEmpty(user))
            {
                ViewBag.Error = "Invalid User name";
            }
            else if (string.IsNullOrEmpty(accountname))
            {
                ViewBag.Error = "Invalid Account name";
            }
            else if (string.IsNullOrEmpty(email) || !email.Contains("@") || !email.Contains("."))
            {
                ViewBag.Error = "Invalid email";
            }
            else if (string.IsNullOrEmpty(phone) || phone.Length != 10)
            {
                ViewBag.Error = "Invalid phone number";
            }
            else if (string.IsNullOrEmpty(address))
            {
                ViewBag.Error = "Invalid address";
            }
            else if (string.IsNullOrEmpty(pass) || pass.Length < 8)
            {
                ViewBag.Error = "Password must be at least 8 characters";
            }
            else if (pass != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
            }
            else if (existingUser != null)
            {
                ViewBag.Error = "User already exists";
            }
            else if (string.IsNullOrEmpty(HttpContext.Session.GetString("OtpCode")))
            {
                ViewBag.Error = "Please verify your email and wait for OTP code!";
            }
            else if (string.IsNullOrEmpty(codeVerify))
            {
                ViewBag.Error = "OTP Code is required!";
            }
            else if (codeVerify != HttpContext.Session.GetString("OtpCode"))
            {
                ViewBag.Error = "OTP Code is incorrect!";
            }
            else
            {
                var newUser = new Account
                {
                    UserName = user,
                    Password = Calculator.SHA256Hash(pass),
                    Email = email,
                    Phone = phone,
                    Address = address,
                    AccountName = accountname,
                    Role = "User",
                    PicAccount = "/img/image/avatar/avatar.png"
                };
                db.Accounts.Add(newUser);
                await db.SaveChangesAsync();

                // Xóa OTP sau khi sử dụng
                HttpContext.Session.Remove("OtpCode");
                _logger.LogInformation("User {User} registered successfully.", user);

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string user, string mail, string pass, string repass, string codeVerify)
        {
            if (HttpContext.Session.GetString("UserName") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrEmpty(user))
            {
                ViewBag.Error = "Username cannot be empty!";
            }
            else if (string.IsNullOrEmpty(mail))
            {
                ViewBag.Error = "Email is required!";
            }
            else if (string.IsNullOrEmpty(HttpContext.Session.GetString("OtpCode")))
            {
                ViewBag.Error = "Please verify your email and wait for OTP code!";
            }
            else if (string.IsNullOrEmpty(codeVerify))
            {
                ViewBag.Error = "OTP Code is required!";
            }
            else if (codeVerify != HttpContext.Session.GetString("OtpCode"))
            {
                ViewBag.Error = "OTP Code is incorrect!";
            }
            else if (string.IsNullOrEmpty(pass))
            {
                ViewBag.Error = "Password cannot be empty!";
            }
            else if (string.IsNullOrEmpty(repass))
            {
                ViewBag.Error = "Re-Password cannot be empty!";
            }
            else
            {
                var userAccount = await db.Accounts.SingleOrDefaultAsync(u => u.UserName == user && u.Email == mail);
                if (pass.Length < 8)
                {
                    ViewBag.Error = "Password must be at least 8 characters!";
                }
                else if (!pass.Equals(repass))
                {
                    ViewBag.Error = "Re-entered password is wrong!";
                }
                else if (userAccount == null)
                {
                    ViewBag.Error = "Account information is incorrect!";
                }
                else
                {
                    userAccount.Password = Calculator.SHA256Hash(pass);
                    db.Accounts.Update(userAccount);
                    await db.SaveChangesAsync();

                    // Xóa OTP sau khi sử dụng
                    HttpContext.Session.Remove("OtpCode");
                    _logger.LogInformation("Password reset successfully for user: {User}", user);

                    return RedirectToAction("Login", "Account");
                }
            }
            return View();
        }

        public IActionResult Logout()
        {
            var userName = HttpContext.Session.GetString("UserName");
            HttpContext.Session.Clear();
            TempData["Message"] = "Logged out successfully";
            _logger.LogInformation("User {User} logged out successfully.", userName);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (userName == null)
            {
                return RedirectToAction("Login");
            }

            var user = await db.Accounts.SingleOrDefaultAsync(a => a.UserName == userName);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(int Uid, string AccountName, string Email, string Phone, string Address, IFormFile PicAccount)
        {
            var user = db.Accounts.SingleOrDefault(u => u.Uid == Uid);
            if (user == null)
            {
                _logger.LogWarning("User with Uid {Uid} not found.", Uid);
                return RedirectToAction("Profile");
            }

            if (!string.IsNullOrEmpty(AccountName))
            {
                user.AccountName = AccountName;
            }
            if (!string.IsNullOrEmpty(Email))
            {
                user.Email = Email;
            }
            if (!string.IsNullOrEmpty(Phone))
            {
                user.Phone = Phone;
            }
            if (!string.IsNullOrEmpty(Address))
            {
                user.Address = Address;
            }

            if (PicAccount != null && PicAccount.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                var extension = Path.GetExtension(PicAccount.FileName).ToLower();

                if (allowedExtensions.Contains(extension))
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "image", "avatar");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }
                    var existingFiles = Directory.GetFiles(uploadPath, $"{user.Uid}.*");
                    foreach (var filePath in existingFiles)
                    {
                        System.IO.File.Delete(filePath);
                    }
                    var newFileName = $"{user.Uid}{extension}";
                    var fullPath = Path.Combine(uploadPath, newFileName);
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await PicAccount.CopyToAsync(fileStream);
                    }
                    user.PicAccount = Path.Combine("/img/image/avatar", newFileName);
                }
                else
                {
                    user.PicAccount = "/img/image/avatar/avatar.png";
                }
            }

            db.SaveChanges();
            _logger.LogInformation("Profile updated for user with Uid: {Uid}", Uid);
            return RedirectToAction("Profile");
        }

        // Gửi email chứa mã OTP
        private async Task SendRegistrationConfirmationEmail(string email, string username)
        {
            var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_configuration["EmailSettings:SmtpPort"]),
                Credentials = new NetworkCredential(_configuration["EmailSettings:SenderEmail"], _configuration["EmailSettings:SenderPassword"]),
                EnableSsl = true,
            };

            Random rant = new Random();
            string otpCode = rant.Next(100000, 1000000).ToString();
            HttpContext.Session.SetString("OtpCode", otpCode);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:SenderEmail"]),
                Subject = "Registration Confirmation",
                Body = $"<h1>Welcome {username}!</h1>" +
                       $"<h2>Your OTP code: {otpCode}</h2>" +
                       $"<h3>Thank you for registering.</h3>",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("OTP email sent to {Email}.", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}.", email);
                throw;
            }
        }

        // Kiểm tra email hợp lệ bằng Hunter.io
        private async Task<bool> CheckEmailExists(string email)
        {
            string requestUrl = $"https://api.hunter.io/v2/email-verifier?email={email}&api_key={hunterApiKey}";

            using (var client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(result))
                        {
                            _logger.LogWarning("Empty response from Hunter.io API for email: {Email}", email);
                            return false;
                        }

                        dynamic jsonResponse = JObject.Parse(result);
                        return jsonResponse.data?.result?.ToString() == "deliverable";
                    }
                    else
                    {
                        _logger.LogWarning("Hunter.io API returned status code: {StatusCode} for email: {Email}", response.StatusCode, email);
                        return false;
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Error connecting to Hunter.io API for email: {Email}", email);
                    return false;
                }
                catch (JsonReaderException ex)
                {
                    _logger.LogError(ex, "Invalid JSON response from Hunter.io API for email: {Email}", email);
                    return false;
                }
            }
        }

        // Xác nhận OTP qua Ajax
        [HttpPost]
        public async Task<IActionResult> VerifyCodeAjax(string username, string email)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Username and email are required." });
            }

            bool isEmailValid = await CheckEmailExists(email);
            if (!isEmailValid)
            {
                return Json(new { success = false, message = "Invalid email!" });
            }

            try
            {
                await SendRegistrationConfirmationEmail(email, username);
                return Json(new { success = true, message = "Verification email sent!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}", email);
                return Json(new { success = false, message = "Failed to send verification email." });
            }
        }
    }
}