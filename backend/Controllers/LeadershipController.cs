using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs;
using System;
using System.Linq;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Leadership")]
    public class LeadershipController : ControllerBase
    {
        private readonly LeadershipService _leadershipService;
        public LeadershipController(LeadershipService leadershipService)
        {
            _leadershipService = leadershipService;
        }

        private int GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Safety check for user ID extraction
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                throw new UnauthorizedAccessException("User ID is missing or invalid in the token.");
            }
            return userId;
        }

        // POST /api/leadership/vendors
        [HttpPost("vendors")]
        public async Task<IActionResult> CreateVendor([FromBody] CreateVendorDto dto)
        {
            var leaderUserId = GetUserId();
            var newVendor = await _leadershipService.CreateVendorAsync(dto, leaderUserId);
            return AcceptedAtAction(nameof(GetVendorById), new { id = newVendor.Id }, newVendor);
        }

        // POST /api/leadership/jobs 
        [HttpPost("jobs")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
        {
            var leaderUserId = GetUserId();
            // This method MUST exist in LeadershipService
            var newJob = await _leadershipService.CreateJobAsync(dto, leaderUserId); // FIX: CS1061 for CreateJobAsync

            if (newJob == null)
            {
                return BadRequest("Failed to create job or assign vendors. Check if vendors exist in the specified country and are verified.");
            }
            return CreatedAtAction(nameof(GetJobById), new { id = newJob.Id }, newJob);
        }

        // GET /api/leadership/jobs/{id}
        [HttpGet("jobs/{id}")]
        public async Task<IActionResult> GetJobById(int id)
        {
            // This method MUST exist in LeadershipService
            var job = await _leadershipService.GetJobByIdAsync(id); // FIX: CS1061 for GetJobByIdAsync
            if (job == null)
            {
                return NotFound();
            }
            return Ok(job);
        }

        // GET /api/leadership/jobs
        [HttpGet("jobs")]
        public async Task<IActionResult> GetJobs()
        {
            var leaderUserId = GetUserId();
            // This method MUST exist in LeadershipService
            var jobs = await _leadershipService.GetJobsAsync(leaderUserId); // FIX: CS1061 for GetJobsAsync
            return Ok(jobs);
        }

        // GET /api/leadership/jobs/{jobId}/employees
        [HttpGet("jobs/{jobId}/employees")]
        public async Task<IActionResult> GetJobEmployees(int jobId)
        {
            // This method MUST exist in LeadershipService
            var employees = await _leadershipService.GetJobEmployeesAsync(jobId); // FIX: CS1061 for GetJobEmployeesAsync
            return Ok(employees);
        }

        // GET /api/leadership/countries/{countryCode}/vendors (Assumed to be correct)
        [HttpGet("countries/{countryCode}/vendors")]
        public async Task<IActionResult> GetVendorsByCountry(string countryCode)
        {
            var vendors = await _leadershipService.GetVendorsByCountryAsync(countryCode);
            if (vendors == null || !vendors.Any())
            {
                return NotFound("No vendors found for the specified country.");
            }
            return Ok(vendors);
        }

        // NEW: Unauthenticated endpoint for Vendor to verify or reject details (Requirement 1)
        [AllowAnonymous]
        [HttpGet("/api/vendor/verify")]
        public async Task<IActionResult> VerifyVendor([FromQuery] Guid token, [FromQuery] bool accept)
        {
            // This method MUST exist in LeadershipService
            var success = await _leadershipService.VerifyVendorAsync(token, accept); // FIX: CS1061 for VerifyVendorAsync
            if (!success)
            {
                return BadRequest("Verification failed. Link is invalid or expired.");
            }
            return Ok(new { message = accept ? "Vendor details accepted and verified." : "Vendor details rejected." });
        }

        // Helper endpoint assumed to exist for CreatedAtAction
        [HttpGet("vendors/{id}")]
        public async Task<IActionResult> GetVendorById(int id)
        {
            // Implementation is omitted, but required for CreatedAtAction
            return Ok();
        }
    }
}