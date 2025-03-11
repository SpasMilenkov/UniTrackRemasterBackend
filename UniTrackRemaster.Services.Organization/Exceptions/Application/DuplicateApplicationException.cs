namespace UniTrackRemaster.Services.Organization.Exceptions.Application;

public class DuplicateApplicationException(string email)
    : Exception($"An application with email {email} already exists.");