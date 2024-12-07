using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NReco.PdfGenerator;
using StudentManagement.Models;
using StudentManagement.ViewModel;
using System.Text;
using DinkToPdf;
using DinkToPdf.Contracts;
using System;


namespace StudentManagement.Controllers
{
    public class MarksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MarksController(ApplicationDbContext context)
        {
            _context = context;
        }
        private async Task PopulateDropdowns(MarksViewModel model, Guid? selectedGradeId = null)
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
        public async Task<IActionResult> Marksform()
        {
            var viewModel = new MarksViewModel();
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateClasses(Guid gradeId)
        {
            if (gradeId == Guid.Empty)
            {
                return Json(new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- No Classes Available --" }
        });
            }

            var classes = await _context.Class
                .Where(c => c.GradeId == gradeId)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"Class {c.Class}" // Ensure this matches the class property name
                })
                .ToListAsync();

            if (!classes.Any())
            {
                classes.Add(new SelectListItem { Value = "", Text = "-- No Classes Available --" });
            }

            return Json(classes);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersByGradeAndClass(Guid gradeId, Guid classId)
        {
            if (gradeId == Guid.Empty || classId == Guid.Empty)
            {
                return Json(new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- No Users Available --" }
        });
            }

            // Join AcademicDetails and Users to filter by grade and class
            var users = await _context.UserAcadamic
                .Where(ad => ad.GradeId == gradeId && ad.ClassId == classId) // Filter by Grade and Class
                .Join(_context.Users,
                    ad => ad.UserID,
                    u => u.Id,
                    (ad, u) => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.FullName // Assuming UserName is the property you want to display
                    })
                .ToListAsync();

            if (!users.Any())
            {
                users.Add(new SelectListItem { Value = "", Text = "-- No Users Available --" });
            }

            return Json(users);
        }
        public IActionResult SubjectGenerator(Guid gradeId)
        {
            if (gradeId == Guid.Empty)
            {
                return RedirectToAction("MarksForm");
              
            }

            var subjects = _context.Subject
                .Where(s => s.GradeId == gradeId)
                .Select(s => new { SubjectId = s.ID, SubjectName = s.Subject })
                .ToList();

            var htmlBuilder = new StringBuilder();

            foreach (var subject in subjects)
            {
                htmlBuilder.AppendLine("<div class='row mb-3'>");
                htmlBuilder.AppendLine($"  <label class='col-md-4 col-form-label' for='subject_{subject.SubjectId}'>{subject.SubjectName}</label>");
                htmlBuilder.AppendLine($"  <div class='col-md-4'>");
                htmlBuilder.AppendLine($"    <input type='text' class='form-control' name='SubjectMarks[{subject.SubjectId}]' id='subject_{subject.SubjectId}' placeholder='Enter marks' />");
                htmlBuilder.AppendLine($"  </div>");
                htmlBuilder.AppendLine("</div>");
            }

            return Content(htmlBuilder.ToString());
        }

        public IActionResult MarksView(MarksViewModel model)
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MarksView()
        {
            var viewModel = new MarksViewModel();
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult MarksFormSubmit(MarksViewModel model)
        {
            if (model == null || model.SubjectMarks == null || !model.SubjectMarks.Any())
            {
                TempData["ErrorMessage"] = "No marks submitted.";
                return RedirectToAction("MarksForm");
            }

            var existingUser = _context.MarksDetail
                     .FirstOrDefault(s => s.UserID == model.UserID && s.Term == model.Term);

            if(existingUser != null)
            {
                TempData["ErrorMessage"] = "In this Term User already been entered.";

                return RedirectToAction("MarksForm");
            }


            foreach (var entry in model.SubjectMarks)
            {
                var marksDetails = new MarksModel
                {
                    Id = Guid.NewGuid(),
                    UserID = model.UserID,
                    Term = model.Term,
                    SubjectID = entry.Key,
                    Marks = entry.Value,
                    ClassId = model.ClassId,
                       GradeId = model.GradeId
                };

                _context.MarksDetail.Add(marksDetails);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Marks submitted successfully!";
            return RedirectToAction("MarksForm");
        }

        [HttpGet]
        public async Task<IActionResult> GetTermsByGrade(Guid gradeId)
        {
            if (gradeId == Guid.Empty)
            {
                return Json(new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "-- No Terms Available --" }
                });
            }

            var terms = await _context.MarksDetail
                .Where(m => m.GradeId == gradeId)
                .Select(m => m.Term)
                .Distinct()
                .OrderBy(term => term)
                .Select(term => new SelectListItem
                {
                    Value = term.ToString(),
                    Text = term.ToString()
                })
                .ToListAsync();

            if (!terms.Any())
            {
                terms.Add(new SelectListItem { Value = "", Text = "-- No Terms Available --" });
            }

            return Json(terms);
        }


        //public IActionResult MarksSheetGenerator(Guid gradeId, Guid classId, string term)
        //{
        //    if (gradeId == Guid.Empty || classId == Guid.Empty || string.IsNullOrEmpty(term))
        //    {
        //        return RedirectToAction("MarksForm");
        //    }

        //    var subjects = _context.Subject
        //        .Where(s => s.GradeId == gradeId)
        //        .Select(s => new { s.ID, s.Subject })
        //        .ToList();

        //    var gradeName = _context.Grades
        //        .Where(g => g.ID == gradeId)
        //        .Select(g => g.Grade.ToString())
        //        .FirstOrDefault() ?? "Unknown Grade";

        //    var marks = _context.MarksDetail
        //        .Where(m => m.GradeId == gradeId && m.ClassId == classId && m.Term == term)
        //        .ToList();

        //    var htmlBuilder = new StringBuilder();

        //    htmlBuilder.AppendLine("<div class='container mt-4'>");
        //    htmlBuilder.AppendLine("  <div class='card shadow-sm'>");
        //    htmlBuilder.AppendLine("    <div class='card-header text-white bg-secondary'>");
        //    htmlBuilder.AppendLine($"      <h4 class='mb-0 text-center'>Marksheet for Grade {gradeName} - {term}</h4>");
        //    htmlBuilder.AppendLine("    </div>");
        //    htmlBuilder.AppendLine("    <div class='card-body'>");

        //    htmlBuilder.AppendLine("      <div class='table-responsive'>");
        //    htmlBuilder.AppendLine("        <table class='table table-striped table-hover text-center table-bordered'>");
        //    htmlBuilder.AppendLine("          <thead>");
        //    htmlBuilder.AppendLine("            <tr>");
        //    htmlBuilder.AppendLine("              <th>Student</th>");

        //    foreach (var subject in subjects)
        //    {
        //        htmlBuilder.AppendLine($"              <th>{subject.Subject}</th>");
        //    }


        //    htmlBuilder.AppendLine($"              <th>Total</th>");
        //    htmlBuilder.AppendLine($"              <th>Avarage</th>");
        //    htmlBuilder.AppendLine("              <th>Rank</th>");

        //    htmlBuilder.AppendLine("            </tr>");
        //    htmlBuilder.AppendLine("          </thead>");
        //    htmlBuilder.AppendLine("          <tbody>");

        //    var students = marks.GroupBy(m => m.UserID);

        //    foreach (var studentGroup in students)
        //    {
        //        var studentId = studentGroup.Key;
        //        var studentName = _context.Users
        //            .Where(r => r.Id == studentId)
        //            .Select(r => r.FullName)
        //            .FirstOrDefault() ?? "Unknown Student";

        //        htmlBuilder.AppendLine("            <tr>");
        //        htmlBuilder.AppendLine($"              <td>{studentName}</td>");

        //        int count = 0;
        //        int totalMarks = 0; 
        //        foreach (var subject in subjects)
        //        {
        //            var studentMarks = studentGroup
        //                .FirstOrDefault(m => m.SubjectID == subject.ID)?.Marks ?? "N/A";

        //            htmlBuilder.AppendLine($"              <td>{studentMarks}</td>");


        //            count++;
        //            if (int.TryParse(studentMarks, out int numericMarks))
        //            {

        //                totalMarks += numericMarks; 
        //            }

        //        }

        //        int average = totalMarks / count;


        //        htmlBuilder.AppendLine($"<td>{totalMarks}</td>");
        //        htmlBuilder.AppendLine($"<td>{average}</td>");




        //        htmlBuilder.AppendLine("            </tr>");
        //    }

        //    htmlBuilder.AppendLine("          </tbody>");
        //    htmlBuilder.AppendLine("        </table>");
        //    htmlBuilder.AppendLine("      </div>");
        //    htmlBuilder.AppendLine("    </div>");
        //    htmlBuilder.AppendLine("  </div>");
        //    htmlBuilder.AppendLine("</div>");

        //    return Content(htmlBuilder.ToString(), "text/html");
        //}

        public IActionResult MarksSheetGenerator(Guid gradeId, Guid classId, string term)
        {
            if (gradeId == Guid.Empty || classId == Guid.Empty || string.IsNullOrEmpty(term))
            {
                return RedirectToAction("MarksForm");
            }

            var subjects = _context.Subject
                .Where(s => s.GradeId == gradeId)
                .Select(s => new { s.ID, s.Subject })
                .ToList();

            var gradeName = _context.Grades
                .Where(g => g.ID == gradeId)
                .Select(g => g.Grade.ToString())
                .FirstOrDefault() ?? "Unknown Grade";

            var marks = _context.MarksDetail
                .Where(m => m.GradeId == gradeId && m.ClassId == classId && m.Term == term)
                .ToList();

            var htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<div class='container mt-4'>");
            htmlBuilder.AppendLine("  <div class='card shadow-sm'>");
            htmlBuilder.AppendLine("    <div class='card-header text-white bg-secondary'>");
            htmlBuilder.AppendLine($"      <h4 class='mb-0 text-center'>Marksheet for Grade {gradeName} - {term}</h4>");
            htmlBuilder.AppendLine("    </div>");
            htmlBuilder.AppendLine("    <div class='card-body'>");

            htmlBuilder.AppendLine("      <div class='table-responsive'>");
            htmlBuilder.AppendLine("        <table class='table table-hover text-center table-bordered'>");
            htmlBuilder.AppendLine("          <thead class= 'table-secondary'>");
            htmlBuilder.AppendLine("            <tr>");
            htmlBuilder.AppendLine("              <th>Rank</th>");
            htmlBuilder.AppendLine("              <th>Student</th>");

            foreach (var subject in subjects)
            {
                htmlBuilder.AppendLine($"              <th>{subject.Subject}</th>");
            }

            htmlBuilder.AppendLine("              <th>Total</th>");
            htmlBuilder.AppendLine("              <th>Average</th>");
            htmlBuilder.AppendLine("            </tr>");
            htmlBuilder.AppendLine("          </thead>");
            htmlBuilder.AppendLine("          <tbody>");

            // Calculate total marks for each student
            var studentTotals = marks
                .GroupBy(m => m.UserID)
                .Select(g => new
                {
                    UserID = g.Key,
                    TotalMarks = g.Sum(m => int.TryParse(m.Marks, out int marks) ? marks : 0),
                    Marks = g.ToList()
                })
                .OrderByDescending(x => x.TotalMarks) // Sort by total marks in descending order
                .ToList();

            int rank = 1;
            foreach (var student in studentTotals)
            {
                var studentName = _context.Users
                    .Where(r => r.Id == student.UserID)
                    .Select(r => r.FullName)
                    .FirstOrDefault() ?? "Unknown Student";

                htmlBuilder.AppendLine("            <tr>");
                htmlBuilder.AppendLine($"              <td>{rank}</td>");
                htmlBuilder.AppendLine($"              <td>{studentName}</td>");

                int totalMarks = student.TotalMarks;
                int count = 0;

                foreach (var subject in subjects)
                {
                    var studentMarks = student.Marks
                        .FirstOrDefault(m => m.SubjectID == subject.ID)?.Marks ?? "N/A";

                    htmlBuilder.AppendLine($"              <td>{studentMarks}</td>");
                    count++;
                }

                int average = count > 0 ? totalMarks / count : 0;
                htmlBuilder.AppendLine($"              <td>{totalMarks}</td>");
                htmlBuilder.AppendLine($"              <td>{average}</td>");
                htmlBuilder.AppendLine("            </tr>");

                rank++;
            }

            htmlBuilder.AppendLine("          </tbody>");
            htmlBuilder.AppendLine("        </table>");
            htmlBuilder.AppendLine("<div class='text-end mt-3 mb-3'>"); // Align content to the right
            htmlBuilder.AppendLine($"  <a href=\"/Marks/DownloadMarksheet?gradeId={gradeId}&classId={classId}&term={term}\" class=\"btn btn-primary btn-sm  justify-content-center\">");
            htmlBuilder.AppendLine("    <i class=\"bi bi-download me-2\"></i>"); // Add a Bootstrap icon for download
            htmlBuilder.AppendLine("    Download");
            htmlBuilder.AppendLine("  </a>");
            htmlBuilder.AppendLine("</div>");

            htmlBuilder.AppendLine("      </div>");
          
            htmlBuilder.AppendLine("    </div>");
            htmlBuilder.AppendLine("  </div>");
            htmlBuilder.AppendLine("</div>");

            return Content(htmlBuilder.ToString(), "text/html");
        }



        public IActionResult MarksAllView(string searchQuery)
        {
            var marksDetailsQuery = _context.MarksDetail
                .Include(m => m.Registration)
                .Include(m => m.Subject)
                .Include(m => m.Grade)
                .Include(m => m.Class)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();

                marksDetailsQuery = marksDetailsQuery.Where(m =>
                    (!string.IsNullOrEmpty(m.Registration.FullName) &&
                        m.Registration.FullName.ToLower().Contains(searchQuery)) ||
                    (!string.IsNullOrEmpty(m.Grade.Grade.ToString()) &&
                        m.Grade.Grade.ToString().ToLower().Contains(searchQuery)) ||
                    (!string.IsNullOrEmpty(m.Class.Class) &&
                        m.Class.Class.ToLower().Contains(searchQuery)) ||
                    (!string.IsNullOrEmpty(m.Subject.Subject) &&
                        m.Subject.Subject.ToLower().Contains(searchQuery)) ||
                    (!string.IsNullOrEmpty(m.Term) &&
                        m.Term.ToLower().Contains(searchQuery))
                );
            }

            var marksDetails = marksDetailsQuery
                .Select(m => new MarksViewModel
                {
                    Id = m.Id,
                    UserID = m.UserID,
                    Registration = m.Registration,
                    SubjectID = m.SubjectID,
                    Subject = m.Subject,
                    GradeId = m.GradeId,
                    ClassId = m.ClassId,
                    Term = m.Term,
                    Marks = m.Marks.ToString(),
                    Grade = m.Grade,
                    Class = m.Class
                })
                .ToList();

            return View(marksDetails);
        }






        [HttpGet]
        public IActionResult MarksEdit(Guid id)
        {
            // Fetch the mark details based on the provided ID
            var marks = _context.MarksDetail
                .Where(m => m.Id == id)
                .Select(m => new MarksViewModel
                {
                    Id = m.Id,
                    Marks = m.Marks,
                    Term = m.Term,
                    GradeId = m.GradeId,
                    Grades = _context.Grades.Select(g => new SelectListItem
                    {
                        Value = g.ID.ToString(),
                        Text = $"Grade {g.Grade}"
                    }).ToList(), // Corrected closing parenthesis and added ToList()

                    ClassId = m.ClassId,
                    Classes = _context.Class.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"Class {c.Class}"
                    }).ToList(), // Corrected closing parenthesis and added ToList()


                    SubjectID = m.SubjectID, // Ensure Subject is fetched
                    Subjects = _context.Subject.Select(s => new SelectListItem
                    {
                        Value = s.ID.ToString(),
                        Text = $"{s.Subject}"
                    }).ToList(),

                    UserID = m.UserID,
                    Users = _context.Users.Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = $"{u.FullName}"
                    }).ToList() // Fetch all users
                }).FirstOrDefault(); // Ensures you fetch a single record or null if not found

            if (marks == null) // Fixed variable name from "subject" to "marks"
            {
                TempData["ErrorMessage"] = "Marks not found.";
                return RedirectToAction("MarksList"); // Redirect to a relevant action (e.g., "MarksList")
            }

            return View(marks);
        }

        [HttpPost]
        public IActionResult MarksEdit(MarksViewModel model)
        {

            foreach (var entry in model.Marks)
            {
                var existingMarks = _context.MarksDetail.SingleOrDefault(m => m.Id == model.Id);
                if (existingMarks == null)
                {
                    TempData["ErrorMessage"] = "Record not found.";
                    return RedirectToAction("MarksAllView");
                }

               
                existingMarks.Marks = model.Marks;
              

                _context.MarksDetail.Update(existingMarks);
                _context.SaveChanges();
            }

                return RedirectToAction("MarksAllView");
        }



        public async Task<IActionResult> DownloadMarksheetAsync(Guid gradeId, Guid classId, string term)
        {
            if (gradeId == Guid.Empty || classId == Guid.Empty || string.IsNullOrEmpty(term))
            {
                return RedirectToAction("MarksForm");
            }

            var subjects = _context.Subject
                .Where(s => s.GradeId == gradeId)
                .Select(s => new { s.ID, s.Subject })
                .ToList();

            var gradeName = _context.Grades
                .Where(g => g.ID == gradeId)
                .Select(g => g.Grade.ToString())
                .FirstOrDefault() ?? "Unknown Grade";

            var marks = _context.MarksDetail
                .Where(m => m.GradeId == gradeId && m.ClassId == classId && m.Term == term)
                .ToList();

            var htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css'>");
            htmlBuilder.AppendLine("<div class='container mt-4'>");
            htmlBuilder.AppendLine("  <div class='card shadow-sm'>");
            htmlBuilder.AppendLine("    <div class='card-header text-white bg-secondary'>");
            htmlBuilder.AppendLine($"      <h2 class='mb-3 text-center'>Marksheet for Grade {gradeName} - {term}</h2>");
            htmlBuilder.AppendLine("    </div>");
            htmlBuilder.AppendLine("    <div class='card-body'>");
            htmlBuilder.AppendLine("      <div class='table-responsive'>");
            htmlBuilder.AppendLine("        <table class='table table-hover text-center table-bordered'>");
            htmlBuilder.AppendLine("          <thead class= 'table-secondary'>");
            htmlBuilder.AppendLine("            <tr>");
            htmlBuilder.AppendLine("              <th>Rank</th>");
            htmlBuilder.AppendLine("              <th>Student</th>");

            foreach (var subject in subjects)
            {
                htmlBuilder.AppendLine($"              <th>{subject.Subject}</th>");
            }

            htmlBuilder.AppendLine("              <th>Total</th>");
            htmlBuilder.AppendLine("              <th>Average</th>");
            htmlBuilder.AppendLine("            </tr>");
            htmlBuilder.AppendLine("          </thead>");
            htmlBuilder.AppendLine("          <tbody>");

            var studentTotals = marks
                .GroupBy(m => m.UserID)
                .Select(g => new
                {
                    UserID = g.Key,
                    TotalMarks = g.Sum(m => int.TryParse(m.Marks, out int marks) ? marks : 0),
                    Marks = g.ToList()
                })
                .OrderByDescending(x => x.TotalMarks)
                .ToList();

            int rank = 1;
            foreach (var student in studentTotals)
            {
                var studentName = _context.Users
                    .Where(r => r.Id == student.UserID)
                    .Select(r => r.FullName)
                    .FirstOrDefault() ?? "Unknown Student";

                htmlBuilder.AppendLine("            <tr>");
                htmlBuilder.AppendLine($"              <td>{rank}</td>");
                htmlBuilder.AppendLine($"              <td>{studentName}</td>");

                int totalMarks = student.TotalMarks;
                int count = 0;

                foreach (var subject in subjects)
                {
                    var studentMarks = student.Marks
                        .FirstOrDefault(m => m.SubjectID == subject.ID)?.Marks ?? "N/A";

                    htmlBuilder.AppendLine($"              <td>{studentMarks}</td>");
                    count++;
                }

                int average = count > 0 ? totalMarks / count : 0;
                htmlBuilder.AppendLine($"              <td>{totalMarks}</td>");
                htmlBuilder.AppendLine($"              <td>{average}</td>");
                htmlBuilder.AppendLine("            </tr>");

                rank++;
            }

            htmlBuilder.AppendLine("          </tbody>");
            htmlBuilder.AppendLine("        </table>");
            htmlBuilder.AppendLine("      </div>");
            htmlBuilder.AppendLine("    </div>");
            htmlBuilder.AppendLine("  </div>");
            htmlBuilder.AppendLine("</div>");

            // Convert the HTML to PDF
            var pdf = await ConvertHtmlToPdfAsync(htmlBuilder.ToString());
            return File(pdf, "application/pdf", "Marksheet.pdf");

        }
        private static readonly SynchronizedConverter _converter = new SynchronizedConverter(new PdfTools());
        private async Task<byte[]> ConvertHtmlToPdfAsync(string html)
        {
            return await Task.Run(() =>
            {
                var doc = new HtmlToPdfDocument
                {
                    GlobalSettings = new GlobalSettings
                    {
                        PaperSize = PaperKind.A4,
                        Orientation = Orientation.Landscape,
                    },
                    Objects = {
                new ObjectSettings
                {
                    HtmlContent = html,
                    WebSettings = {
                        DefaultEncoding = "utf-8",
                        UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/css/bootstrap.min.css")
                    }
                }
            }
                };

                return _converter.Convert(doc);
            });
        }

    }



}
