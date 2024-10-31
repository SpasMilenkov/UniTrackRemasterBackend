namespace UniTrackRemaster.Api.Dto.Response;

public record SchoolAddressResponseDto(string Country, string Settlement, string PostalCode, string Street, Guid SchoolId);