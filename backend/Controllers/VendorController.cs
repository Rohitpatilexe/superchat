using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/vendor")]
    [Authorize(Roles = "Vendor")] // Restrict access to vendors only
    public class VendorController : ControllerBase
    {
        private readonly VendorService _vendorService;

        public VendorController(VendorService vendorService)
        {
            _vendorService = vendorService;
        }

        private int GetVendorId()
        {
            // This method extracts the VendorId from the JWT token claims.
            // You will need to add the VendorId to the claims when the vendor user logs in.
            var vendorIdClaim = User.FindFirst("VendorId")?.Value;
            if (vendorIdClaim == null)
            {
                throw new UnauthorizedAccessException("Vendor ID not found in token.");
            }
            return int.Parse(vendorIdClaim);
        }

        private int GetUserId()
        {
            // Extracts the User Id from the JWT token claims.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return int.Parse(userIdClaim);
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            var vendorId = GetVendorId();
            var employees = await _vendorService.GetEmployeesAsync(vendorId);
            return Ok(employees);
        }

        [HttpPost("employees")]
        public async Task<IActionResult> AddEmployee([FromBody] Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var vendorId = GetVendorId();
                var userId = GetUserId();
                var newEmployee = await _vendorService.AddEmployeeAsync(employee, vendorId, userId);
                return CreatedAtAction(nameof(GetEmployeeById), new { id = newEmployee.Id }, newEmployee);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message); // 409 Conflict for duplicate employee code
            }
        }

        [HttpGet("employees/{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var vendorId = GetVendorId();
            var employee = await _vendorService.GetEmployeeByIdAsync(id, vendorId);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        [HttpPut("employees/{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee employeeUpdate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vendorId = GetVendorId();
            var updatedEmployee = await _vendorService.UpdateEmployeeAsync(id, vendorId, employeeUpdate);
            if (updatedEmployee == null)
            {
                return NotFound();
            }
            return Ok(updatedEmployee);
        }

        [HttpDelete("employees/{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var vendorId = GetVendorId();
            var result = await _vendorService.DeleteEmployeeAsync(id, vendorId);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}