namespace UniTrackRemaster.Services.User.Parents.Exceptions;

/// <summary>
/// Exception thrown when attempting to associate a student who already has the maximum number of parents
/// </summary>
public class MaxParentsExceededException : ParentException
{
    public Guid StudentId { get; }
    public int MaxParents { get; }

    public MaxParentsExceededException(Guid studentId, int maxParents = 2) 
        : base($"Student {studentId} already has the maximum number of parents ({maxParents})")
    {
        StudentId = studentId;
        MaxParents = maxParents;
    }
}
