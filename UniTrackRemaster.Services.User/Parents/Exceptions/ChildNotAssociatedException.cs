namespace UniTrackRemaster.Services.User.Parents.Exceptions;

/// <summary>
/// Exception thrown when attempting to remove a child that is not associated with the parent
/// </summary>
public class ChildNotAssociatedException : ParentException
{
    public Guid ParentId { get; }
    public Guid StudentId { get; }

    public ChildNotAssociatedException(Guid parentId, Guid studentId) 
        : base($"Student {studentId} is not a child of parent {parentId}")
    {
        ParentId = parentId;
        StudentId = studentId;
    }
}