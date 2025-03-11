using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum PermissionType
{
    [Display(Name = "ManageUsers" )]
    ManageUsers,
    [Display(Name = "ManageStudents" )]
    ManageStudents,
    [Display(Name = "ManageTeachers" )]
    ManageTeachers,
    [Display(Name = "ManageCourses" )]
    ManageCourses,
    [Display(Name = "ManageGrades" )]
    ManageGrades,
    [Display(Name = "ManageAttendance" )]
    ManageAttendance,
    [Display(Name = "ManageReports" )]
    ManageReports,
    [Display(Name = "ManageSettings" )]
    ManageSettings,
    [Display(Name = "ViewAnalytics" )]
    ViewAnalytics,
    [Display(Name = "FullAccess" )]
    FullAccess
}