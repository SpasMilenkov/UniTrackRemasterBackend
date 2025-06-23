using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum ReactionType
{
    [Display(Name = "Like")]
    Like,
    [Display(Name = "Love")]
    Love,
    [Display(Name = "Laugh")]
    Laugh,
    [Display(Name = "Wow")]
    Wow,
    [Display(Name = "Sad")]
    Sad,
    [Display(Name = "Angry")]
    Angry,
    [Display(Name = "Thumbs Up")]
    ThumbsUp,
    [Display(Name = "Thumbs Down")]
    ThumbsDown,
    [Display(Name = "Heart")]
    Heart,
    [Display(Name = "Fire")]
    Fire
}

