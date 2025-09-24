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
        
        // Defines the endpoint at GET /api/vendor/employees
        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            var vendorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var employees = await _vendorService.GetEmployeesAsync(vendorUserId);
            return Ok(employees);
        }

        // Defines the endpoint at POST /api/vendor/employees
        // Updated to include JobId from the DTO.
        [HttpPost("employees")]
        public async Task<IActionResult> CreateEmployee([FromForm] CreateEmployeeDto dto)
        {
            var vendorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var employee = await _vendorService.CreateEmployeeAsync(dto, vendorUserId);
            if (employee == null)
            {
                return BadRequest("Could not create employee.");
            }
            return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, employee);
        }
        
        // Defines the endpoint at GET /api/vendor/employees/{id}
        [HttpGet("employees/{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var vendorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var employee = await _vendorService.GetEmployeeByIdAsync(id, vendorUserId);
            if (employee == null) return NotFound();
            return Ok(employee);
        }

        // Defines the endpoint at PUT /api/vendor/employees/{id}
        [HttpPut("employees/{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
        {
            var vendorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var employee = await _vendorService.UpdateEmployeeAsync(id, dto, vendorUserId);
            if (employee == null) return NotFound();
            return Ok(employee);
        }

        // Defines the endpoint at DELETE /api/vendor/employees/{id}
        [HttpDelete("employees/{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var vendorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var success = await _vendorService.DeleteEmployeeAsync(id, vendorUserId);
            if (!success) return NotFound();
            return NoContent();
        }

        // New endpoint to get the jobs assigned to the current vendor.
        // GET /api/vendor/jobs
        [HttpGet("jobs")]
        public async Task<IActionResult> GetAssignedJobs()
        {
            var vendorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var jobs = await _vendorService.GetAssignedJobsAsync(vendorUserId);
            return Ok(jobs);
        }
    }
}
