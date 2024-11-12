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
                    Age = model.Age
                  
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
                    return View("~/Views/Home/Index.cshtml");

                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return RedirectToAction("Registration");
        }
    }
}
