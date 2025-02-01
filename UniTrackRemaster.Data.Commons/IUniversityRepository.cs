using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Commons;

public interface IUniversityRepository
{
    public Task<University> InitUniversityAsync(InitUniversityDto initDto);
}