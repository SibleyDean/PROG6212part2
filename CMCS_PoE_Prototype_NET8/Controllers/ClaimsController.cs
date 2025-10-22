using CMCS.Mvc.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace CMCS.Mvc.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly InMemoryStore _store;
        private readonly IWebHostEnvironment _env;

        public ClaimsController(InMemoryStore store, IWebHostEnvironment env)
        {
            _store = store ?? throw new System.ArgumentNullException(nameof(store));
            _env = env ?? throw new System.ArgumentNullException(nameof(env));
        }

        // GET: /Claims
        public IActionResult Index()
        {
            var claims = _store.GetAllClaims();
            return View(claims);
        }

        // GET:/Claims/Details/5
        public IActionResult Details(int id)
        {
            var claim = _store.GetClaim(id);
            if (claim == null) return NotFound();
            return View(claim);
        }

        // GET:/Claims/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST:/Claims/Create - With file validation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string lecturerName, string title, string description, int hours = 0, decimal rate = 0, IFormFile file = null)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(lecturerName))
                {
                    ModelState.AddModelError("LecturerName", "Lecturer Name is required.");
                    return View();
                }

                if (string.IsNullOrWhiteSpace(title))
                {
                    ModelState.AddModelError("Title", "Title is required.");
                    return View();
                }

                // Validate file if provided
                string filePath = null;
                if (file != null && file.Length > 0)
                {
                    // File size limit: 10MB
                    if (file.Length > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError("file", "File size must be less than 10MB.");
                        return View();
                    }

                    // Allowed file types
                    var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".doc", ".xls" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("file", "Only PDF, Word (.docx, .doc), and Excel (.xlsx, .xls) files are allowed.");
                        return View();
                    }

                    // Secure file storage
                    var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                    Directory.CreateDirectory(uploadsRoot);

                    // Generate secure filename
                    var safeFileName = Path.GetFileName(file.FileName);
                    var uniqueFileName = $"{System.Guid.NewGuid()}_{safeFileName}";
                    var relativePath = Path.Combine("uploads", uniqueFileName);
                    var absolutePath = Path.Combine(_env.WebRootPath ?? "wwwroot", relativePath);

                    using (var stream = new FileStream(absolutePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    filePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
                }

                // Create claim
                var claim = new Claim
                {
                    LecturerName = lecturerName.Trim(),
                    Title = title.Trim(),
                    Description = string.IsNullOrWhiteSpace(description) ? "No description provided" : description.Trim(),
                    Hours = hours,
                    Rate = rate,
                    FilePath = filePath,
                    Status = "Pending"
                };

                var addedClaim = _store.AddClaim(claim);
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "Error submitting claim. Please try again.");
                return View();
            }
        }

        // GET: /Claims/Edit/5
        public IActionResult Edit(int id)
        {
            var claim = _store.GetClaim(id);
            if (claim == null) return NotFound();
            return View(claim);
        }

        // POST:/Claims/Edit - With file validation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, string lecturerName, string title, string description, int hours = 0, decimal rate = 0, IFormFile file = null)
        {
            var existingClaim = _store.GetClaim(id);
            if (existingClaim == null) return NotFound();

            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(lecturerName))
                {
                    ModelState.AddModelError("LecturerName", "Lecturer Name is required.");
                    return View(existingClaim);
                }

                if (string.IsNullOrWhiteSpace(title))
                {
                    ModelState.AddModelError("Title", "Title is required.");
                    return View(existingClaim);
                }

                // Validate file if provided
                string filePath = existingClaim.FilePath;
                if (file != null && file.Length > 0)
                {
                    // File size limit: 10MB
                    if (file.Length > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError("file", "File size must be less than 10MB.");
                        return View(existingClaim);
                    }

                    // Allowed file types
                    var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".doc", ".xls" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("file", "Only PDF, Word (.docx, .doc), and Excel (.xlsx, .xls) files are allowed.");
                        return View(existingClaim);
                    }

                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(existingClaim.FilePath))
                    {
                        try
                        {
                            var oldFilePath = Path.Combine(_env.WebRootPath ?? "wwwroot",
                                existingClaim.FilePath.Replace('/', Path.DirectorySeparatorChar));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                        catch
                        {
                            // Ignore deletion errors
                        }
                    }

                    // Secure file storage
                    var uploadsRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                    Directory.CreateDirectory(uploadsRoot);

                    // Generate secure filename
                    var safeFileName = Path.GetFileName(file.FileName);
                    var uniqueFileName = $"{System.Guid.NewGuid()}_{safeFileName}";
                    var relativePath = Path.Combine("uploads", uniqueFileName);
                    var absolutePath = Path.Combine(_env.WebRootPath ?? "wwwroot", relativePath);

                    using (var stream = new FileStream(absolutePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    filePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
                }

                // Update claim
                existingClaim.LecturerName = lecturerName.Trim();
                existingClaim.Title = title.Trim();
                existingClaim.Description = string.IsNullOrWhiteSpace(description) ? "No description provided" : description.Trim();
                existingClaim.Hours = hours;
                existingClaim.Rate = rate;
                existingClaim.FilePath = filePath;

                var ok = _store.UpdateClaim(existingClaim);
                if (!ok) return NotFound();

                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "Error updating claim. Please try again.");
                return View(existingClaim);
            }
        }

        // [Keep the rest of your existing methods - Delete, Download, etc.]
        // GET: /Claims/Delete/5
        public IActionResult Delete(int id)
        {
            var claim = _store.GetClaim(id);
            if (claim == null) return NotFound();
            return View(claim);
        }

        // POST: /Claims/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var claim = _store.GetClaim(id);
            if (claim != null && !string.IsNullOrEmpty(claim.FilePath))
            {
                try
                {
                    var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot",
                        claim.FilePath.Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch
                {
                    // Ignore file system failures
                }
            }

            var removed = _store.DeleteClaim(id);
            if (!removed) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Claims/Download/5
        public IActionResult Download(int id)
        {
            var claim = _store.GetClaim(id);
            if (claim == null || string.IsNullOrEmpty(claim.FilePath)) return NotFound();

            var absolutePath = Path.Combine(_env.WebRootPath ?? "wwwroot",
                claim.FilePath.Replace('/', Path.DirectorySeparatorChar));

            if (!System.IO.File.Exists(absolutePath)) return NotFound();

            var fileName = Path.GetFileName(absolutePath);
            return PhysicalFile(absolutePath, "application/octet-stream", fileName);
        }
    }
}