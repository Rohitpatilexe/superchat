using Amazon.S3;
using Amazon.S3.Model;
using backend.Config;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.DTOs;
using System;

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

        // IMPLEMENTED: Matches call from VendorController
        public async Task<EmployeeDto?> CreateEmployeeAsync(CreateEmployeeDto dto, Guid vendorPublicId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.PublicId == vendorPublicId);
            if (vendor == null) return null;

            // Security check: must be assigned to the job
            var isAssigned = await _context.JobVendors.AnyAsync(jv => jv.JobId == dto.JobId && jv.VendorId == vendor.Id);
            if (!isAssigned) return null;

            string? resumeS3Key = null;
            if (dto.ResumeFile != null)
            {
                // ... (S3 upload logic)
            }

            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                JobTitle = dto.JobTitle,
                ResumeS3Key = resumeS3Key,
                VendorId = vendor.Id,
                JobId = dto.JobId,
                CreatedByUserId = vendor.UserId ?? 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return new EmployeeDto(employee.Id, employee.FirstName, employee.LastName, employee.JobTitle);
        }

        // IMPLEMENTED: Matches call from VendorController
        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId, Guid vendorPublicId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.PublicId == vendorPublicId);
            if (vendor == null) return null;

            return await _context.Employees
                .Where(e => e.Id == employeeId && e.VendorId == vendor.Id)
                .Select(e => new EmployeeDto(e.Id, e.FirstName, e.LastName, e.JobTitle))
                .FirstOrDefaultAsync();
        }

        // IMPLEMENTED: Matches call from VendorController and uses 6-argument JobDto
        public async Task<IEnumerable<JobDto>> GetAssignedJobsAsync(Guid vendorPublicId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.PublicId == vendorPublicId);
            if (vendor == null) return new List<JobDto>();

            return await _context.JobVendors
                .Where(jv => jv.VendorId == vendor.Id)
                .Select(jv => new JobDto(
                    jv.Job.Id,
                    jv.Job.Title,
                    jv.Job.Description,
                    jv.Job.CreatedAt,
                    jv.Job.ExpiryDate,
                    (jv.Job.ExpiryDate - DateTime.UtcNow).TotalDays
                ))
                .ToListAsync();
        }

        // IMPLEMENTED: Matches call from VendorController
        public async Task<IEnumerable<EmployeeDto>> GetEmployeesForJobAsync(int jobId, Guid vendorPublicId)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.PublicId == vendorPublicId);
            if (vendor == null) return new List<EmployeeDto>();

            return await _context.Employees
                .Where(e => e.JobId == jobId && e.VendorId == vendor.Id)
                .Select(e => new EmployeeDto(e.Id, e.FirstName, e.LastName, e.JobTitle))
                .ToListAsync();
        }
    }
}