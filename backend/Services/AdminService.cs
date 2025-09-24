using backend.Config;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.DTOs;

namespace backend.Services
{
    public class AdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        public AdminService(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Handles the creation of a new vendor with the new 'Country' field.
        public async Task<VendorDto?> CreateVendorAsync(string companyName, string contactEmail, string country, int addedByLeaderId)
        {
            var vendor = new Vendor
            {
                CompanyName = companyName,
                ContactEmail = contactEmail,
                Country = country, // Added the new 'Country' property
                Status = "PendingInvitation",
                VerificationToken = Guid.NewGuid(),
                TokenExpiry = DateTime.UtcNow.AddDays(7),
                AddedByLeaderId = addedByLeaderId, // Updated from AdminId to AddedByLeaderId
                CreatedAt = DateTime.UtcNow
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            await _emailService.SendInvitationEmailAsync(vendor.ContactEmail, vendor.VerificationToken.Value);

            return new VendorDto(vendor.Id, vendor.CompanyName, vendor.ContactEmail, vendor.Country, vendor.Status, vendor.CreatedAt, vendor.AddedByLeaderId);
        }

        // Approves a vendor who has submitted their details.
        public async Task<bool> ApproveVendorAsync(int vendorId)
        {
            // Note: The Vendor model now uses UserId and AddedByLeaderId, replacing Pending... fields.
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == vendorId && v.Status == "PendingApproval");

            if (vendor == null) return false;

            var vendorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Vendor");
            if (vendorRole == null)
            {
                return false;
            }

            // Create a new User account for the vendor.
            var user = new User
            {
                Email = vendor.ContactEmail,
                PasswordHash = vendor.VerificationToken.ToString(), // A temporary password hash
                FirstName = vendor.CompanyName, // Use CompanyName as a placeholder
            };
            user.Roles.Add(vendorRole);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Link the new User ID to the Vendor.
            vendor.UserId = user.Id;
            vendor.Status = "Verified";
            vendor.VerificationToken = null;
            vendor.TokenExpiry = null;

            await _context.SaveChangesAsync();
            return true;
        }
        
        // Rejects a vendor who is awaiting approval.
        public async Task<bool> RejectVendorAsync(int vendorId, string? reason)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == vendorId && v.Status == "PendingApproval");
            if (vendor == null) return false;

            vendor.Status = "Rejected";
            vendor.VerificationToken = null;
            vendor.TokenExpiry = null;

            await _context.SaveChangesAsync();
            return true;
        }

        // Retrieves a list of all vendors in the system.
        public async Task<IEnumerable<VendorDto>> GetAllVendorsAsync()
        {
            return await _context.Vendors
                .Select(v => new VendorDto(v.Id, v.CompanyName, v.ContactEmail, v.Country, v.Status, v.CreatedAt, v.AddedByLeaderId))
                .ToListAsync();
        }

        // Retrieves a single vendor by their ID.
        public async Task<VendorDetailDto?> GetVendorByIdAsync(int vendorId)
        {
            return await _context.Vendors
                .Where(v => v.Id == vendorId)
                .Select(v => new VendorDetailDto(
                    v.Id,
                    v.CompanyName,
                    v.ContactEmail,
                    v.Status,
                    v.Employees.Select(e => new EmployeeDto(e.Id, e.FirstName, e.LastName, e.JobTitle)).ToList()
                ))
                .FirstOrDefaultAsync();
        }

        // Deletes a vendor from the database.
        public async Task<bool> DeleteVendorAsync(int vendorId)
        {
            var vendor = await _context.Vendors.FindAsync(vendorId);
            if (vendor == null) return false;

            _context.Vendors.Remove(vendor);
            await _context.SaveChangesAsync();
            return true;
        }

        // New method to create a job.
        public async Task<Job> CreateJobAsync(CreateJobRequest request, int createdByLeaderId)
        {
            var job = new Job
            {
                Title = request.Title,
                Description = request.Description,
                Country = request.Country,
                ExpiryDate = request.ExpiryDate,
                CreatedByLeaderId = createdByLeaderId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        // New method to get a single job by ID.
        public async Task<Job?> GetJobByIdAsync(int id)
        {
            return await _context.Jobs
                .Include(j => j.Employees)
                .Include(j => j.VendorAssignments).ThenInclude(jv => jv.Vendor)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        // New method to get all jobs.
        public async Task<IEnumerable<Job>> GetAllJobsAsync()
        {
            return await _context.Jobs
                .Include(j => j.CreatedByLeader)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }
    }
}
