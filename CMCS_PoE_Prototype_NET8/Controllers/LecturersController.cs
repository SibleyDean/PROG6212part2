using CMCS.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace CMCS.Mvc.Controllers
{
    public class LecturersController : Controller
    {
        private readonly InMemoryStore _store;

        public LecturersController(InMemoryStore store)
        {
            _store = store;
        }

        // GET: /Lecturers
        public IActionResult Index()
        {
            var lecturers = _store.GetAllLecturers();
            return View(lecturers);
        }

        // GET: /Lecturers/Details/5
        public IActionResult Details(int id)
        {
            var lecturer = _store.GetLecturer(id);
            if (lecturer == null)
            {
                return NotFound();
            }
            return View(lecturer);
        }

        // GET: /Lecturers/Create
        public IActionResult Create()
        {
            // Create a new empty lecturer for the form
            var lecturer = new Lecturer();
            return View(lecturer);
        }

        // POST: /Lecturers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Lecturer lecturer)
        {
            if (ModelState.IsValid)
            {
                _store.AddLecturer(lecturer);
                return RedirectToAction(nameof(Index));
            }
            return View(lecturer);
        }

        // GET: /Lecturers/Edit/5
        public IActionResult Edit(int id)
        {
            var lecturer = _store.GetLecturer(id);
            if (lecturer == null)
            {
                return NotFound();
            }
            return View(lecturer);
        }

        // POST: /Lecturers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Lecturer lecturer)
        {
            if (id != lecturer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var updated = _store.UpdateLecturer(lecturer);
                if (!updated)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(lecturer);
        }

        // GET: /Lecturers/Delete/5
        public IActionResult Delete(int id)
        {
            var lecturer = _store.GetLecturer(id);
            if (lecturer == null)
            {
                return NotFound();
            }
            return View(lecturer);
        }

        // POST: /Lecturers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var deleted = _store.DeleteLecturer(id);
            if (!deleted)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}