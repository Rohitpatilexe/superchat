using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // GET /api/leadership/search?type=...&query=...
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string type, [FromQuery] string query)
        {
            var result = await _leadershipService.SearchAsync(type, query);
            if (result == null)
            {
                return BadRequest("Invalid search type or query.");
            }
            return Ok(result);
        }

        // GET /api/leadership/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var vendors = await _leadershipService.GetDashboardAsync();
            return Ok(vendors);
        }

        // GET /api/leadership/vendors/{id}
        [HttpGet("vendors/{id}")]
        public async Task<IActionResult> GetVendorById(int id)
        {
            var vendor = await _leadershipService.GetVendorByIdAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }
            return Ok(vendor);
        }

        // GET /api/leadership/vendors/{vendorId}/employees/{employeeId}
        [HttpGet("vendors/{vendorId}/employees/{employeeId}")]
        public async Task<IActionResult> GetEmployeeById(int vendorId, int employeeId)
        {
            var employeeDetails = await _leadershipService.GetEmployeeByIdAsync(vendorId, employeeId);
            if (employeeDetails == null)
            {
                return NotFound();
            }
            return Ok(employeeDetails);
        }

        // New endpoint to get all jobs for leadership.
        // GET /api/leadership/jobs
        [HttpGet("jobs")]
        public async Task<IActionResult> GetAllJobs()
        {
            var jobs = await _leadershipService.GetAllJobsAsync();
            return Ok(jobs);
        }

        // New endpoint to assign a job to one or more vendors.
        // POST /api/leadership/jobs/{jobId}/assign-vendors
        [HttpPost("jobs/{jobId}/assign-vendors")]
        public async Task<IActionResult> AssignVendorsToJob(int jobId, [FromBody] List<int> vendorIds)
        {
            var success = await _leadershipService.AssignVendorsToJobAsync(jobId, vendorIds);
            if (!success)
            {
                return BadRequest("Failed to assign vendors to job.");
            }
            return Ok(new { message = "Vendors assigned successfully." });
        }
    }
}
