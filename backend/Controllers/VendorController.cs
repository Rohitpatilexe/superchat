using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Vendor")]
    public class VendorController : ControllerBase
    {
        private readonly VendorService _vendorService;

        public VendorController(VendorService vendorService)
        {
            _vendorService = vendorService;
        }

        private Guid GetVendorPublicId()
        {
            var publicIdString = User.FindFirstValue("PublicId");
            if (string.IsNullOrEmpty(publicIdString))
            {
                throw new InvalidOperationException("Vendor PublicId claim not found.");
            }
            return Guid.Parse(publicIdString);
        }

        // POST /api/vendor/employees
        [HttpPost("employees")]
        public async Task<IActionResult> CreateEmployee([FromForm] CreateEmployeeDto dto)
        {
            var vendorPublicId = GetVendorPublicId();
            var employee = await _vendorService.CreateEmployeeAsync(dto, vendorPublicId);
            if (employee == null)
            {
                return BadRequest("Could not create employee.");
            }
            return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, employee);
        }

        // GET /api/vendor/employees/{id}
        [HttpGet("employees/{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var vendorPublicId = GetVendorPublicId();
            var employee = await _vendorService.GetEmployeeByIdAsync(id, vendorPublicId);
            if (employee == null) return NotFound();
            return Ok(employee);
        }

        // GET /api/vendor/jobs
        [HttpGet("jobs")]
        public async Task<IActionResult> GetAssignedJobs()
        {
            var vendorPublicId = GetVendorPublicId();
            var jobs = await _vendorService.GetAssignedJobsAsync(vendorPublicId);
            return Ok(jobs);
        }

        // GET /api/vendor/jobs/{jobId}/employees
        [HttpGet("jobs/{jobId}/employees")]
        public async Task<IActionResult> GetEmployeesForJob(int jobId)
        {
            var vendorPublicId = GetVendorPublicId();
            var employees = await _vendorService.GetEmployeesForJobAsync(jobId, vendorPublicId);
            return Ok(employees);
        }
    }
}