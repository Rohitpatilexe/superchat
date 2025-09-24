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

        // Retrieves all employees that belong to the currently logged-in vendor.
        public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(int vendorUserId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.UserId == vendorUserId);
            if (vendor == null) return new List<EmployeeDto>();

            return await _context.Employees
                .Where(e => e.VendorId == vendor.Id)
                .Select(e => new EmployeeDto(e.Id, e.FirstName, e.LastName, e.JobTitle))
                .ToListAsync();
        }

        // Retrieves a single employee by their ID, ensuring they belong to the logged-in vendor.
        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId, int vendorUserId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.UserId == vendorUserId);
            if (vendor == null) return null;

            return await _context.Employees
                .Where(e => e.Id == employeeId && e.VendorId == vendor.Id)
                .Select(e => new EmployeeDto(e.Id, e.FirstName, e.LastName, e.JobTitle))
                .FirstOrDefaultAsync();
        }

        // Creates a new employee and uploads their resume to S3 if provided.
        // Updated to accept JobId.
        public async Task<EmployeeDto?> CreateEmployeeAsync(CreateEmployeeDto dto, int vendorUserId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.UserId == vendorUserId);
            if (vendor == null) return null;

            // Optional: Validate that the job exists and is assigned to this vendor.
            var jobAssignment = await _context.JobVendors
                .FirstOrDefaultAsync(jv => jv.JobId == dto.JobId && jv.VendorId == vendor.Id);

            if (jobAssignment == null)
            {
                // This vendor is not assigned to this job.
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
                JobId = dto.JobId, // Added the JobId here
                CreatedByUserId = vendorUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return new EmployeeDto(employee.Id, employee.FirstName, employee.LastName, employee.JobTitle);
        }

        // Updates an existing employee's details.
        public async Task<EmployeeDto?> UpdateEmployeeAsync(int employeeId, UpdateEmployeeDto dto, int vendorUserId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.UserId == vendorUserId);
            if (vendor == null) return null;

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.VendorId == vendor.Id);
            if (employee == null) return null;

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.JobTitle = dto.JobTitle;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedByUserId = vendorUserId;
            // No logic for updating resume is provided, as per the old code's comment.

            await _context.SaveChangesAsync();
            return new EmployeeDto(employee.Id, employee.FirstName, employee.LastName, employee.JobTitle);
        }

        // Deletes an employee record.
        public async Task<bool> DeleteEmployeeAsync(int employeeId, int vendorUserId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.UserId == vendorUserId);
            if (vendor == null) return false;

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.VendorId == vendor.Id);
            if (employee == null) return false;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return true;
        }

        // New method to get all jobs assigned to the current vendor.
        public async Task<IEnumerable<JobDto>> GetAssignedJobsAsync(int vendorUserId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.UserId == vendorUserId);
            if (vendor == null) return new List<JobDto>();

            // Eager load the JobAssignments and then the related Job for the current vendor.
            return await _context.JobVendors
                .Where(jv => jv.VendorId == vendor.Id)
                .Select(jv => new JobDto(jv.Job.Id, jv.Job.Title, jv.Job.Country, jv.Job.CreatedAt, jv.Job.ExpiryDate, jv.Job.IsActive))
                .ToListAsync();
        }
    }
}
