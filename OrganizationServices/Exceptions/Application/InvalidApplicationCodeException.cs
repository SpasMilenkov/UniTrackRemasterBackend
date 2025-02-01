namespace OrganizationServices.Exceptions.Application;

public class InvalidApplicationCodeException(string code) : Exception($"Invalid application code: {code}");
