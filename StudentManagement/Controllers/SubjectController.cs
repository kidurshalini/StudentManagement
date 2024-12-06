using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.ViewModel;

namespace StudentManagement.Controllers
{
    public class SubjectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Subject()
        {
            var gradeItems = _context.Grades
        .Select(g => new SelectListItem
        {
            Value = g.ID.ToString(),
            Text = $"Grade {g.Grade}"
        })
        .ToList();

            var viewModel = new SubjectViewModel
            {
                Grades = gradeItems
            };

            return View(viewModel);


        }
        //[HttpPost]
        //public IActionResult Subject(SubjectViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var Subject = new SubjectModel
        //            {
        //                ID = Guid.NewGuid(),
        //                Subject = model.Subject,
        //                GradeId = model.GradeId // Assign the selected grade
        //            };

        //            _context.Subject.Add(Subject);
        //            _context.SaveChanges();

        //            TempData["SuccessMessage"] = "Subject added successfully!";
        //            return RedirectToAction("Subject");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error: {ex.Message}");
        //            TempData["ErrorMessage"] = "An error occurred while saving the subject.";
        //        }
        //    }
        //    else
        //    {
        //        TempData["ErrorMessage"] = "Please fill all required fields correctly.";
        //    }

        //    // Repopulate the dropdown in case of validation errors
        //    model.Grades = _context.Grades
        //        .Select(g => new SelectListItem
        //        {
        //            Value = g.ID.ToString(),
        //            Text = $"Grade {g.Grade}"
        //        })
        //        .ToList();

        //    return View(model);
        //}

        [HttpPost]
        public IActionResult Subject(SubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if the combination of GradeId and Subject already exists
                    var existingSubject = _context.Subject
                        .FirstOrDefault(s => s.GradeId == model.GradeId && s.Subject == model.Subject);

                    if (existingSubject != null)
                    {
                        TempData["ErrorMessage"] = "This subject for the selected grade has already been entered.";
                        // Repopulate the dropdown list in case of error
                        model.Grades = _context.Grades
                            .Select(g => new SelectListItem
                            {
                                Value = g.ID.ToString(),
                                Text = $"Grade {g.Grade}"
                            })
                            .ToList();

                        return View(model); // Return to the same view with the error message
                    }

                    // Proceed with adding the new subject if no duplicates found
                    var newSubject = new SubjectModel
                    {
                        ID = Guid.NewGuid(),
                        Subject = model.Subject,
                        GradeId = model.GradeId // Assign the selected grade
                    };

                    _context.Subject.Add(newSubject);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Subject added successfully!";
                    return RedirectToAction("Subject");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    TempData["ErrorMessage"] = "An error occurred while saving the subject.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please fill all required fields correctly.";
            }

            // Repopulate the dropdown in case of validation errors
            model.Grades = _context.Grades
                .Select(g => new SelectListItem
                {
                    Value = g.ID.ToString(),
                    Text = $"Grade {g.Grade}"
                })
                .ToList();

            return View(model);
        }



        public IActionResult Class()
        {

            var gradeItems = _context.Grades
                .Select(g => new SelectListItem
                {
                    Value = g.ID.ToString(),
                    Text = $"Grade {g.Grade}"
                })
                .ToList();

            var viewModel = new ClassViewModel
            {
                Grades = gradeItems
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Class(ClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {


                    var existingClass = _context.Class
                     .FirstOrDefault(c => c.GradeId == model.GradeId && c.Class == model.Class);


                    if (existingClass != null)
                    {
                        TempData["ErrorMessage"] = "This subject for the selected grade has already been entered.";
                        // Repopulate the dropdown list in case of error
                        model.Grades = _context.Grades
                            .Select(g => new SelectListItem
                            {
                                Value = g.ID.ToString(),
                                Text = $"Grade {g.Grade}"
                            })
                            .ToList();

                        return View(model); // Return to the same view with the error message
                    }

                    var Class = new ClassModel
                    {
                        Id = Guid.NewGuid(),
                        Class = model.Class,
                        GradeId = model.GradeId // Assign the selected grade
                    };

                    _context.Class.Add(Class);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Class added successfully!";
                    return RedirectToAction("Class");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    TempData["ErrorMessage"] = "An error occurred while saving the subject.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please fill all required fields correctly.";
            }

            // Repopulate the dropdown in case of validation errors
            model.Grades = _context.Grades
                .Select(g => new SelectListItem
                {
                    Value = g.ID.ToString(),
                    Text = $"Grade {g.Grade}"
                })
                .ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult ViewSubject()
        {
            var model = new SubjectViewModel
            {
                // Populate the Grades dropdown list
                Grades = _context.Grades
                    .Select(g => new SelectListItem
                    {
                        Value = g.ID.ToString(),   // Grade ID as GUID (converted to string)
                        Text = $"Grade {g.Grade}"   // Display text, for example, "Grade 1"
                    })
                    .ToList(),

                // Populate the list of subjects (with related grade)
                Subjects = _context.Subject
                    .Include(s => s.Grades)  // Include Grade for each subject
                    .ToList()
            };

            return View(model);  // Pass the populated model to the view
        }

  [HttpGet]
public IActionResult ClassView(string searchQuery)
{
    var classDetails = _context.Class
        .Include(c => c.Grades) // Ensure Grades navigation property is loaded
        .Select(c => new ClassViewModel
        {
            Id = c.Id,
            GradeId = c.GradeId, // Use GradeId for reference
            Class = $"Class {c.Class}",
            Grades = new List<SelectListItem>
            {
                new SelectListItem
                {
                    // Directly accessing the Grade property from GradeModel
                    Text = c.Grades != null ? $"Grade {c.Grades.Grade}" : "No Grade", 
                    Value = c.GradeId.ToString() // GradeId as value
                }
            }
        })
        .ToList();

    // Apply search filter if search query is provided
    if (!string.IsNullOrEmpty(searchQuery))
    {
        classDetails = classDetails.Where(c =>
            c.Class.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
            c.Grades.Any(g => g.Text.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) // Searching in Grade text
        ).ToList(); // Ensure to call ToList() after filtering
    }

    return View(classDetails);
}




        [HttpGet]
        public IActionResult SubjectEdit(Guid id)
        {
            var subject = _context.Subject
                .Where(s => s.ID == id)
                .Select(s => new SubjectViewModel
                {
                    Id = s.ID,
                    Subject = s.Subject,
                    GradeId = s.GradeId,
                    Grades = _context.Grades.Select(g => new SelectListItem
                    {
                        Value = g.ID.ToString(),
                        Text = $"Grade {g.Grade}"
                    }).ToList()
                })
                .FirstOrDefault();

            if (subject == null)
            {
                TempData["ErrorMessage"] = "Subject not found.";
                return RedirectToAction("SubjectEdit");
            }

            return View(subject);
        }


        [HttpPost]
		public IActionResult SubjectEdit(SubjectViewModel model)
		{
			if (ModelState.IsValid)
			{
                // Check if the combination of GradeId and Subject already exists
                var existingSubjects = _context.Subject
                    .FirstOrDefault(s => s.GradeId == model.GradeId && s.Subject == model.Subject);

                if (existingSubjects != null)
                {
                    TempData["ErrorMessage"] = "This subject for the selected grade has already been entered.";
                    // Repopulate the dropdown list in case of error
                    model.Grades = _context.Grades
                        .Select(g => new SelectListItem
                        {
                            Value = g.ID.ToString(),
                            Text = $"Grade {g.Grade}"
                        })
                        .ToList();

                    return View(model); // Return to the same view with the error message
                }
                // Retrieve the existing subject from the database
                var existingSubject = _context.Subject.FirstOrDefault(s => s.ID == model.Id);

				if (existingSubject == null)
				{
					TempData["ErrorMessage"] = "Subject not found.";
					return RedirectToAction("SubjectEdit"); // Redirect to the list page if not found
				}

				// Update the subject's properties
				existingSubject.Subject = model.Subject;
				existingSubject.GradeId = model.GradeId;

				_context.Subject.Update(existingSubject);
				_context.SaveChanges();

			
				return RedirectToAction("ViewSubject"); // Redirect to the list page after successful update
			}

			// Reload the Grades list in case of validation errors
			model.Grades = _context.Grades.Select(g => new SelectListItem
			{
				Value = g.ID.ToString(),
				Text = $"Grade {g.Grade}"
			}).ToList();

			return View(model); // Return to the form with the validation errors
		}

        [HttpGet]
        public IActionResult SubjectDelete(Guid id)
        {
            var subject = _context.Subject
                .Where(s => s.ID == id)
                .Select(s => new SubjectViewModel
                {
                    Id = s.ID,
                    Subject = s.Subject,
                    GradeId = s.GradeId,
                    Grades = _context.Grades.Select(g => new SelectListItem
                    {
                        Value = g.ID.ToString(),
                        Text = $"Grade {g.Grade}"
                    }).ToList()
                })
                .FirstOrDefault();

            if (subject == null)
            {
                TempData["ErrorMessage"] = "Subject not found.";
                return RedirectToAction("SubjectDelete");
            }

            return View(subject);
        }

        [HttpPost]
        public IActionResult SubjectDelete(SubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
             
            
                // Retrieve the existing subject from the database
                var existingSubject = _context.Subject.FirstOrDefault(s => s.ID == model.Id);

          
                _context.Subject.Remove(existingSubject);
                _context.SaveChanges();

             
                return RedirectToAction("ViewSubject"); 
            }

            // Reload the Grades list in case of validation errors
            model.Grades = _context.Grades.Select(g => new SelectListItem
            {
                Value = g.ID.ToString(),
                Text = $"Grade {g.Grade}"
            }).ToList();

            return View(model); // Return to the form with the validation errors
        }


        // GET: ClassEdit
        [HttpGet]
        public IActionResult ClassEdit(Guid id)
        {
            // Retrieve the class with the given Id
            var classViewModel = _context.Class
                .Where(c => c.Id == id)
                .Select(c => new ClassViewModel
                {
                    Id = c.Id,
                    Class = c.Class,
                    GradeId = c.GradeId,
                    Grades = _context.Grades.Select(g => new SelectListItem
                    {
                        Value = g.ID.ToString(),
                        Text = $"Grade {g.Grade}"
                    }).ToList()
                })
                .FirstOrDefault();

            // Check if class is not found
            if (classViewModel == null)
            {
                TempData["ErrorMessage"] = "Class not found.";
                return RedirectToAction("ClassView"); // Redirect to a list view or appropriate action
            }

            return View(classViewModel); // Pass the model to the view
        }


        // POST: ClassEdit
        [HttpPost]
        public IActionResult ClassEdit(ClassViewModel model)
        {
            if (model.Id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid Class ID.";
                model.Grades = _context.Grades
                      .Select(g => new SelectListItem
                      {
                          Value = g.ID.ToString(),
                          Text = $"Grade {g.Grade}"
                      })
                      .ToList();

                return View(model);
            }

            if (ModelState.IsValid)
            {
                // Check for duplicate class with the same GradeId and Class name
                var existingClass = _context.Class
                    .FirstOrDefault(c => c.GradeId == model.GradeId && c.Class == model.Class);

                if (existingClass != null)
                {
                    TempData["ErrorMessage"] = "This class for the selected grade already exists.";
                    // Repopulate the dropdown list in case of error
                    model.Grades = _context.Grades
                        .Select(g => new SelectListItem
                        {
                            Value = g.ID.ToString(),
                            Text = $"Grade {g.Grade}"
                        })
                        .ToList();

                    return View(model); // Return to the same view with the error message
                }

                // Retrieve the existing class record from the database
                var classToUpdate = _context.Class.FirstOrDefault(c => c.Id == model.Id);

                if (classToUpdate == null)
                {
                    TempData["ErrorMessage"] = "Class not found.";
                    return RedirectToAction("ClassView"); // Redirect to the list page if class not found
                }

                // Update the class properties
                classToUpdate.Class = model.Class;
                classToUpdate.GradeId = model.GradeId;

                try
                {
                    _context.Class.Update(classToUpdate);
                    _context.SaveChanges();
                    
                    return RedirectToAction("ClassView"); // Redirect to the list page after successful update
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating class: {ex.Message}";
                    // Log the exception (optional)
                    Console.WriteLine(ex.Message);
                }
            }

            // Reload the Grades list in case of validation errors
            model.Grades = _context.Grades
                .Select(g => new SelectListItem
                {
                    Value = g.ID.ToString(),
                    Text = $"Grade {g.Grade}"
                })
                .ToList();

            return View(model); // Return to the form with validation errors
        }


        [HttpGet]
        public IActionResult ClassDelete(Guid id)
        {
            var classViewModel = _context.Class
            .Where(c => c.Id == id)
            .Select(c => new ClassViewModel
            {
                Id = c.Id,
                Class = c.Class,
                GradeId = c.GradeId,
                Grades = _context.Grades.Select(g => new SelectListItem
                {
                    Value = g.ID.ToString(),
                    Text = $"Grade {g.Grade}"
                }).ToList()
            })
            .FirstOrDefault();

            // Check if class is not found
            if (classViewModel == null)
            {
                TempData["ErrorMessage"] = "Class not found.";
                return RedirectToAction("ClassView"); // Redirect to a list view or appropriate action
            }

            return View(classViewModel); 
        }
        [HttpPost]
        public IActionResult ClassDelete(ClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == Guid.Empty)
                {
                    TempData["ErrorMessage"] = "Invalid Class ID.";
                    model.Grades = _context.Grades
                        .Select(g => new SelectListItem
                        {
                            Value = g.ID.ToString(),
                            Text = $"Grade {g.Grade}"
                        })
                        .ToList();

                    return View(model);
                }

                // Check for existing class dependencies
                var hasDependencies = _context.UserAcadamic.Any(ua => ua.ClassId == model.Id);
                if (hasDependencies)
                {
                    TempData["ErrorMessage"] = "This class is currently in use and cannot be deleted.";
                    model.Grades = _context.Grades
                        .Select(g => new SelectListItem
                        {
                            Value = g.ID.ToString(),
                            Text = $"Grade {g.Grade}"
                        })
                        .ToList();

                    return View(model);
                }

                try
                {
                    // Retrieve the class to delete
                    var classToRemove = _context.Class.FirstOrDefault(c => c.Id == model.Id);

                    if (classToRemove == null)
                    {
                        TempData["ErrorMessage"] = "Class not found.";
                        return RedirectToAction("ClassView");
                    }

                    // Delete the class
                    _context.Class.Remove(classToRemove);
                    _context.SaveChanges();

              
                    return RedirectToAction("ClassView");
                }
                catch (DbUpdateException ex)
                {
                    // Log the exception for debugging
                    Console.WriteLine(ex);

                    // Show a user-friendly message
                    TempData["ErrorMessage"] = "Unable to delete this class as it is being referenced by other records.";
                }
            }

            // Reload the Grades list in case of validation errors
            model.Grades = _context.Grades
                .Select(g => new SelectListItem
                {
                    Value = g.ID.ToString(),
                    Text = $"Grade {g.Grade}"
                })
                .ToList();

            return View(model);
        }

        public IActionResult classback()
        {
            return RedirectToAction("ClassView");
        }

        public IActionResult subjectback()
        {
            return RedirectToAction("ViewSubject");
        }
    }
}