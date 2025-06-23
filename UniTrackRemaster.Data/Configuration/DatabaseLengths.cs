namespace UniTrackRemaster.Data.Configuration;

public static class DatabaseLengths
{
    // Identity & Names
    public const int PersonName = 50;           // FirstName, LastName
    public const int EntityName = 100;         // Institution name, Subject name, etc.
    public const int Title = 200;              // Event title, document title
    
    // Codes & Identifiers  
    public const int Code = 20;                // Subject code, institution code
    public const int Phone = 20;              // Phone numbers
    public const int Email = 320;             // RFC 5321 standard
    
    // Descriptions
    public const int ShortDescription = 500;   // Brief descriptions
    public const int LongDescription = 2000;  // Detailed descriptions
    public const int Notes = 1000;            // General notes
    
    // URLs & Links
    public const int Url = 2048;              // URLs, meeting links
    public const int FilePath = 500;          // File paths, attachment URLs
    
    // Text Content
    public const int MessageContent = 4000;   // Chat messages, notifications
    public const int Reason = 250;            // Absence reason, etc.
    
    // Location & Address
    public const int Address = 200;           // Street, city, etc.
    public const int Location = 200;          // Event location
}