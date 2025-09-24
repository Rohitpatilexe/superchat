using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs;

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

        // For an admin to approve a vendor.
        // PUT /api/admin/vendors/{id}/approve
        [HttpPut("vendors/{id}/approve")]
        public async Task<IActionResult> ApproveVendor(int id)
        {
            var success = await _adminService.ApproveVendorAsync(id);
            if (!success) return BadRequest("Vendor approval failed.");
            return Ok(new { message = "Vendor approved successfully." });
        }

        // For an admin to reject a vendor.
        // PUT /api/admin/vendors/{id}/reject
        [HttpPut("vendors/{id}/reject")]
        public async Task<IActionResult> RejectVendor(int id, [FromBody] RejectVendorRequest request)
        {
            var success = await _adminService.RejectVendorAsync(id, request.Reason);
            if (!success) return BadRequest("Vendor rejection failed.");
            return Ok(new { message = "Vendor rejected." });
        }
        
        // To get a list of all vendors.
        // GET /api/admin/vendors
        [HttpGet("vendors")]
        public async Task<IActionResult> GetAllVendors()
        {
            return Ok(await _adminService.GetAllVendorsAsync());
        }

        // To get a single vendor by their ID.
        // GET /api/admin/vendors/{id}
        [HttpGet("vendors/{id}")]
        public async Task<IActionResult> GetVendorById(int id)
        {
            var vendor = await _adminService.GetVendorByIdAsync(id);
            if (vendor == null) return NotFound();
            return Ok(vendor);
        }

        // Endpoint to delete a vendor.
        // DELETE /api/admin/vendors/{id}
        [HttpDelete("vendors/{id}")]
        public async Task<IActionResult> DeleteVendor(int id)
        {
            var success = await _adminService.DeleteVendorAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // New endpoint to get all jobs.
        // GET /api/admin/jobs
        [HttpGet("jobs")]
        public async Task<IActionResult> GetAllJobs()
        {
            return Ok(await _adminService.GetAllJobsAsync());
        }

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

        // New endpoint to get a job by ID.
        // GET /api/admin/jobs/{id}
        [HttpGet("jobs/{id}")]
        public async Task<IActionResult> GetJobById(int id)
        {
            var job = await _adminService.GetJobByIdAsync(id);
            if (job == null) return NotFound();
            return Ok(job);
        }
    }
}
