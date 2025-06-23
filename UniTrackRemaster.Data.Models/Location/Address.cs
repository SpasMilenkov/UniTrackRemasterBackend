namespace UniTrackRemaster.Data.Models.Location;

public class Address: BaseEntity
{
    public Guid Id { get; set; }
    public string Country { get; set; }
    public string Settlement { get; set; }
    public string PostalCode { get; set; }
    public string Street { get; set; }
}