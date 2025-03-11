using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum InstitutionType
{
    [Display(Name = "Public School")]
    PublicSchool,
    [Display(Name = "Private School")]
    PrivateSchool,
    [Display(Name = "Charter School")]
    CharterSchool,
    [Display(Name = "International School")]
    InternationalSchool,
    [Display(Name = "Public University")]
    PublicUniversity,
    [Display(Name = "Private University")]
    PrivateUniversity,
    [Display(Name = "Community College")]
    CommunityCollege,
    [Display(Name = "Technical College")]
    TechnicalCollege,
    [Display(Name = "Liberal Arts College")]
    LiberalArtsCollege,
    [Display(Name = "Primary School")]
    PrimarySchool,
    [Display(Name = "Secondary School")]
    SecondarySchool,
    [Display(Name = "High School")]
    HighSchool,
    [Display(Name = "Vocational School")]
    VocationalSchool,
    [Display(Name = "Special Education School")]
    SpecialEducationSchool,
    [Display(Name = "Language School")]
    LanguageSchool,
    [Display(Name = "Other")]
    Other
}