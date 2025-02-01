namespace OrganizationServices.Exceptions.Application;

public class ApplicationNotFoundException : Exception
{
    public ApplicationNotFoundException(Guid id) 
        : base($"Application with ID {id} was not found.") { }
    
    public ApplicationNotFoundException(string email) 
        : base($"Application with email {email} was not found.") { }
}