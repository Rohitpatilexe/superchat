// backend/DTOs/AppDtos.cs

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
    public record AdminCreateVendorRequest(string CompanyName, string ContactEmail);
    public record RejectVendorRequest(string? Reason);

    // --- Leadership DTOs ---
    public record CreateVendorDto(
        [Required] string CompanyName,
        [Required][EmailAddress] string ContactEmail,
        [Required] string Country
    );
    public record VendorDto(
        int Id,
        string CompanyName,
        string ContactEmail,
        string Status,
        DateTime CreatedAt,
        int AddedByLeaderId
    );
    public record VendorDetailDto(
        int Id,
        string CompanyName,
        string ContactEmail,
        string Status,
        List<EmployeeDto> Employees
    );
    public record SearchResultDto(string Type, List<object> Results);

    // Updated Job DTO to include DaysRemaining for notification/display (Requirement 4)
    // From All.cs
    public record JobDto(
        int Id,
        string Title,
        string Description,
        DateTime CreatedAt,
        DateTime ExpiryDate,
        double DaysRemaining // Must be 6th property
    );

    public record CreateJobDto(
        [Required] string Title,
        [Required] string Description,
        [Required] string CountryCode,
        [Required] DateTime ExpiryDate,
        [Required] List<int> VendorIds
    );

    // --- Vendor DTOs ---
    public class CreateEmployeeDto
    {
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public IFormFile? ResumeFile { get; set; }
        [Required] public int JobId { get; set; } // Required for employee-to-job mapping
    }
    public record UpdateEmployeeDto(string FirstName, string LastName, string? JobTitle);
    public record EmployeeDto(int Id, string FirstName, string LastName, string? JobTitle);
    public record EmployeeDetailDto(
        int Id,
        string FirstName,
        string LastName,
        string? JobTitle,
        int VendorId,
        string? ResumeDownloadUrl
    );
}