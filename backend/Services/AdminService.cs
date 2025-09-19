using backend.Config;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace backend.Services
{
    public class AdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService; // Assuming a service for sending emails

        public AdminService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<Vendor> CreateVendorAsync(string companyName, string contactEmail, int addedByAdminId)
        {
            var token = Guid.NewGuid();
            var newVendor = new Vendor
            {
                CompanyName = companyName,
                ContactEmail = contactEmail,
                Status = "Pending", // Set initial status
                VerificationToken = token,
                AddedByAdminId = addedByAdminId,
                TokenExpiry = DateTime.UtcNow.AddHours(24) // Token expires in 24 hours
            };

            _context.Vendors.Add(newVendor);
            await _context.SaveChangesAsync();

            // Logic to send invitation email with the setup link
            // In a real app, the link would point to a specific frontend route.
            var setupLink = $"your-frontend-url/vendor/setup?token={token}";
            await _emailService.SendVendorInvitationAsync(contactEmail, setupLink);

            return newVendor;
        }

        public async Task<List<Vendor>> GetAllVendorsAsync()
        {
            return await _context.Vendors.ToListAsync();
        }

        public async Task<Vendor?> GetVendorByIdAsync(int id)
        {
            return await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<bool> VerifyVendorAsync(int id, int updatedByAdminId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);
            if (vendor == null || vendor.Status != "Pending")
            {
                return false;
            }
            vendor.Status = "Verified";
            vendor.UpdatedAt = DateTime.UtcNow;
            vendor.UpdatedByAdminId = updatedByAdminId;
            vendor.VerificationToken = null; // Invalidate the token
            vendor.TokenExpiry = null;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectVendorAsync(int id, string reason, int updatedByAdminId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);
            if (vendor == null || vendor.Status != "Pending")
            {
                return false;
            }
            vendor.Status = "Rejected";
            vendor.UpdatedAt = DateTime.UtcNow;
            vendor.UpdatedByAdminId = updatedByAdminId;
            // You can add a RejectionReason property to the Vendor model if needed.
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteVendorAsync(int id)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);
            if (vendor == null)
            {
                return false;
            }
            // Cascading delete is configured in your DbContext to remove employees.
            _context.Vendors.Remove(vendor);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
