using Amazon.S3;
using Amazon.S3.Model;
using backend.Config;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    // NOTE: This interface definition must be included or defined elsewhere for compilation.
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

        // 1. VENDOR MANAGEMENT METHODS (Required by LeadershipController)

        // Creates vendor details and initiates the verification link process (Requirement 1)
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

            // Sends the link to the vendor
            await _emailService.SendVerificationEmail(vendor.ContactEmail, vendor.VerificationToken.Value);

            // Returns all 7 required arguments for VendorDto
            return new VendorDto(vendor.Id, vendor.CompanyName, vendor.ContactEmail, vendor.Country, vendor.Status, vendor.CreatedAt, vendor.AddedByLeaderId);
        }

        // Vendor uses the link to Accept or Reject the entered details (Requirement 1)
        public async Task<bool> VerifyVendorAsync(Guid verificationToken, bool accept)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v =>
                v.VerificationToken == verificationToken &&
                v.TokenExpiry > DateTime.UtcNow
            );

            if (vendor == null) return false;

            vendor.Status = accept ? "Verified" : "Rejected by Vendor";

            vendor.VerificationToken = null;
            vendor.TokenExpiry = null;
            vendor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // Retrieves all vendors from a specific country (Required by LeadershipController)
        public async Task<IEnumerable<VendorDto>?> GetVendorsByCountryAsync(string countryCode)
        {
            var vendors = await _context.Vendors
                .Where(v => v.Country == countryCode)
                .Select(v => new VendorDto(v.Id, v.CompanyName, v.ContactEmail, v.Country, v.Status, v.CreatedAt, v.AddedByLeaderId))
                .ToListAsync();

            return vendors.Any() ? vendors : null;
        }

        // 2. JOB MANAGEMENT METHODS (Required by LeadershipController)

        // Creates a new job and assigns vendors (Requirement 3, 5)
        public async Task<JobDto?> CreateJobAsync(CreateJobDto dto, int leaderUserId)
        {
            // Filter vendors by provided IDs and country, ensuring they are verified (Requirement 5)
            var vendors = await _context.Vendors
                .Where(v => dto.VendorIds.Contains(v.Id) && v.Country == dto.CountryCode && v.Status == "Verified")
                .ToListAsync();

            if (!vendors.Any()) return null;

            var job = new Job
            {
                Title = dto.Title,
                Description = dto.Description,
                Country = dto.CountryCode, // Job must have a single country (Requirement 5)
                ExpiryDate = dto.ExpiryDate,
                CreatedByLeaderId = leaderUserId,
                CreatedAt = DateTime.UtcNow, // Requirement 4: Date of creation
                IsActive = true
            };

            // Create many-to-many relationship entries (JobVendor)
            foreach (var vendor in vendors)
            {
                job.VendorAssignments.Add(new JobVendor { Vendor = vendor });
            }

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            double daysRemaining = (job.ExpiryDate - DateTime.UtcNow).TotalDays;

            // Returns 6 required arguments for JobDto
            return new JobDto(job.Id, job.Title, job.Description, job.CreatedAt, job.ExpiryDate, daysRemaining);
        }

        // Retrieves all jobs created by a leader (Requirement 4)
        public async Task<IEnumerable<JobDto>> GetJobsAsync(int leaderUserId)
        {
            return await _context.Jobs
                .Where(j => j.CreatedByLeaderId == leaderUserId)
                .Select(j => new JobDto(
                    j.Id,
                    j.Title,
                    j.Description,
                    j.CreatedAt,
                    j.ExpiryDate,
                    (j.ExpiryDate - DateTime.UtcNow).TotalDays // Requirement 4: Days/time remaining
                ))
                .ToListAsync();
        }

        // Retrieves a single job by its ID. (Required by LeadershipController)
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
                   (j.ExpiryDate - DateTime.UtcNow).TotalDays
               ))
               .FirstOrDefaultAsync();
        }

        // Retrieves all employees for a specific job ID (Requirement 4: Who got assigned)
        public async Task<IEnumerable<EmployeeDto>> GetJobEmployeesAsync(int jobId)
        {
            // Retrieves the list of submitted candidates for the job.
            return await _context.Employees
                .Where(e => e.JobId == jobId)
                .Select(e => new EmployeeDto(e.Id, e.FirstName, e.LastName, e.JobTitle))
                .ToListAsync();
        }
    }
}