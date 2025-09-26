// backend/Controllers/LeadershipController.cs

using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs;

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
            return int.Parse(userIdString);
        }

        // POST /api/leadership/vendors - Leader adds vendor details (Requirement 1)
        [HttpPost("vendors")]
        public async Task<IActionResult> CreateVendor([FromBody] CreateVendorDto dto)
        {
            var leaderUserId = GetUserId();
            var newVendor = await _leadershipService.CreateVendorAsync(dto, leaderUserId);
            // Use 202 Accepted because the verification is still pending.
            return AcceptedAtAction(nameof(GetVendorById), new { id = newVendor.Id }, newVendor);
        }

        // POST /api/leadership/jobs - Leader creates a new job (Requirement 3, 5)
        [HttpPost("jobs")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
        {
            var leaderUserId = GetUserId();
            var newJob = await _leadershipService.CreateJobAsync(dto, leaderUserId);

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
            var job = await _leadershipService.GetJobByIdAsync(id);
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
            var jobs = await _leadershipService.GetJobsAsync(leaderUserId);
            return Ok(jobs);
        }

        // GET /api/leadership/jobs/{jobId}/employees
        [HttpGet("jobs/{jobId}/employees")]
        public async Task<IActionResult> GetJobEmployees(int jobId)
        {
            var employees = await _leadershipService.GetJobEmployeesAsync(jobId);
            return Ok(employees);
        }

        // GET /api/leadership/countries/{countryCode}/vendors
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
        // This should be mapped by the email verification link.
        [AllowAnonymous]
        [HttpGet("/api/vendor/verify")]
        public async Task<IActionResult> VerifyVendor([FromQuery] Guid token, [FromQuery] bool accept)
        {
            var success = await _leadershipService.VerifyVendorAsync(token, accept);
            if (!success)
            {
                return BadRequest("Verification failed. Link is invalid or expired.");
            }
            // Front-end should handle the final display/redirect.
            return Ok(new { message = accept ? "Vendor details accepted and verified." : "Vendor details rejected." });
        }

        // NOTE: GetVendorById is assumed to exist for CreatedAtAction to work.
        [HttpGet("vendors/{id}")]
        public async Task<IActionResult> GetVendorById(int id)
        {
            // Implementation detail: Add a GetVendorByIdAsync to your LeadershipService.
            return Ok();
        }
    }
}