using Microsoft.AspNetCore.Http;

namespace UniTrackRemaster.Api.Dto.Request;

public record UniversityFilesModel(
    IFormFile? Logo,
    List<IFormFile> AdditionalImages);