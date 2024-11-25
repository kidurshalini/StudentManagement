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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace StudentManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<RegistrationModel> _userManager;
        private readonly SignInManager<RegistrationModel> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;

        public AccountController(
         UserManager<RegistrationModel> userManager,
         SignInManager<RegistrationModel> signInManager,
         RoleManager<IdentityRole> roleManager,
         ILogger<AccountController> logger,
         ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context;
        }

        private async Task PopulateDropdowns(RegistrationViewModel model, Guid? selectedGradeId = null)
        {
            model.Grades = await _context.Grades
                .Select(g => new SelectListItem
                {
                    Value = g.ID.ToString(),
                    Text = $"Grade {g.Grade}"
                })
                .ToListAsync();

            if (selectedGradeId.HasValue)
            {
                model.Classes = await _context.Class
                    .Where(c => c.GradeId == selectedGradeId.Value) // Filter classes by selected grade
                    .Select(c => new SelectListItem
                    {
                        Value = c.ID.ToString(),
                        Text = $"Class {c.Class}"
                    })
                    .ToListAsync();
            }
            else
            {
                model.Classes = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Select Class --" }
        };
            }
        }


        [HttpGet]
        public async Task<IActionResult> Registration()
        {
            var viewModel = new RegistrationViewModel();
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateClasses(Guid gradeId)
        {
            // Ensure gradeId is valid
            if (gradeId == Guid.Empty)
            {
                return Json(new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- No Classes Available --" }
        });
            }

            // Fetch the classes associated with the grade
            var classes = await _context.Class
                .Where(c => c.GradeId == gradeId) // Filter by GradeId
                .Select(c => new SelectListItem
                {
                    Value = c.ID.ToString(),
                    Text = c.Class // Use the correct property name for class name
                })
                .ToListAsync();

            if (!classes.Any())
            {
                // Return a default message if no classes are found
                classes.Add(new SelectListItem { Value = "", Text = "-- No Classes Available --" });
            }

            return Json(classes);
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
                    ModelState.AddModelError("", "Email or Password incorrect");
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
                // This ensures that if the model is null, we return an error.
                return BadRequest("Model is null");
            }

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email is already in use.");
                return View("Registration", model);
            }

            // Create the user object (RegistrationModel)
            var user = new RegistrationModel
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Address = model.Address,
                gender = model.gender,
                DateOfBirth = model.DateOfBirth,
                Age = model.Age,
                PhoneNumber = model.PhoneNumber
            };

            // Create the user in Identity
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                // Assign role to the user
                if (!string.IsNullOrWhiteSpace(model.Role) && await _roleManager.RoleExistsAsync(model.Role))
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                    if (!roleResult.Succeeded)
                    {
                        // Handle any errors in adding to role
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                        // If role assignment fails, delete the user and return the view
                        await _userManager.DeleteAsync(user);
                        return View("Registration", model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Selected role does not exist.");
                    return View("Registration", model);
                }

                // After user creation, check for student role and create class registration
                if (model.Role == "Student")
                {
                    user = await _userManager.FindByEmailAsync(user.Email); // Reload user to get the assigned Id

                    if (user == null)
                    {
                        // If user is still null, return an error (edge case handling)
                        ModelState.AddModelError("", "Error finding user after creation.");
                        return View("Registration", model);
                    }

                    // Create the ClassRegistrationModel (academic details)
                    var classRegistration = new ClassRegistrationModel
                    {
                        UserID = user.Id,  // Now user.Id is guaranteed to be assigned
                        GradeId = model.GradeId,
                        ClassId = model.ClassId,
                    };

                    // Add the class registration to the database
                    await _context.UserAcadamic.AddAsync(classRegistration);
                    await _context.SaveChangesAsync(); // Make sure to save the changes to the DB
                }

                TempData["SuccessMessage"] = "Registration successful!";
                return RedirectToAction("Registration", "Account");
            }

            // If the creation fails, show error messages
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            TempData["ErrorMessage"] = "Registration failed.";
            return View("Registration", model);
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
        public IActionResult ForgotPassword()
        {
            return View();
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


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }


    }
}