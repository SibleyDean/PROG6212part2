
using CMCS.Mvc.Controllers;
using CMCS.Mvc.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

using ModelsClaim = CMCS.Mvc.Models.Claim; 

namespace CMCS.Mvc.Tests.UnitTests
{
    public class ComprehensiveTests
    {
        private readonly InMemoryStore _store;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly ClaimsController _claimsController;
        private readonly LecturersController _lecturersController;
        private readonly AcademicManagerController _academicManagerController;

        public ComprehensiveTests()
        {
            _store = new InMemoryStore();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(m => m.WebRootPath).Returns("wwwroot");

            _claimsController = new ClaimsController(_store, _mockEnvironment.Object);
            _lecturersController = new LecturersController(_store);
            _academicManagerController = new AcademicManagerController(_store);
        }

        // ===== IN MEMORY STORE TESTS =====

        [Fact]
        public void Store_AddClaim_WithValidData_ShouldReturnClaimWithId()
        {
            // Arrange
            var claim = new ModelsClaim
            {
                LecturerName = "Dr. John Smith",
                Title = "Test Claim",
                Description = "Test Description",
                Hours = 10,
                Rate = 100.00m
            };

            // Act
            var result = _store.AddClaim(claim);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Dr. John Smith", result.LecturerName);
            Assert.Equal("Test Claim", result.Title);
        }

        [Fact]
        public void Store_GetClaim_WithExistingId_ShouldReturnClaim()
        {
            // Arrange
            var claim = new ModelsClaim { LecturerName = "Test Lecturer", Title = "Test Title" };
            var addedClaim = _store.AddClaim(claim);

            // Act
            var result = _store.GetClaim(addedClaim.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedClaim.Id, result.Id);
            Assert.Equal("Test Lecturer", result.LecturerName);
        }

        [Fact]
        public void Store_UpdateClaim_WithValidData_ShouldUpdateSuccessfully()
        {
            // Arrange
            var originalClaim = _store.AddClaim(new ModelsClaim
            {
                LecturerName = "Original Name",
                Title = "Original Title"
            });

            var updatedClaim = new ModelsClaim
            {
                Id = originalClaim.Id,
                LecturerName = "Updated Name",
                Title = "Updated Title",
                Hours = 20,
                Rate = 150.00m
            };

            // Act
            var result = _store.UpdateClaim(updatedClaim);
            var retrievedClaim = _store.GetClaim(originalClaim.Id);

            // Assert
            Assert.True(result);
            Assert.NotNull(retrievedClaim);
            Assert.Equal("Updated Name", retrievedClaim.LecturerName);
            Assert.Equal("Updated Title", retrievedClaim.Title);
        }

        [Fact]
        public void Store_DeleteClaim_WithExistingId_ShouldRemoveClaim()
        {
            // Arrange
            var claim = _store.AddClaim(new ModelsClaim { LecturerName = "Test", Title = "Test" });

            // Act
            var result = _store.DeleteClaim(claim.Id);
            var deletedClaim = _store.GetClaim(claim.Id);

            // Assert
            Assert.True(result);
            Assert.Null(deletedClaim);
        }

        [Fact]
        public void Store_GetPendingClaims_ShouldReturnOnlyPendingClaims()
        {
            // Arrange
            var pendingClaim = _store.AddClaim(new ModelsClaim
            {
                LecturerName = "Pending",
                Title = "Pending Claim",
                Status = "Pending"
            });

            var approvedClaim = _store.AddClaim(new ModelsClaim
            {
                LecturerName = "Approved",
                Title = "Approved Claim",
                Status = "Approved"
            });

            // Act
            var result = _store.GetPendingClaims();

            // Assert
            Assert.Single(result);
            Assert.All(result, c => Assert.Equal("Pending", c.Status));
        }

        [Fact]
        public void Store_UpdateClaimStatus_WithValidId_ShouldUpdateStatus()
        {
            // Arrange
            var claim = _store.AddClaim(new ModelsClaim
            {
                LecturerName = "Test",
                Title = "Test",
                Status = "Pending"
            });

            // Act
            var result = _store.UpdateClaimStatus(claim.Id, "Approved");
            var updatedClaim = _store.GetClaim(claim.Id);

            // Assert
            Assert.True(result);
            Assert.Equal("Approved", updatedClaim.Status);
        }

        [Fact]
        public void Store_AddLecturer_WithValidData_ShouldReturnLecturerWithId()
        {
            // Arrange
            var lecturer = new Lecturer
            {
                Name = "Dr. Jane Doe",
                Email = "jane.doe@university.edu",
                Department = "Computer Science",
                Phone = "123-456-7890"
            };

            // Act
            var result = _store.AddLecturer(lecturer);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Dr. Jane Doe", result.Name);
            Assert.Equal("jane.doe@university.edu", result.Email);
        }

        [Fact]
        public void Store_Claim_AmountCalculation_ShouldBeCorrect()
        {
            // Arrange
            var claim = new ModelsClaim
            {
                Hours = 40,
                Rate = 150.00m
            };

            // Act & Assert
            Assert.Equal(6000.00m, claim.Amount);
        }

        // ===== CLAIMS CONTROLLER TESTS =====

        [Fact]
        public void ClaimsController_Index_ShouldReturnViewWithClaims()
        {
            // Arrange
            _store.AddClaim(new ModelsClaim { LecturerName = "Lecturer 1", Title = "Claim 1" });
            _store.AddClaim(new ModelsClaim { LecturerName = "Lecturer 2", Title = "Claim 2" });

            // Act
            var result = _claimsController.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ModelsClaim>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public void ClaimsController_Create_GET_ShouldReturnView()
        {
            // Act
            var result = _claimsController.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void ClaimsController_Create_POST_WithValidData_ShouldRedirectToIndex()
        {
            // Act
            var result = _claimsController.Create("Test Lecturer", "Test Title", "Test Description", 10, 100.00m, null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public void ClaimsController_Create_POST_WithMissingLecturerName_ShouldReturnViewWithError()
        {
            // Act
            var result = _claimsController.Create("", "Test Title", "Test Description", 10, 100.00m, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_claimsController.ModelState.IsValid);
        }

        [Fact]
        public void ClaimsController_Details_WithValidId_ShouldReturnView()
        {
            // Arrange
            var claim = _store.AddClaim(new ModelsClaim { LecturerName = "Test", Title = "Test" });

            // Act
            var result = _claimsController.Details(claim.Id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ModelsClaim>(viewResult.Model);
            Assert.Equal(claim.Id, model.Id);
        }

        [Fact]
        public void ClaimsController_Details_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var result = _claimsController.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // ===== LECTURERS CONTROLLER TESTS =====

        [Fact]
        public void LecturersController_Index_ShouldReturnViewWithLecturers()
        {
            // Arrange
            _store.AddLecturer(new Lecturer { Name = "Lecturer 1", Email = "lecturer1@test.com" });
            _store.AddLecturer(new Lecturer { Name = "Lecturer 2", Email = "lecturer2@test.com" });

            // Act
            var result = _lecturersController.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Lecturer>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public void LecturersController_Create_GET_ShouldReturnView()
        {
            // Act
            var result = _lecturersController.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void LecturersController_Create_POST_WithValidData_ShouldRedirectToIndex()
        {
            // Arrange
            var lecturer = new Lecturer { Name = "Test Lecturer", Email = "test@test.com" };

            // Act
            var result = _lecturersController.Create(lecturer);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public void LecturersController_Edit_GET_WithValidId_ShouldReturnView()
        {
            // Arrange
            var lecturer = _store.AddLecturer(new Lecturer { Name = "Test", Email = "test@test.com" });

            // Act
            var result = _lecturersController.Edit(lecturer.Id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Lecturer>(viewResult.Model);
            Assert.Equal(lecturer.Id, model.Id);
        }

        // ===== ACADEMIC MANAGER CONTROLLER TESTS =====

        [Fact]
        public void AcademicManagerController_Index_ShouldReturnPendingClaims()
        {
            // Arrange
            _store.AddClaim(new ModelsClaim { LecturerName = "Lecturer 1", Title = "Claim 1", Status = "Pending" });
            _store.AddClaim(new ModelsClaim { LecturerName = "Lecturer 2", Title = "Claim 2", Status = "Approved" });

            // Act
            var result = _academicManagerController.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List< ModelsClaim >>(viewResult.Model);
            Assert.Single(model);
            Assert.All(model, c => Assert.Equal("Pending", c.Status));
        }

        [Fact]
        public void AcademicManagerController_Review_WithValidId_ShouldReturnView()
        {
            // Arrange
            var claim = _store.AddClaim(new ModelsClaim { LecturerName = "Test", Title = "Test", Status = "Pending" });

            // Act
            var result = _academicManagerController.Review(claim.Id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ModelsClaim>(viewResult.Model);
            Assert.Equal(claim.Id, model.Id);
        }

        [Fact]
        public void AcademicManagerController_Approve_WithValidId_ShouldUpdateStatusAndRedirect()
        {
            // Arrange
            var claim = _store.AddClaim(new ModelsClaim { LecturerName = "Test", Title = "Test", Status = "Pending" });

            // Act
            var result = _academicManagerController.Approve(claim.Id);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var updatedClaim = _store.GetClaim(claim.Id);
            Assert.Equal("Approved", updatedClaim.Status);
        }

        [Fact]
        public void AcademicManagerController_Reject_WithValidId_ShouldUpdateStatusAndRedirect()
        {
            // Arrange
            var claim = _store.AddClaim(new ModelsClaim { LecturerName = "Test", Title = "Test", Status = "Pending" });

            // Act
            var result = _academicManagerController.Reject(claim.Id);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var updatedClaim = _store.GetClaim(claim.Id);
            Assert.Equal("Rejected", updatedClaim.Status);
        }

        // ===== INTEGRATION STYLE TESTS =====

        [Fact]
        public void EndToEnd_ClaimWorkflow_ShouldWorkCorrectly()
        {
            // Arrange - Create a lecturer
            var lecturer = _store.AddLecturer(new Lecturer
            {
                Name = "Dr. Test Professor",
                Email = "test@university.edu"
            });

            // Act 1 - Submit a claim
            var claimResult = _claimsController.Create("Dr. Test Professor", "Research Hours", "Monthly research work", 40, 200.00m, null);

            // Assert 1 - Claim should be created
            var redirectResult = Assert.IsType<RedirectToActionResult>(claimResult);
            Assert.Equal("Index", redirectResult.ActionName);

            // Act 2 - Get pending claims as academic manager
            var pendingClaims = _store.GetPendingClaims();

            // Assert 2 - Claim should be in pending state
            Assert.Single(pendingClaims);
            Assert.Equal("Pending", pendingClaims[0].Status);

            // Act 3 - Approve the claim
            var approveResult = _academicManagerController.Approve(pendingClaims[0].Id);

            // Assert 3 - Claim should be approved
            Assert.IsType<RedirectToActionResult>(approveResult);
            var approvedClaim = _store.GetClaim(pendingClaims[0].Id);
            Assert.Equal("Approved", approvedClaim.Status);
        }
    }
}