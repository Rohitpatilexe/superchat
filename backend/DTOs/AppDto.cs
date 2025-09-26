using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace backend.DTOs
{
    // --- Generic DTOs ---
    public record UserDto(int Id, string Email, string? FirstName, string? LastName);
    public record RoleDto(int Id, string Name);

    // --- Auth DTOs ---
    public record LoginRequest(string Email, string Password);
    public record VendorSubmissionRequest(string FirstName, string LastName, string Password);
    public record CreateUserRequest(string Email, string Password);

    // --- Admin DTOs ---
    public record CreateVendorRequest(
        [Required] string CompanyName,
        [Required] string ContactEmail,
        [Required] string Country
    );
    public record RejectVendorRequest(string? Reason);

    // Vendor DTO used by Admin and Leadership
    public record VendorDto(
        int Id,
        string CompanyName,
        string ContactEmail,
        string Country,
        string Status,
        DateTime CreatedAt,
        int AddedByLeaderId
    );

    // NEW: DTO for Admin/Leader to create a new Job (using Admin naming convention)
    public record CreateJobRequest(
        [Required] string Title,
        [Required] string Description,
        [Required] string Country, // The job's country
        [Required] DateTime ExpiryDate
    );

    // --- Leadership DTOs ---
    // The core Job DTO with all 6 required arguments.
    public record JobDto(
        int Id,
        string Title,
        string Description,
        DateTime CreatedAt,
        DateTime ExpiryDate,
        double DaysRemaining // NEW: Calculated time remaining
    );

    // DTO for Leadership to create job (similar to Admin's but includes VendorIds for assignment)
    public record CreateJobDto(
        [Required] string Title,
        [Required] string Description,
        [Required] string CountryCode,
        [Required] DateTime ExpiryDate,
        [Required] List<int> VendorIds
    );

    // DTO for Leadership to create vendor
    public record CreateVendorDto(
        [Required] string CompanyName,
        [Required][EmailAddress] string ContactEmail,
        [Required] string Country
    );

    // --- Vendor DTOs ---
    public class CreateEmployeeDto
    {
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public IFormFile? ResumeFile { get; set; }
        [Required] public int JobId { get; set; }
    }
    public record UpdateEmployeeDto(string FirstName, string LastName, string? JobTitle);
    public record EmployeeDto(int Id, string FirstName, string LastName, string? JobTitle);

    public record VendorDetailDto(
        int Id,
        string CompanyName,
        string ContactEmail,
        string Status,
        List<EmployeeDto> Employees
    );
    public record EmployeeDetailDto(
        int Id,
        string FirstName,
        string LastName,
        string? JobTitle,
        int VendorId,
        string? ResumeDownloadUrl
    );
}