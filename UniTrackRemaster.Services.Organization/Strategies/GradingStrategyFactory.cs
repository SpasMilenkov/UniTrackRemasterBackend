using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Services.Organization.Strategies;

/// <summary>
/// Factory for creating appropriate grading strategy based on type
/// </summary>
public class GradingStrategyFactory
{
    private readonly Dictionary<GradingSystemType, IGradingStrategy> _strategies;
    private readonly IServiceProvider _serviceProvider;

    public GradingStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _strategies = new Dictionary<GradingSystemType, IGradingStrategy>
            {
                { GradingSystemType.American, new AmericanGradingStrategy() },
                { GradingSystemType.European, new EuropeanGradingStrategy() },
                { GradingSystemType.Bulgarian, new BulgarianGradingStrategy() }
            };
    }

    public IGradingStrategy GetStrategy(GradingSystemType type)
    {
        if (_strategies.TryGetValue(type, out var strategy))
        {
            return strategy;
        }

        if (type == GradingSystemType.Custom)
        {
            // Resolve custom strategy from DI container
            return (IGradingStrategy)_serviceProvider.GetService(typeof(CustomGradingStrategy));
        }

        throw new NotSupportedException($"Grading system type '{type}' is not supported");
    }

    public IGradingStrategy GetStrategy(GradingSystem gradingSystem)
    {
        if (gradingSystem.Type != GradingSystemType.Custom)
        {
            return GetStrategy(gradingSystem.Type);
        }

        // For custom grading systems, create a strategy with the provided grade scales
        return new CustomGradingStrategy(gradingSystem);
    }
}
/// <summary>
/// American grading system (A to F with +/- on 4.0 scale)
/// </summary>
public class AmericanGradingStrategy : IGradingStrategy
{
    public string ConvertScoreToGrade(decimal score)
    {
        if (score >= 97) return "A+";
        if (score >= 93) return "A";
        if (score >= 90) return "A-";
        if (score >= 87) return "B+";
        if (score >= 83) return "B";
        if (score >= 80) return "B-";
        if (score >= 77) return "C+";
        if (score >= 73) return "C";
        if (score >= 70) return "C-";
        if (score >= 67) return "D+";
        if (score >= 63) return "D";
        if (score >= 60) return "D-";
        return "F";
    }

    public double ConvertScoreToGpaPoints(decimal score)
    {
        if (score >= 97) return 4.0;
        if (score >= 93) return 4.0;
        if (score >= 90) return 3.7;
        if (score >= 87) return 3.3;
        if (score >= 83) return 3.0;
        if (score >= 80) return 2.7;
        if (score >= 77) return 2.3;
        if (score >= 73) return 2.0;
        if (score >= 70) return 1.7;
        if (score >= 67) return 1.3;
        if (score >= 63) return 1.0;
        if (score >= 60) return 0.7;
        return 0.0;
    }

    public string DetermineStatus(decimal score, decimal passingScore)
    {
        return score >= passingScore ? "Completed" : "Failed";
    }

    public decimal ConvertGradeToScore(string grade)
    {
        return grade switch
        {
            "A+" => 97,
            "A" => 95,
            "A-" => 90,
            "B+" => 87,
            "B" => 85,
            "B-" => 80,
            "C+" => 77,
            "C" => 75,
            "C-" => 70,
            "D+" => 67,
            "D" => 65,
            "D-" => 60,
            "F" => 55,
            _ => 0
        };
    }

    public GradingSystemType GetSystemType() => GradingSystemType.American;

    public GradingSystem CreateDefaultGradingSystem(Guid institutionId)
    {
        var system = new GradingSystem
        {
            Id = Guid.NewGuid(),
            Name = "American Grading System",
            Description = "Standard American grading system with letter grades on a 4.0 scale",
            Type = GradingSystemType.American,
            IsDefault = true,
            MinimumPassingScore = 60,
            MaximumScore = 100,
            InstitutionId = institutionId
        };

        system.GradeScales = new List<GradeScale>
            {
                new GradeScale { Id = Guid.NewGuid(), Grade = "A+", Description = "Outstanding", MinimumScore = 97, MaximumScore = 100, GpaValue = 4.0, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "A", Description = "Excellent", MinimumScore = 93, MaximumScore = 96.99m, GpaValue = 4.0, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "A-", Description = "Very Good", MinimumScore = 90, MaximumScore = 92.99m, GpaValue = 3.7, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "B+", Description = "Good Plus", MinimumScore = 87, MaximumScore = 89.99m, GpaValue = 3.3, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "B", Description = "Good", MinimumScore = 83, MaximumScore = 86.99m, GpaValue = 3.0, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "B-", Description = "Good Minus", MinimumScore = 80, MaximumScore = 82.99m, GpaValue = 2.7, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "C+", Description = "Satisfactory Plus", MinimumScore = 77, MaximumScore = 79.99m, GpaValue = 2.3, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "C", Description = "Satisfactory", MinimumScore = 73, MaximumScore = 76.99m, GpaValue = 2.0, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "C-", Description = "Satisfactory Minus", MinimumScore = 70, MaximumScore = 72.99m, GpaValue = 1.7, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "D+", Description = "Poor Plus", MinimumScore = 67, MaximumScore = 69.99m, GpaValue = 1.3, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "D", Description = "Poor", MinimumScore = 63, MaximumScore = 66.99m, GpaValue = 1.0, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "D-", Description = "Poor Minus", MinimumScore = 60, MaximumScore = 62.99m, GpaValue = 0.7, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "F", Description = "Failing", MinimumScore = 0, MaximumScore = 59.99m, GpaValue = 0.0, GradingSystemId = system.Id }
            };

        return system;
    }
}

/// <summary>
/// European grading system (ECTS)
/// </summary>
public class EuropeanGradingStrategy : IGradingStrategy
{
    public string ConvertScoreToGrade(decimal score)
    {
        if (score >= 90) return "A";
        if (score >= 80) return "B";
        if (score >= 70) return "C";
        if (score >= 60) return "D";
        if (score >= 50) return "E";
        if (score >= 40) return "FX";
        return "F";
    }

    public double ConvertScoreToGpaPoints(decimal score)
    {
        if (score >= 90) return 4.0;
        if (score >= 80) return 3.5;
        if (score >= 70) return 3.0;
        if (score >= 60) return 2.5;
        if (score >= 50) return 2.0;
        if (score >= 40) return 1.0;
        return 0.0;
    }

    public string DetermineStatus(decimal score, decimal passingScore)
    {
        return score >= passingScore ? "Completed" : "Failed";
    }

    public decimal ConvertGradeToScore(string grade)
    {
        return grade switch
        {
            "A" => 95,
            "B" => 85,
            "C" => 75,
            "D" => 65,
            "E" => 55,
            "FX" => 45,
            "F" => 30,
            _ => 0
        };
    }

    public GradingSystemType GetSystemType() => GradingSystemType.European;

    public GradingSystem CreateDefaultGradingSystem(Guid institutionId)
    {
        var system = new GradingSystem
        {
            Id = Guid.NewGuid(),
            Name = "European Credit Transfer System (ECTS)",
            Description = "Standard European grading system using A to F scale",
            Type = GradingSystemType.European,
            IsDefault = false,
            MinimumPassingScore = 50,
            MaximumScore = 100,
            InstitutionId = institutionId
        };

        system.GradeScales = new List<GradeScale>
            {
                new GradeScale { Id = Guid.NewGuid(), Grade = "A", Description = "EXCELLENT - outstanding performance with only minor errors", MinimumScore = 90, MaximumScore = 100, GpaValue = 4.0, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "B", Description = "VERY GOOD - above the average standard but with some errors", MinimumScore = 80, MaximumScore = 89.99m, GpaValue = 3.5, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "C", Description = "GOOD - generally sound work with a number of notable errors", MinimumScore = 70, MaximumScore = 79.99m, GpaValue = 3.0, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "D", Description = "SATISFACTORY - fair but with significant shortcomings", MinimumScore = 60, MaximumScore = 69.99m, GpaValue = 2.5, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "E", Description = "SUFFICIENT - performance meets the minimum criteria", MinimumScore = 50, MaximumScore = 59.99m, GpaValue = 2.0, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "FX", Description = "FAIL - some more work required before the credit can be awarded", MinimumScore = 40, MaximumScore = 49.99m, GpaValue = 1.0, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "F", Description = "FAIL - considerable further work is required", MinimumScore = 0, MaximumScore = 39.99m, GpaValue = 0.0, GradingSystemId = system.Id }
            };

        return system;
    }
}

/// <summary>
/// Bulgarian grading system (2-6 scale)
/// </summary>
public class BulgarianGradingStrategy : IGradingStrategy
{
    public string ConvertScoreToGrade(decimal score)
    {
        if (score >= 92) return "6.00";
        if (score >= 88) return "5.75";
        if (score >= 83) return "5.50";
        if (score >= 78) return "5.25";
        if (score >= 73) return "5.00";
        if (score >= 68) return "4.75";
        if (score >= 63) return "4.50";
        if (score >= 58) return "4.25";
        if (score >= 53) return "4.00";
        if (score >= 48) return "3.75";
        if (score >= 43) return "3.50";
        if (score >= 38) return "3.25";
        if (score >= 30) return "3.00";
        if (score >= 25) return "2.50";
        return "2.00";
    }

    public double ConvertScoreToGpaPoints(decimal score)
    {
        // Convert Bulgarian 2-6 scale to GPA equivalent (approximate)
        var bulgarianGrade = ConvertScoreToGrade(score);
        return bulgarianGrade switch
        {
            "6.00" => 4.0,
            "5.75" => 3.8,
            "5.50" => 3.7,
            "5.25" => 3.5,
            "5.00" => 3.3,
            "4.75" => 3.0,
            "4.50" => 2.7,
            "4.25" => 2.3,
            "4.00" => 2.0,
            "3.75" => 1.7,
            "3.50" => 1.3,
            "3.25" => 1.0,
            "3.00" => 0.7,
            _ => 0.0
        };
    }

    public string DetermineStatus(decimal score, decimal passingScore)
    {
        // In Bulgarian system, 3.00 (30%) is passing
        return score >= passingScore ? "Completed" : "Failed";
    }

    public decimal ConvertGradeToScore(string grade)
    {
        return grade switch
        {
            "6.00" => 95,
            "5.75" => 90,
            "5.50" => 85,
            "5.25" => 80,
            "5.00" => 75,
            "4.75" => 70,
            "4.50" => 65,
            "4.25" => 60,
            "4.00" => 55,
            "3.75" => 50,
            "3.50" => 45,
            "3.25" => 40,
            "3.00" => 35,
            "2.50" => 25,
            "2.00" => 15,
            _ => 0
        };
    }

    public GradingSystemType GetSystemType() => GradingSystemType.Bulgarian;

    public GradingSystem CreateDefaultGradingSystem(Guid institutionId)
    {
        var system = new GradingSystem
        {
            Id = Guid.NewGuid(),
            Name = "Bulgarian Grading System",
            Description = "Bulgarian 2-6 scale grading system",
            Type = GradingSystemType.Bulgarian,
            IsDefault = false,
            MinimumPassingScore = 30, // 3.00 is passing
            MaximumScore = 100,
            InstitutionId = institutionId
        };

        system.GradeScales = new List<GradeScale>
            {
                new GradeScale { Id = Guid.NewGuid(), Grade = "6.00", Description = "Excellent", MinimumScore = 92, MaximumScore = 100, GpaValue = 4.0, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "5.75", Description = "Very Good Plus", MinimumScore = 88, MaximumScore = 91.99m, GpaValue = 3.8, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "5.50", Description = "Very Good", MinimumScore = 83, MaximumScore = 87.99m, GpaValue = 3.7, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "5.25", Description = "Very Good Minus", MinimumScore = 78, MaximumScore = 82.99m, GpaValue = 3.5, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "5.00", Description = "Good Plus", MinimumScore = 73, MaximumScore = 77.99m, GpaValue = 3.3, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "4.75", Description = "Good", MinimumScore = 68, MaximumScore = 72.99m, GpaValue = 3.0, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "4.50", Description = "Good Minus", MinimumScore = 63, MaximumScore = 67.99m, GpaValue = 2.7, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "4.25", Description = "Average Plus", MinimumScore = 58, MaximumScore = 62.99m, GpaValue = 2.3, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "4.00", Description = "Average", MinimumScore = 53, MaximumScore = 57.99m, GpaValue = 2.0, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "3.75", Description = "Average Minus", MinimumScore = 48, MaximumScore = 52.99m, GpaValue = 1.7, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "3.50", Description = "Poor Plus", MinimumScore = 43, MaximumScore = 47.99m, GpaValue = 1.3, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "3.25", Description = "Poor", MinimumScore = 38, MaximumScore = 42.99m, GpaValue = 1.0, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "3.00", Description = "Poor Minus", MinimumScore = 30, MaximumScore = 37.99m, GpaValue = 0.7, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "2.50", Description = "Fail Plus", MinimumScore = 25, MaximumScore = 29.99m, GpaValue = 0.3, GradingSystemId = system.Id },
                new GradeScale { Id = Guid.NewGuid(), Grade = "2.00", Description = "Fail", MinimumScore = 0, MaximumScore = 24.99m, GpaValue = 0.0, GradingSystemId = system.Id }
            };

        return system;
    }
}

/// <summary>
/// Custom grading system using institutional specific scales
/// </summary>
public class CustomGradingStrategy : IGradingStrategy
{
    private readonly GradingSystem _gradingSystem;

    public CustomGradingStrategy(GradingSystem gradingSystem)
    {
        _gradingSystem = gradingSystem;
    }

    // Default constructor for DI
    public CustomGradingStrategy()
    {
        _gradingSystem = null;
    }

    public string ConvertScoreToGrade(decimal score)
    {
        if (_gradingSystem == null || !_gradingSystem.GradeScales.Any())
        {
            throw new InvalidOperationException("Custom grading system requires grade scales");
        }

        foreach (var scale in _gradingSystem.GradeScales.OrderByDescending(s => s.MinimumScore))
        {
            if (score >= scale.MinimumScore && score <= scale.MaximumScore)
            {
                return scale.Grade;
            }
        }

        // Default to lowest grade if no match
        return _gradingSystem.GradeScales.OrderBy(s => s.MinimumScore).First().Grade;
    }

    public double ConvertScoreToGpaPoints(decimal score)
    {
        if (_gradingSystem == null || !_gradingSystem.GradeScales.Any())
        {
            throw new InvalidOperationException("Custom grading system requires grade scales");
        }

        foreach (var scale in _gradingSystem.GradeScales.OrderByDescending(s => s.MinimumScore))
        {
            if (score >= scale.MinimumScore && score <= scale.MaximumScore)
            {
                return scale.GpaValue;
            }
        }

        // Default to lowest GPA if no match
        return _gradingSystem.GradeScales.OrderBy(s => s.MinimumScore).First().GpaValue;
    }

    public string DetermineStatus(decimal score, decimal passingScore)
    {
        return score >= passingScore ? "Completed" : "Failed";
    }

    public decimal ConvertGradeToScore(string grade)
    {
        if (_gradingSystem == null || !_gradingSystem.GradeScales.Any())
        {
            throw new InvalidOperationException("Custom grading system requires grade scales");
        }

        var scale = _gradingSystem.GradeScales.FirstOrDefault(s => s.Grade == grade);
        if (scale == null)
        {
            return 0;
        }

        // Return middle of range
        return (scale.MinimumScore + scale.MaximumScore) / 2;
    }

    public GradingSystemType GetSystemType() => GradingSystemType.Custom;

    public GradingSystem CreateDefaultGradingSystem(Guid institutionId)
    {
        var system = new GradingSystem
        {
            Id = Guid.NewGuid(),
            Name = "Custom Grading System",
            Description = "Custom institution-specific grading system",
            Type = GradingSystemType.Custom,
            IsDefault = false,
            MinimumPassingScore = 60,
            MaximumScore = 100,
            InstitutionId = institutionId
        };

        // Custom systems need to be populated with institution-specific scales
        system.GradeScales = new List<GradeScale>();

        return system;
    }
}