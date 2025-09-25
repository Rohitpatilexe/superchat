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

        // Helper method to get the authenticated user's ID.
        private int GetUserId()
        {
            // This is a secure way to get the user's ID from their JWT claims.
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // It's a good practice to handle potential nulls or parse errors, though roles should prevent this.
            return int.Parse(userIdString);
        }

        // POST /api/leadership/jobs
        // Endpoint to create a new job.
        // It accepts a CreateJobDto containing job details and a list of vendor IDs.
        [HttpPost("jobs")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
        {
            var leaderUserId = GetUserId();
            var newJob = await _leadershipService.CreateJobAsync(dto, leaderUserId);

            if (newJob == null)
            {
                // Return a 400 Bad Request if the job creation or vendor assignment fails.
                return BadRequest("Failed to create job or assign vendors. Check if vendors exist in the specified country.");
            }
            // Return a 201 Created response, a RESTful best practice.
            // This includes the location of the new resource and the created object.
            return CreatedAtAction(nameof(GetJobById), new { id = newJob.Id }, newJob);
        }

        // GET /api/leadership/jobs/{id}
        // Endpoint to get a specific job by its ID.
        [HttpGet("jobs/{id}")]
        public async Task<IActionResult> GetJobById(int id)
        {
            var job = await _leadershipService.GetJobByIdAsync(id);
            if (job == null)
            {
                // Return 404 Not Found if the job doesn't exist.
                return NotFound();
            }
            return Ok(job);
        }

        // GET /api/leadership/jobs
        // Endpoint to get all jobs created by the current leader.
        [HttpGet("jobs")]
        public async Task<IActionResult> GetJobs()
        {
            var leaderUserId = GetUserId();
            var jobs = await _leadershipService.GetJobsAsync(leaderUserId);
            return Ok(jobs);
        }

        // GET /api/leadership/jobs/{jobId}/employees
        // Endpoint to get all employees mapped to a specific job.
        [HttpGet("jobs/{jobId}/employees")]
        public async Task<IActionResult> GetJobEmployees(int jobId)
        {
            var employees = await _leadershipService.GetJobEmployeesAsync(jobId);
            return Ok(employees);
        }

        // New: Endpoint to retrieve all vendors for a specific country
        // This is a new requirement. We'll need a new method in the service.
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
    }
}