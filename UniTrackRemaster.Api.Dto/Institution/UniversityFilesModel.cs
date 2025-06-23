using Microsoft.AspNetCore.Http;

namespace UniTrackRemaster.Api.Dto.Institution;

public record UniversityFilesModel(
    IFormFile? Logo,
    List<IFormFile> AdditionalImages);