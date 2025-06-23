
using Microsoft.AspNetCore.Http;

namespace UniTrackRemaster.Api.Dto.Institution;

public record SchoolFilesModel(
    IFormFile? Logo,
    List<IFormFile> AdditionalImages);
