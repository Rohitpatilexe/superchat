// backend/Services/LeadershipService.cs

using Amazon.S3;
using Amazon.S3.Model;
using backend.Config;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Services
{
    // Placeholder for an email service (needed for Requirement 1)
    public interface ISomeEmailService
    {
        Task SendVerificationEmail(string toEmail, Guid token);
    }

    public class LeadershipService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAmazonS3 _s3Client;
        private readonly ISomeEmailService _emailService;

        public LeadershipService(ApplicationDbContext context, IAmazonS3 s3Client, ISomeEmailService emailService)
        {
            _context = context;
            _s3Client = s3Client;
            _emailService = emailService;
        }

        // NEW: Creates vendor details and initiates the verification link process (Requirement 1)
        public async Task<VendorDto> CreateVendorAsync(CreateVendorDto dto, int leaderUserId)
        {
            var vendor = new Vendor
            {
                CompanyName = dto.CompanyName,
                ContactEmail = dto.ContactEmail,
                Country = dto.Country,
                Status = "Pending Verification",
                VerificationToken = Guid.NewGuid(),
                TokenExpiry = DateTime.UtcNow.AddHours(24),
                AddedByLeaderId = leaderUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            await _emailService.SendVerificationEmail(vendor.ContactEmail, vendor.VerificationToken.Value);

            return new VendorDto(vendor.Id, vendor.CompanyName, vendor.ContactEmail, vendor.Status, vendor.CreatedAt, vendor.AddedByLeaderId);
        }

        // NEW: Vendor uses the link to Accept or Reject the entered details (Requirement 1)
        public async Task<bool> VerifyVendorAsync(Guid verificationToken, bool accept)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v =>
                v.VerificationToken == verificationToken &&
                v.TokenExpiry > DateTime.UtcNow
            );

            if (vendor == null) return false;

            if (accept)
            {
                vendor.Status = "Verified";
            }
            else
            {
                vendor.Status = "Rejected by Vendor";
            }

            vendor.VerificationToken = null;
            vendor.TokenExpiry = null;
            vendor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // NEW: Creates a new job and assigns vendors (Requirement 3, 5)
        public async Task<JobDto?> CreateJobAsync(CreateJobDto dto, int leaderUserId)
        {
            var vendors = await _context.Vendors
                .Where(v => dto.VendorIds.Contains(v.Id) && v.Country == dto.CountryCode && v.Status == "Verified")
                .ToListAsync();

            if (!vendors.Any()) return null;

            var job = new Job
            {
                Title = dto.Title,
                Description = dto.Description,
                Country = dto.CountryCode,
                ExpiryDate = dto.ExpiryDate,
                CreatedByLeaderId = leaderUserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            foreach (var vendor in vendors)
            {
                job.VendorAssignments.Add(new JobVendor { Vendor = vendor });
            }

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            double daysRemaining = (job.ExpiryDate - DateTime.UtcNow).TotalDays;

            // FIX 1: Now passes 6 arguments to JobDto
            return new JobDto(job.Id, job.Title, job.Description, job.CreatedAt, job.ExpiryDate, daysRemaining);
        }

        // Retrieves all jobs created by a leader (Requirement 4)
        public async Task<IEnumerable<JobDto>> GetJobsAsync(int leaderUserId)
        {
            // Calculate days remaining for notification/display
            return await _context.Jobs
                .Where(j => j.CreatedByLeaderId == leaderUserId)
                .Select(j => new JobDto(
                    j.Id,
                    j.Title,
                    j.Description,
                    j.CreatedAt,
                    j.ExpiryDate,
                    // FIX 2: Passes 6 arguments to JobDto in the Select statement
                    (j.ExpiryDate - DateTime.UtcNow).TotalDays // Days/time remaining calculation
                ))
                .ToListAsync();
        }

        // NEW: Retrieves a single job by its ID.
        public async Task<JobDto?> GetJobByIdAsync(int jobId)
        {
            return await _context.Jobs
               .Where(j => j.Id == jobId)
               .Select(j => new JobDto(
                   j.Id,
                   j.Title,
                   j.Description,
                   j.CreatedAt,
                   j.ExpiryDate,
                   // FIX 3: Passes 6 arguments to JobDto in the Select statement
                   (j.ExpiryDate - DateTime.UtcNow).TotalDays
               ))
               .FirstOrDefaultAsync();
        }

        // Retrieves all employees for a specific job ID (Requirement 4: Who got assigned)
        public async Task<IEnumerable<EmployeeDto>> GetJobEmployeesAsync(int jobId)
        {
            return await _context.Employees
                .Where(e => e.JobId == jobId)
                .Select(e => new EmployeeDto(e.Id, e.FirstName, e.LastName, e.JobTitle))
                .ToListAsync();
        }

        // Retrieves all vendors from a specific country (Requirement 5)
        public async Task<IEnumerable<VendorDto>?> GetVendorsByCountryAsync(string countryCode)
        {
            var vendors = await _context.Vendors
                .Where(v => v.Country == countryCode)
                .Select(v => new VendorDto(v.Id, v.CompanyName, v.ContactEmail, v.Status, v.CreatedAt, v.AddedByLeaderId))
                .ToListAsync();

            return vendors.Any() ? vendors : null;
        }
    }
}