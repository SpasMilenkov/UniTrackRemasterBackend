using System.ComponentModel.DataAnnotations;

namespace UniTrackReimagined.Data.Models.Organizations;

public class University
{
    #region Properties

        public required Guid UniversityId { get; set; }
        [Length(0, 100)]
        public required string Name { get; set; }
        [Length(0, 255)]
        public string? Description { get; set; }

    #endregion
}