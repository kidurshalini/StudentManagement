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
using System.Linq;

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
                        Value = c.Id.ToString(),
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
                    Value = c.Id.ToString(),
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

        //[HttpPost]
        //public async Task<IActionResult> Login(LoginViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

        //        if (result.Succeeded)
        //        {
        //            return RedirectToAction("Index", "Home");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", "Email or Password incorrect");
        //            return View(model);
        //        }
        //    }
        //    return View(model);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Return the same view with errors if model validation fails.
            }

            // Attempt to sign in the user.
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Fetch the user details.
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // Store user details in session
                    HttpContext.Session.SetString("UserId", user.Id);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("FullName", user.FullName); // Assuming `FullName` exists in `RegistrationModel`.

                    // Redirect to the desired page (e.g., Home or Dashboard).
                    return RedirectToAction("Index", "Home");
                }

                // Handle the unlikely case where user is null.
                ModelState.AddModelError("", "An error occurred while processing your login. Please try again.");
                return View(model);
            }
            else if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Your account is locked out. Please try again later.");
            }
            else if (result.RequiresTwoFactor)
            {
                return RedirectToAction("SendCode", new { ReturnUrl = Url.Action("Index", "Home"), RememberMe = model.RememberMe });
            }
            else
            {
                ModelState.AddModelError("", "Invalid login attempt. Please check your email and password.");
            }

            return View(model); // Return the view with errors if login fails.
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
                PhoneNumber = model.PhoneNumber,
                GardianName = model.GardianName
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

        [HttpGet]
        public async Task<IActionResult> TeacherView()
        {
            var teacherRole = "Teacher";
            var teachers = await _userManager.GetUsersInRoleAsync(teacherRole);

            var teacherViewModels = teachers.Select(user => new RegistrationViewModel
            {
                ID = Guid.TryParse(user.Id, out var guidId) ? guidId : Guid.Empty,
                Email = user.Email,
                FullName = user.FullName,
                gender = user.gender,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth


            }).ToList();

            return View(teacherViewModels);
        }

		//[HttpGet]
		//public async Task<IActionResult> StudentView()
		//{
		//    var StudentRole = "Student";

		//    // Get all students in the "Student" role
		//    var students = await _userManager.GetUsersInRoleAsync(StudentRole);

		//    // Fetch ClassRegistration details for these students
		//    var studentIds = students.Select(s => s.Id).ToList();

		//    var classRegistrations = await _context.UserAcadamic
		//        .Where(cr => studentIds.Contains(cr.UserID)) // No need for Select() here
		//        .Include(cr => cr.Grades)  // Include Grade details
		//        .Include(cr => cr.Class)   // Include Class details
		//        .ToListAsync();

		//    // Map the data into ViewModels
		//    var studentViewModels = students.Select(user =>
		//    {
		//        // Find the class registration details for the current user
		//        var classRegistration = classRegistrations.FirstOrDefault(cr => cr.UserID == user.Id);

		//        return new RegistrationViewModel
		//        {
		//            ID = Guid.TryParse(user.Id, out var studentId) ? studentId : Guid.Empty,
		//            Email = user.Email,
		//            FullName = user.FullName,
		//            gender = user.gender,
		//            PhoneNumber = user.PhoneNumber,
		//            Address = user.Address,
		//            DateOfBirth = user.DateOfBirth,
		//            Grades = new List<SelectListItem>
		//    {
		//        new SelectListItem
		//        {
		//            Value = classRegistration?.Grades?.ID.ToString(),
		//            Text = classRegistration?.Grades?.Grade.ToString() ?? "N/A"
		//        }
		//    },
		//            Classes = new List<SelectListItem>
		//    {
		//        new SelectListItem
		//        {
		//            Value = classRegistration?.Class?.Id.ToString(),
		//            Text = classRegistration?.Class?.Class ?? "N/A"
		//        }
		//    }
		//        };
		//    }).ToList();

		//    return View(studentViewModels);
		//}

		[HttpGet]
		public async Task<IActionResult> StudentView()
		{
			var studentRole = "Student";

			// Get all students in the "Student" role
			var students = await _userManager.GetUsersInRoleAsync(studentRole);

			// Fetch ClassRegistration details for these students
			var studentIds = students.Select(s => s.Id).ToList();
			var classRegistrations = await _context.UserAcadamic
				.Where(cr => studentIds.Contains(cr.UserID))
				.Include(cr => cr.Grades)  // Include Grade details
				.Include(cr => cr.Class)   // Include Class details
				.ToListAsync();

			// Map the data into ViewModels
			var studentViewModels = students.Select(user =>
			{
				// Find the class registration details for the current user
				var classRegistration = classRegistrations.FirstOrDefault(cr => cr.UserID == user.Id);

				return new RegistrationViewModel
				{
					ID = Guid.TryParse(user.Id, out var studentId) ? studentId : Guid.Empty,

					Email = user.Email,
					FullName = user.FullName,
					gender = user.gender,
					PhoneNumber = user.PhoneNumber,
					Address = user.Address,
					DateOfBirth = user.DateOfBirth,
                    GardianName = user.GardianName,
					Grades = new List<SelectListItem>
			{
				new SelectListItem
				{
					Value = classRegistration?.Grades?.ID.ToString(),
					Text = classRegistration?.Grades?.Grade.ToString() ?? "N/A"
				}
			},
					Classes = new List<SelectListItem>
			{
				new SelectListItem
				{
					Value = classRegistration?.Class?.Id.ToString(),
					Text = classRegistration?.Class?.Class ?? "N/A"
				}
			},
					GradeId = classRegistration?.GradeId ?? Guid.Empty,  // Set GradeId
					ClassId = classRegistration?.ClassId ?? Guid.Empty   // Set ClassId
				};
			}).ToList();

			return View(studentViewModels);
		}

		[HttpGet]
		public IActionResult TeacherEdit(string id)
		{
			var RegistrationViewModel = _context.Users
				.Where(u => u.Id == id)
				.Select(u => new RegistrationViewModel
				{
					
					FullName = u.FullName,
					Email = u.Email,
			    Password = u.PasswordHash,
					gender = u.gender,
					PhoneNumber = u.PhoneNumber,
					Address = u.Address,
					DateOfBirth = u.DateOfBirth,
					GardianName = u.GardianName,
				
					
				})
				.FirstOrDefault();

			if (RegistrationViewModel == null)
			{
				TempData["ErrorMessage"] = "Teacher not found.";
				return RedirectToAction("TeacherList"); // Redirect to the teacher list page if not found
			}

			return View(RegistrationViewModel);
		}

        [HttpPost]
        public IActionResult TeacherEdit(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the existing teacher from the database
                var existingTeacherRecord = _context.Users.FirstOrDefault(u => u.Id == model.ID.ToString());

                if (existingTeacherRecord == null)
                {
                    TempData["ErrorMessage"] = "Teacher not found.";
                    return RedirectToAction("TeacherList"); // Redirect to the teacher list page if not found
                }

                // Check if the email has changed and if it exists in the database
                if (existingTeacherRecord.Email != model.Email)
                {
                    var emailExists = _context.Users.Any(u => u.Email == model.Email);
                    if (emailExists)
                    {
                        ModelState.AddModelError("Email", "The email address is already taken.");
                        return View(model); // Return to the view with the validation error
                    }
                }

                if (model.Role == "Student" && !string.IsNullOrEmpty(model.GardianName))
                {
                    existingTeacherRecord.GardianName = model.GardianName;
                }

                // If a new password is provided, hash and update it
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var passwordHasher = new PasswordHasher<IdentityUser>();
                    existingTeacherRecord.PasswordHash = passwordHasher.HashPassword(existingTeacherRecord, model.Password);
                }

                // Update the teacher's properties
                existingTeacherRecord.FullName = model.FullName;
                existingTeacherRecord.Email = model.Email;
                existingTeacherRecord.gender = model.gender;
                existingTeacherRecord.PhoneNumber = model.PhoneNumber;
                existingTeacherRecord.Address = model.Address;
                existingTeacherRecord.DateOfBirth = model.DateOfBirth;
          

                _context.Users.Update(existingTeacherRecord);
                _context.SaveChanges();

              
                return RedirectToAction("TeacherView"); // Redirect to teacher view page after successful update
            }

            return View(model); // Return to the view if the model is invalid
        }

        [HttpGet]
        public IActionResult TeacherDelete(string id)
        {
            var RegistrationViewModel = _context.Users
                .Where(u => u.Id == id)
                .Select(u => new RegistrationViewModel
                {

                    FullName = u.FullName,
                    Email = u.Email,
                    Password = u.PasswordHash,
                    gender = u.gender,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    DateOfBirth = u.DateOfBirth,
                    GardianName = u.GardianName,


                })
                .FirstOrDefault();

            if (RegistrationViewModel == null)
            {
                TempData["ErrorMessage"] = "Teacher not found.";
                return RedirectToAction("TeacherList"); // Redirect to the teacher list page if not found
            }

            return View(RegistrationViewModel);
        }

        [HttpPost]
        public IActionResult TeacherDelete(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the existing teacher from the database
                var existingTeacherRecord = _context.Users.FirstOrDefault(u => u.Id == model.ID.ToString());

                if (existingTeacherRecord == null)
                {
                    TempData["ErrorMessage"] = "Teacher not found.";
                    return RedirectToAction("TeacherList"); // Redirect to the teacher list page if not found
                }

             

                _context.Users.Remove(existingTeacherRecord);
                _context.SaveChanges();


                return RedirectToAction("TeacherView"); // Redirect to teacher view page after successful update
            }

            return View(model); // Return to the view if the model is invalid
        }

        [HttpGet]
        public IActionResult StudentEdit(string id)
        {
            var RegistrationViewModel = (from u in _context.Users
                                         where u.Id == id
                                         join ua in _context.UserAcadamic on u.Id equals ua.UserID into academicData
                                         from ua in academicData.DefaultIfEmpty()
                                         select new RegistrationViewModel
                                         {
                                             ID = Guid.Parse(u.Id),
                                             FullName = u.FullName,
                                             Email = u.Email,
                                             Password = u.PasswordHash,
                                             gender = u.gender,
                                             PhoneNumber = u.PhoneNumber,
                                             Address = u.Address,
                                             DateOfBirth = u.DateOfBirth,
                                             GardianName = u.GardianName,
                                             GradeId = ua.GradeId,
                                             ClassId = ua.ClassId
                                         }).FirstOrDefault();

            if (RegistrationViewModel == null)
            {
                TempData["ErrorMessage"] = "Student not found.";

				return RedirectToAction("StudentView");
			}

            RegistrationViewModel.Grades = _context.Grades
                .Select(g => new SelectListItem { Value = g.ID.ToString(), Text = g.Grade.ToString() }).ToList();

            RegistrationViewModel.Classes = _context.Class
                .Where(c => c.GradeId == RegistrationViewModel.GradeId)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Class }).ToList();

            return View(RegistrationViewModel);
        }

        [HttpPost]
        public IActionResult StudentEdit(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Id == model.ID.ToString());
                if (existingUser == null)
                {
                    TempData["ErrorMessage"] = "Student not found.";

					return RedirectToAction("StudentView");
				}

                // Update user information
                existingUser.FullName = model.FullName;
                existingUser.Email = model.Email;
                existingUser.gender = model.gender;
                existingUser.PhoneNumber = model.PhoneNumber;
                existingUser.Address = model.Address;
                existingUser.DateOfBirth = model.DateOfBirth;
                existingUser.GardianName = model.GardianName;

                // If a new password is provided, update it
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var passwordHasher = new PasswordHasher<IdentityUser>();
                    existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, model.Password);
                }

                _context.Users.Update(existingUser);

                // Update or create UserAcademic entry
                var existingAcademicRecord = _context.UserAcadamic.FirstOrDefault(ua => ua.UserID == existingUser.Id);
                if (existingAcademicRecord == null)
                {
                    _context.UserAcadamic.Add(new ClassRegistrationModel
                    {
                        UserID = existingUser.Id, // No need for ToString() as it's already a string
                        GradeId = model.GradeId,
                        ClassId = model.ClassId
                    });
                }
                else
                {
                    existingAcademicRecord.GradeId = model.GradeId;
                    existingAcademicRecord.ClassId = model.ClassId;
                    _context.UserAcadamic.Update(existingAcademicRecord);
                }

                _context.SaveChanges();

                return RedirectToAction("StudentView");
            }

            // Reload grade and class dropdowns if the model state is invalid
            model.Grades = _context.Grades
                .Select(g => new SelectListItem { Value = g.ID.ToString(), Text = g.Grade.ToString() }).ToList();

            model.Classes = _context.Class
                .Where(c => c.GradeId == model.GradeId)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Class }).ToList();

            return View(model);
        }

		[HttpGet]
		public IActionResult StudentDelete(string id)
		{
			var RegistrationViewModel = (from u in _context.Users
										 where u.Id == id
										 join ua in _context.UserAcadamic on u.Id equals ua.UserID into academicData
										 from ua in academicData.DefaultIfEmpty()
										 select new RegistrationViewModel
										 {
											 ID = Guid.Parse(u.Id),
											 FullName = u.FullName,
											 Email = u.Email,
											 Password = u.PasswordHash,
											 gender = u.gender,
											 PhoneNumber = u.PhoneNumber,
											 Address = u.Address,
											 DateOfBirth = u.DateOfBirth,
											 GardianName = u.GardianName,
											 GradeId = ua.GradeId,
											 ClassId = ua.ClassId
										 }).FirstOrDefault();

			if (RegistrationViewModel == null)
			{
				TempData["ErrorMessage"] = "Student not found.";

				return RedirectToAction("StudentView");
			}

			RegistrationViewModel.Grades = _context.Grades
				.Select(g => new SelectListItem { Value = g.ID.ToString(), Text = g.Grade.ToString() }).ToList();

			RegistrationViewModel.Classes = _context.Class
				.Where(c => c.GradeId == RegistrationViewModel.GradeId)
				.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Class }).ToList();

			return View(RegistrationViewModel);
		}

		[HttpPost]
		public IActionResult StudentDelete(RegistrationViewModel model)
		{
			if (ModelState.IsValid)
			{
				var existingUser = _context.Users.FirstOrDefault(u => u.Id == model.ID.ToString());
				if (existingUser == null)
				{
					TempData["ErrorMessage"] = "Student not found.";

					return RedirectToAction("StudentView");
				}

			


				// Update or create UserAcademic entry
				var existingAcademicRecord = _context.UserAcadamic.FirstOrDefault(ua => ua.UserID == existingUser.Id);
				if (existingAcademicRecord == null)
				{
					_context.UserAcadamic.Add(new ClassRegistrationModel
					{
						UserID = existingUser.Id, // No need for ToString() as it's already a string
						GradeId = model.GradeId,
						ClassId = model.ClassId,

					});
				}
				else
				{
					existingAcademicRecord.GradeId = model.GradeId;
					existingAcademicRecord.ClassId = model.ClassId;
					_context.UserAcadamic.Remove(existingAcademicRecord);
				}

                _context.Users.Remove(existingUser);
                _context.SaveChanges();

				return RedirectToAction("StudentView");
			}

			// Reload grade and class dropdowns if the model state is invalid
			model.Grades = _context.Grades
				.Select(g => new SelectListItem { Value = g.ID.ToString(), Text = g.Grade.ToString() }).ToList();

			model.Classes = _context.Class
				.Where(c => c.GradeId == model.GradeId)
				.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Class }).ToList();

			return View(model);
		}

        public IActionResult studentback()
        {
            return RedirectToAction("studentview");
        }

        public IActionResult teacherback()
        {
            return RedirectToAction("studentview");
        }
    }

}