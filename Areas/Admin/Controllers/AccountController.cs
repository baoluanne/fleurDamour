using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using fleurDamour;
using fleurDamour.Models;
using System.Linq;
using System;

namespace fleurDamour.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class AccountController : Controller
	{

		FleurDamourContext db = new();

		private readonly ILogger<Account> _logger;

		public AccountController(ILogger<Account> logger)
		{
			_logger = logger;
		}

		[CheckAdminRole]
		[HttpGet]
		[Route("Account/GetUser")]
		public IActionResult GetUser(int userId)
		{
			try
			{
				var user = db.Accounts.SingleOrDefault(u => u.Uid == userId);
				if (user != null)
				{				
					return Ok(user);
				}

				return NotFound();
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}
	}
}