namespace UniTrackRemaster.Services.Organization.Exceptions.School;

public class SchoolNotFoundException(Guid applicationId)
    : Exception($"School for application {applicationId} was not found.");