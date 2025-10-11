using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace backend.DTOs
{
    // --- AUTH DTOs ---
    public record LoginRequest(string Email, string Password);

    // DTO for the vendor to submit their password during account setup
    public record VendorSetPasswordRequest([Required] string Password);

    // --- ADMIN DTOs ---
    public record CreateLeaderRequest(
        [Required] string Email,
        [Required] string Password,
        [Required] string FirstName,
        [Required] string LastName
    );
    public record UserDto(Guid PublicId, string Email, string FirstName, string LastName);

    // --- LEADERSHIP DTOs ---
    public record CreateVendorRequest(
        [Required] string CompanyName,
        [Required] string ContactEmail,
        [Required] string Country
    );
    public record VendorDto(
        Guid PublicId,
        string CompanyName,
        string ContactEmail,
        string Country,
        string Status
    );
    public record CreateJobRequest(
        [Required] string Title,
        [Required] string Description,
        [Required] string Country,
        [Required] DateTime ExpiryDate,
        [Required] List<Guid> AssignedVendorPublicIds
    );
    public record JobDto(
        Guid PublicId,
        string Title,
        string Country,
        DateTime CreatedAt,
        DateTime ExpiryDate,
        double DaysRemaining
    );
    public record EmployeeWithVendorDto(
        Guid PublicId,
        string FirstName,
        string LastName,
        string? JobTitle,
        string VendorCompanyName
    );
    public record JobDetailDto(
        Guid PublicId,
        string Title,
        string Description,
        string Country,
        DateTime ExpiryDate,
        UserDto CreatedBy,
        List<VendorDto> AssignedVendors,
        List<EmployeeWithVendorDto> SubmittedEmployees
    );

    // --- VENDOR DTOs ---
    public class CreateEmployeeRequest
    {
        [Required] public Guid JobPublicId { get; set; }
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public IFormFile? ResumeFile { get; set; }
    }
    public record EmployeeDto(
        Guid PublicId,
        string FirstName,
        string LastName,
        string? JobTitle
    );
    // For detailed employee response
    public record EmployeeDetailDto(
        Guid PublicId,
        string FirstName,
        string LastName,
        string? JobTitle,
        DateTime CreatedAt,
        string VendorCompanyName,
        string? ResumeDownloadUrl // This will hold the temporary S3 link
    );

    // --- MESSAGING DTOs ---

    // Used by the Leader in the request body to start a chat
    public record CreateConversationRequest(Guid JobPublicId, Guid VendorPublicId);

    // Used to return message data to the client
    public record MessageDto(
        int Id,
        string Content,
        DateTime SentAt,
        Guid SenderPublicId,
        string SenderName
    );

    // Represents a conversation in a list view, providing summary details.
    public record ConversationDto(
        Guid ConversationPublicId,
        string JobTitle,
        Guid ParticipantPublicId,
        string ParticipantName,
        string? LastMessage,
        DateTime? LastMessageTimestamp
    );
}
public record UpdateEmployeeStatusRequest(string Status);

