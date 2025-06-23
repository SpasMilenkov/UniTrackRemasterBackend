namespace UniTrackRemaster.Data.Exceptions;

public class ForbiddenException : BusinessException
{
    public ForbiddenException(string message) : base(message) { }

    public ForbiddenException() : base("You do not have permission to perform this action.") { }
}
