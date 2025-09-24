using Amazon.S3;
using Amazon.S3.Model;
using backend.Config;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.DTOs;

namespace backend.Services
{
    public class LeadershipService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAmazonS3 _s3Client;
        public LeadershipService(ApplicationDbContext context, IAmazonS3 s3Client)
        {
            _context = context;
            _s3Client = s3Client;
        }

        // Provides a generic search capability across different entity types (vendors, employees, or jobs).
        public async Task<SearchResultDto> SearchAsync(string type, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            if (type.Equals("vendor", StringComparison.OrdinalIgnoreCase))
            {
                var results = await _context.Vendors
                    .Where(v => v.CompanyName.Contains(query))
                    .Select(v => new VendorDto(v.Id, v.CompanyName, v.ContactEmail, v.Country, v.Status, v.CreatedAt, v.AddedByLeaderId))
                    .ToListAsync<object>();
                return new SearchResultDto("Vendor", results);
            }
            if (type.Equals("employee", StringComparison.OrdinalIgnoreCase))
            {
                var results = await _context.Employees
                    .Where(e => (e.FirstName + " " + e.LastName).Contains(query))
                    .Select(e => new EmployeeDto(e.Id, e.FirstName, e.LastName, e.JobTitle, e.JobId))
                    .ToListAsync<object>();
                return new SearchResultDto("Employee", results);
            }
            // New search type for jobs.
            if (type.Equals("job", StringComparison.OrdinalIgnoreCase))
            {
                var results = await _context.Jobs
                    .Where(j => j.Title.Contains(query) || j.Description.Contains(query))
                    .Select(j => new JobDto(j.Id, j.Title, j.Country, j.CreatedAt, j.ExpiryDate, j.IsActive))
                    .ToListAsync<object>();
                return new SearchResultDto("Job", results);
            }

            return null;
        }

        // Fetches all vendor data for the main leadership dashboard.
        public async Task<IEnumerable<VendorDto>> GetDashboardAsync()
        {
            return await _context.Vendors
                .Select(v => new VendorDto(v.Id, v.CompanyName, v.ContactEmail, v.Country, v.Status, v.CreatedAt, v.AddedByLeaderId))
                .ToListAsync();
        }

        // Retrieves a single vendor by their ID, including all their associated employees.
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

        // Retrieves a single employee's details and generates a secure, temporary link to their resume.
        public async Task<EmployeeDetailDto?> GetEmployeeByIdAsync(int vendorId, int employeeId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId && e.VendorId == vendorId);

            if (employee == null) return null;

            string? resumeUrl = null;
            if (!string.IsNullOrEmpty(employee.ResumeS3Key))
            {
                resumeUrl = GeneratePresignedUrl(employee.ResumeS3Key);
            }

            return new EmployeeDetailDto(employee.Id, employee.FirstName, employee.LastName, employee.JobTitle, employee.VendorId, resumeUrl);
        }

        // New method to get all jobs.
        public async Task<IEnumerable<JobDto>> GetAllJobsAsync()
        {
            return await _context.Jobs
                .Select(j => new JobDto(j.Id, j.Title, j.Country, j.CreatedAt, j.ExpiryDate, j.IsActive))
                .ToListAsync();
        }

        // New method to assign a job to one or more vendors.
        public async Task<bool> AssignVendorsToJobAsync(int jobId, List<int> vendorIds)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null) return false;

            // Remove existing assignments to prevent duplicates.
            var existingAssignments = await _context.JobVendors
                .Where(jv => jv.JobId == jobId)
                .ToListAsync();
            _context.JobVendors.RemoveRange(existingAssignments);

            // Add new assignments based on the list of vendor IDs.
            foreach (var vendorId in vendorIds)
            {
                var vendor = await _context.Vendors.FindAsync(vendorId);
                if (vendor != null)
                {
                    _context.JobVendors.Add(new JobVendor { JobId = jobId, VendorId = vendorId });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private string GeneratePresignedUrl(string key)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = Environment.GetEnvironmentVariable("S3_BUCKET"),
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(5)
            };
            return _s3Client.GetPreSignedURL(request);
        }
    }
}
