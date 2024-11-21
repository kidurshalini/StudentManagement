﻿using System.Text;
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
                model.Class = await _context.Class
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
                model.Class = new List<SelectListItem>
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
                return BadRequest("Model is null");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model, model.GradeId);
                return View("Registration", model);
            }

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
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                if (await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }
                else
                {
                    ModelState.AddModelError("", "Selected role does not exist.");
                    await PopulateDropdowns(model, model.GradeId);
                    return View("Registration", model);
                }

                var classRegistration = new ClassRegistrationModel
                {
                    UserID = user.Id,
                    GradeId = model.GradeId,
                    ClassId = model.ClassId
                };
                _context.Add(classRegistration);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registration successful!";
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await PopulateDropdowns(model, model.GradeId);
            return View("Registration", model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
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
