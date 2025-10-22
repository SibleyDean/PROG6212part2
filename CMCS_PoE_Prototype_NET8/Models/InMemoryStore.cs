using CMCS.Mvc.Models;
using System.Collections.Generic;
using System.Linq;

namespace CMCS.Mvc
{
    /// <summary>
    /// Central in-memory store for Claims and Lecturers.
    /// Used instead of a database — data resets when the app restarts.
    /// </summary>
    public class InMemoryStore
    {
        // === CLAIMS ===
        private readonly List<Claim> _claims = new();
        private int _nextClaimId = 1;
        private readonly object _claimsLock = new();

        public List<Claim> GetAllClaims()
        {
            lock (_claimsLock)
            {
                return _claims.Select(CloneClaim).ToList();
            }
        }

        public Claim? GetClaim(int id)
        {
            lock (_claimsLock)
            {
                var claim = _claims.FirstOrDefault(c => c.Id == id);
                return claim != null ? CloneClaim(claim) : null;
            }
        }

        public Claim AddClaim(Claim claim)
        {
            lock (_claimsLock)
            {
                var copy = CloneClaim(claim);
                copy.Id = _nextClaimId++;
                _claims.Add(copy);
                return CloneClaim(copy);
            }
        }

        public bool UpdateClaim(Claim claim)
        {
            lock (_claimsLock)
            {
                var existing = _claims.FirstOrDefault(c => c.Id == claim.Id);
                if (existing == null) return false;

                existing.Title = claim.Title;
                existing.Description = claim.Description;
                existing.Hours = claim.Hours;
                existing.Rate = claim.Rate;
                existing.FilePath = claim.FilePath;
                existing.Status = claim.Status;
                return true;
            }
        }

        public bool DeleteClaim(int id)
        {
            lock (_claimsLock)
            {
                var c = _claims.FirstOrDefault(x => x.Id == id);
                if (c == null) return false;

                _claims.Remove(c);
                return true;
            }
        }

        private static Claim CloneClaim(Claim c) => new()
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            Hours = c.Hours,
            Rate = c.Rate,
            FilePath = c.FilePath,
            Status = c.Status
        };

        // === LECTURERS ===
        private readonly List<Lecturer> _lecturers = new();
        private int _nextLecturerId = 1;
        private readonly object _lecturerLock = new();

        public List<Lecturer> GetAllLecturers()
        {
            lock (_lecturerLock)
            {
                return _lecturers.Select(CloneLecturer).ToList();
            }
        }

        public Lecturer? GetLecturer(int id)
        {
            lock (_lecturerLock)
            {
                var lecturer = _lecturers.FirstOrDefault(l => l.Id == id);
                return lecturer != null ? CloneLecturer(lecturer) : null;
            }
        }

        public Lecturer AddLecturer(Lecturer lecturer)
        {
            lock (_lecturerLock)
            {
                var copy = CloneLecturer(lecturer);
                copy.Id = _nextLecturerId++;
                _lecturers.Add(copy);
                return CloneLecturer(copy);
            }
        }

        public bool UpdateLecturer(Lecturer lecturer)
        {
            lock (_lecturerLock)
            {
                var existing = _lecturers.FirstOrDefault(l => l.Id == lecturer.Id);
                if (existing == null) return false;

                existing.Name = lecturer.Name;
                existing.Email = lecturer.Email;
                existing.Department = lecturer.Department;
                existing.Phone = lecturer.Phone;
                return true;
            }
        }

        public bool DeleteLecturer(int id)
        {
            lock (_lecturerLock)
            {
                var existing = _lecturers.FirstOrDefault(l => l.Id == id);
                if (existing == null) return false;

                _lecturers.Remove(existing);
                return true;
            }
        }

        private static Lecturer CloneLecturer(Lecturer l) => new()
        {
            Id = l.Id,
            Name = l.Name ?? string.Empty,
            Email = l.Email ?? string.Empty,
            Department = l.Department ?? string.Empty,
            Phone = l.Phone ?? string.Empty
        };

        // Add to InMemoryStore class
        public List<Claim> GetPendingClaims()
        {
            lock (_claimsLock)
            {
                return _claims.Where(c => c.Status == "Pending")
                             .Select(CloneClaim)
                             .ToList();
            }
        }

        public bool UpdateClaimStatus(int claimId, string newStatus)
        {
            lock (_claimsLock)
            {
                var claim = _claims.FirstOrDefault(c => c.Id == claimId);
                if (claim == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Claim {claimId} not found for status update");
                    return false;
                }

                claim.Status = newStatus;
                System.Diagnostics.Debug.WriteLine($"Claim {claimId} status updated to: {newStatus}");
                return true;
            }
        }
    }
}