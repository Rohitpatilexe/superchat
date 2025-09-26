// backend/Services/VendorService.cs

using Amazon.S3;
using Amazon.S3.Model;
using backend.Config;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.DTOs;

namespace backend.Services
{
    public class VendorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAmazonS3 _s3Client;

        public VendorService(ApplicationDbContext context, IAmazonS3 s3Client)
        {
            _context = context;
            _s3Client = s3Client;
        }

        // Retrieves all employees for the currently logged-in vendor.
        public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(Guid vendorPublicId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.PublicId == vendorPublicId);
            if (vendor == null) return new List<EmployeeDto>();

            return await _context.Employees
                .Where(e => e.VendorId == vendor.Id)
                .Select(e => new EmployeeDto(e.Id, e.FirstName, e.LastName, e.JobTitle))
                .ToListAsync();
        }

        // Creates a new employee and associates them with a Job.
        public async Task<EmployeeDto?> CreateEmployeeAsync(CreateEmployeeDto dto, Guid vendorPublicId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.PublicId == vendorPublicId);
            if (vendor == null) return null;

            // Check if the vendor is actually assigned to the job before allowing creation.
            var isAssigned = await _context.JobVendors.AnyAsync(jv => jv.JobId == dto.JobId && jv.VendorId == vendor.Id);
            if (!isAssigned)
            {
                return null;
            }

            string? resumeS3Key = null;
            if (dto.ResumeFile != null)
            {
                resumeS3Key = $"resumes/vendor-{vendor.Id}/{Guid.NewGuid()}-{dto.ResumeFile.FileName}";

                var putRequest = new PutObjectRequest
                {
                    BucketName = Environment.GetEnvironmentVariable("S3_BUCKET"),
                    Key = resumeS3Key,
                    InputStream = dto.ResumeFile.OpenReadStream(),
                    ContentType = dto.ResumeFile.ContentType
                };
                await _s3Client.PutObjectAsync(putRequest);
            }

            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                JobTitle = dto.JobTitle,
                ResumeS3Key = resumeS3Key,
                VendorId = vendor.Id,
                JobId = dto.JobId, // Assign the employee to the specific job.
                CreatedByUserId = vendor.UserId ?? 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return new EmployeeDto(employee.Id, employee.FirstName, employee.LastName, employee.JobTitle);
        }

        // Retrieves a single employee by their ID, ensuring they belong to the correct vendor.
        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId, Guid vendorPublicId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.PublicId == vendorPublicId);
            if (vendor == null) return null;

            return await _context.Employees
                .Where(e => e.Id == employeeId && e.VendorId == vendor.Id)
                .Select(e => new EmployeeDto(e.Id, e.FirstName, e.LastName, e.JobTitle))
                .FirstOrDefaultAsync();
        }

        // Retrieves all jobs assigned to a specific vendor.
        public async Task<IEnumerable<JobDto>> GetAssignedJobsAsync(Guid vendorPublicId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.PublicId == vendorPublicId);
            if (vendor == null) return new List<JobDto>();

            // FIX: Must include the DaysRemaining calculation (6th argument) for JobDto.
            return await _context.JobVendors
                .Where(jv => jv.VendorId == vendor.Id)
                .Select(jv => new JobDto(
                    jv.Job.Id,
                    jv.Job.Title,
                    jv.Job.Description,
                    jv.Job.CreatedAt,
                    jv.Job.ExpiryDate,
                    // The missing 6th argument: Calculate time remaining
                    (jv.Job.ExpiryDate - DateTime.UtcNow).TotalDays
                ))
                .ToListAsync();
        }

        // Retrieves all employees submitted by a vendor for a particular job.
        public async Task<IEnumerable<EmployeeDto>> GetEmployeesForJobAsync(int jobId, Guid vendorPublicId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.PublicId == vendorPublicId);
            if (vendor == null) return new List<EmployeeDto>();

            // Query employees filtered by both JobId and the vendor's ID for security.
            return await _context.Employees
                .Where(e => e.JobId == jobId && e.VendorId == vendor.Id)
                .Select(e => new EmployeeDto(e.Id, e.FirstName, e.LastName, e.JobTitle))
                .ToListAsync();
        }

        // You would also need to add UpdateEmployeeAsync and DeleteEmployeeAsync if they are not already here.
    }
}