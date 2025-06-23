namespace UniTrackRemaster.Api.Dto.Student.Analytics;

public record TeacherCommentDto(
    Guid Id,
    string Subject,
    string Teacher,
    string Content,
    DateTime Date
);
