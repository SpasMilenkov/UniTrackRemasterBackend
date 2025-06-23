namespace UniTrackRemaster.Services.User.Parents.Exceptions;

/// <summary>
/// Exception thrown when a parent is not found
/// </summary>
public class ParentNotFoundException : ParentException
{
    public string Identifier { get; }

    public ParentNotFoundException(string identifier) 
        : base($"Parent not found: {identifier}")
    {
        Identifier = identifier;
    }

    public ParentNotFoundException(Guid id) 
        : base($"Parent not found with ID: {id}")
    {
        Identifier = id.ToString();
    }
}