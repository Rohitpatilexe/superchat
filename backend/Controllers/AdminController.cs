using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs; // ADDED: Required for all DTO types

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        // Creating a new vendor with the new 'Country' field.
        // POST /api/admin/vendors
        [HttpPost("vendors")]
        public async Task<IActionResult> CreateVendor([FromBody] CreateVendorRequest request)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminIdString) || !int.TryParse(adminIdString, out var adminId))
            {
                return Unauthorized("Invalid user token.");
            }
            // Updated to pass the 'Country' field to the service.
            var vendor = await _adminService.CreateVendorAsync(request.CompanyName, request.ContactEmail, request.Country, adminId);
            return CreatedAtAction(nameof(GetVendorById), new { id = vendor.Id }, vendor);
        }

        // ... (Other AdminController methods remain the same)

        // New endpoint to create a job.
        // POST /api/admin/jobs
        [HttpPost("jobs")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest request)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminIdString) || !int.TryParse(adminIdString, out var adminId))
            {
                return Unauthorized("Invalid user token.");
            }
            var job = await _adminService.CreateJobAsync(request, adminId);
            return CreatedAtAction(nameof(GetJobById), new { id = job.Id }, job);
        }

        // ... (GetJobById and other job/vendor GET/PUT/DELETE endpoints remain the same)
    }
}