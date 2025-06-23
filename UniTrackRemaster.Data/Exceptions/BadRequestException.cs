namespace UniTrackRemaster.Data.Exceptions;

public class BadRequestException : BusinessException
{
    public BadRequestException(string message) : base(message) { }

    public BadRequestException() : base("The request is invalid.") { }
}
