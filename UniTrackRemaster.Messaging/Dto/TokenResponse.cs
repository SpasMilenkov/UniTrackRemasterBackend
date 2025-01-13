using System.Text.Json.Serialization;

namespace UniTrackRemaster.Messaging.Dto;

// Class to deserialize the token response from the oauth process
// Needed for authenticating with Google
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
}