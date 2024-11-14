using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using StudentManagement.Models;
using StudentManagement.ViewModel; 
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.UI.Services;
using StudentManagement.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace StudentManagement.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<RegistrationModel> _userManager;
        private readonly SignInManager<RegistrationModel> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<RegistrationModel> userManager,
            SignInManager<RegistrationModel> signInManager,
            RoleManager<IdentityRole> roleManager,
              ILogger<AccountController> logger) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }


        public IActionResult Registration()
        {
            return View();
        }
		public IActionResult Login()
		{
			return View();
		}

      
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("","Email or Password incorrect");
                    return View(model);
                }
               
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegistrationViewModel model)
        {

            if (model == null)
            {
                return BadRequest("Model is null");
            }

            if (ModelState.IsValid)
            {
                var user = new RegistrationModel
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Address = model.Address,
                    GardianName = model.GardianName,
                    gender = model.gender,
                    DateOfBirth = model.DateOfBirth,
                    Age = model.Age,
                    PhoneNumber= model.PhoneNumber
                  
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _userManager.AddToRoleAsync(user, model.Role);

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    TempData["SuccessMessage"] = "Registration successful!";
                    return RedirectToAction("Registration");
              

                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    TempData["ErrorMessage"] = "Registration Not Success!";
                    return View(model);

                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return RedirectToAction("Registration");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login","Account");     
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid password reset token.";
                return RedirectToAction("ForgotPassword", "Account");
            }

      
            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userManager.FindByEmailAsync(model.Email).Result;
                if (user != null)
                {
                   
                    var token = _userManager.GeneratePasswordResetTokenAsync(user).Result;

                  
                    return RedirectToAction("ResetPassword", new { token, email = model.Email });

                }


                ModelState.AddModelError(string.Empty, "User not found.");
                TempData["ErrorMessage"] = "User not found.";
                return View(model);
            }

            return View(model);
        }

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
			
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user == null)
				{
				
					ModelState.AddModelError(string.Empty, "No user found with this email address.");
					return View(model);
				}

		
				var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);


				if (result.Succeeded)
				{
					
					TempData["SuccessMessage"] = "Password reset successfully!";
					return RedirectToAction("Login", "Account");
				}
				else
				{
			
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
				}
			}

		
			return View(model);
		}


	}
}
