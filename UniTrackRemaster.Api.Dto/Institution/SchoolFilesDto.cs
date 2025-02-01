using Microsoft.AspNetCore.Http;

namespace UniTrackRemaster.Api.Dto.Request;

public record SchoolFilesModel(
    IFormFile? Logo,
    List<IFormFile> AdditionalImages);
