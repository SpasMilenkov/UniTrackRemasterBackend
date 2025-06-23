namespace UniTrackRemaster.Services.User.Parents.Exceptions;


/// <summary>
/// Exception thrown when attempting to add a child that is already associated with the parent
/// </summary>
public class DuplicateChildException : ParentException
{
    public Guid ParentId { get; }
    public Guid StudentId { get; }

    public DuplicateChildException(Guid parentId, Guid studentId) 
        : base($"Student {studentId} is already a child of parent {parentId}")
    {
        ParentId = parentId;
        StudentId = studentId;
    }
}