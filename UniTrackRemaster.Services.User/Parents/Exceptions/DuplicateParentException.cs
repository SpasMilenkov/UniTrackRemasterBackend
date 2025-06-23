namespace UniTrackRemaster.Services.User.Parents.Exceptions;

/// <summary>
/// Exception thrown when attempting to create a parent profile that already exists
/// </summary>
public class DuplicateParentException : ParentException
{
    public Guid UserId { get; }

    public DuplicateParentException(Guid userId) 
        : base($"A parent profile already exists for user with ID: {userId}")
    {
        UserId = userId;
    }
}
