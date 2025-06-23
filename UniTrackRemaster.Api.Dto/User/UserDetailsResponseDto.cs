namespace UniTrackRemaster.Api.Dto.User;

public class UserDetailsResponse
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Department { get; set; }
    public bool IsLinked { get; set; }
    public string? Role { get; set; } // Primary role
    public string? InstitutionId { get; set; }
    public string? InstitutionName { get; set; }
    public List<string> AllRoles { get; set; } // All roles the user has

    public UserDetailsResponse(
        Guid id,
        string? firstName,
        string? lastName,
        string? email,
        string? phone,
        string? profileImageUrl,
        string? department,
        bool isLinked,
        string? role,
        string? institutionId = null,
        string? institutionName = null,
        List<string>? allRoles = null)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        ProfileImageUrl = profileImageUrl;
        Department = department;
        IsLinked = isLinked;
        Role = role;
        InstitutionId = institutionId;
        InstitutionName = institutionName;
        AllRoles = allRoles ?? new List<string> { role ?? "guest" };
    }
}