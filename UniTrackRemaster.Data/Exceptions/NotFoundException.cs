namespace UniTrackRemaster.Data.Exceptions;

public class NotFoundException : BusinessException
{
    public NotFoundException(string message) : base(message) { }
}