using CMCS.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace CMCS.Mvc.Controllers
{
    public class AcademicManagerController : Controller
    {
        private readonly InMemoryStore _store;

        public AcademicManagerController(InMemoryStore store)
        {
            _store = store;
        }

        // GET: /AcademicManager
        public IActionResult Index()
        {
            var pendingClaims = _store.GetPendingClaims();
            return View(pendingClaims);
        }

        // GET: /AcademicManager/Review/5
        public IActionResult Review(int id)
        {
            var claim = _store.GetClaim(id);
            if (claim == null) return NotFound();

            return View(claim);
        }

        // POST: /AcademicManager/Approve/5 - Fixed version
        [HttpPost]
        public IActionResult Approve(int id)
        {
            var success = _store.UpdateClaimStatus(id, "Approved");
            if (!success)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to approve claim {id}");
                return NotFound();
            }

            System.Diagnostics.Debug.WriteLine($"Claim {id} approved successfully");
            return RedirectToAction(nameof(Index));
        }

        // POST: /AcademicManager/Reject/5 - Fixed version
        [HttpPost]
        public IActionResult Reject(int id)
        {
            var success = _store.UpdateClaimStatus(id, "Rejected");
            if (!success)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to reject claim {id}");
                return NotFound();
            }

            System.Diagnostics.Debug.WriteLine($"Claim {id} rejected successfully");
            return RedirectToAction(nameof(Index));
        }
    }
}