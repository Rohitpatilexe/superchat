using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;

        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        // POST /api/admin/vendors
        [HttpPost("vendors")]
        public async Task<IActionResult> CreateVendor([FromBody] Vendor vendor)
        {
            if (string.IsNullOrEmpty(vendor.CompanyName) || string.IsNullOrEmpty(vendor.ContactEmail))
            {
                return BadRequest("Company name and contact email are required.");
            }

            // Get the admin's User ID from the JWT token.
            // This is a crucial security step to prevent a client from setting their own admin ID.
            int addedByAdminId = 1; // Placeholder: Replace with logic to get user ID from JWT token.

            var newVendor = await _adminService.CreateVendorAsync(vendor.CompanyName, vendor.ContactEmail, addedByAdminId);
            return CreatedAtAction(nameof(GetVendor), new { id = newVendor.Id }, newVendor);
        }

        // GET /api/admin/vendors
        [HttpGet("vendors")]
        public async Task<ActionResult<IEnumerable<Vendor>>> GetVendors()
        {
            var vendors = await _adminService.GetAllVendorsAsync();
            return Ok(vendors);
        }

        // GET /api/admin/vendors/{id}
        [HttpGet("vendors/{id}")]
        public async Task<ActionResult<Vendor>> GetVendor(int id)
        {
            var vendor = await _adminService.GetVendorByIdAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }
            return Ok(vendor);
        }

        // PUT /api/admin/vendors/{id}/verify
        [HttpPut("vendors/{id}/verify")]
        public async Task<IActionResult> VerifyVendor(int id)
        {
            // Get the admin's User ID from the JWT token.
            int updatedByAdminId = 1; // Placeholder
            var result = await _adminService.VerifyVendorAsync(id, updatedByAdminId);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        // PUT /api/admin/vendors/{id}/reject
        [HttpPut("vendors/{id}/reject")]
        public async Task<IActionResult> RejectVendor(int id, [FromBody] string reason)
        {
            // Get the admin's User ID from the JWT token.
            int updatedByAdminId = 1; // Placeholder
            var result = await _adminService.RejectVendorAsync(id, reason, updatedByAdminId);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        // DELETE /api/admin/vendors/{id}
        [HttpDelete("vendors/{id}")]
        public async Task<IActionResult> DeleteVendor(int id)
        {
            var result = await _adminService.DeleteVendorAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
