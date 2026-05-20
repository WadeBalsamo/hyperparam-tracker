using System.ComponentModel.DataAnnotations;

namespace PaperCutDash.Models;

public class Run
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public string Name { get; set; } = "";
    public string? ConfigJsonRaw { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // === PaperCut hyperparameters (mapped from the Python PaperCut class) ===
    public double TargetOutputLenSeconds { get; set; } = 90.0;
    public string PositivePrompt { get; set; } = "";
    public string NegativePrompt { get; set; } = "";
    public string PosFilterPrompt { get; set; } = "";
    public string NegFilterPrompt { get; set; } = "";
    public bool PostFilter { get; set; } = true;
    public bool PreFilter { get; set; } = false;
    public double PreFilterPosWeight { get; set; } = 1;
    public double PreFilterNegWeight { get; set; } = -2;
    public double PostFilterPosWeight { get; set; } = 1;
    public double PostFilterNegWeight { get; set; } = -2;
    public double FilterNPercentIrrelevant { get; set; } = 0.1;
    public double CoherenceWeight { get; set; } = 0.3;
    public double VarianceWeight { get; set; } = 0.5;
    public string Order { get; set; } = "chronological";
    public double MinimumSecsPerCut { get; set; } = 3;
    public double Padding { get; set; } = 7;
    public double EndPadding { get; set; } = 0.5;
    public int MaxGroups { get; set; } = 50;
    public int SplitParts { get; set; } = 1;
    public bool Retime { get; set; } = true;
    public double Pace { get; set; } = 1.2;
    public double CoherenceOverTimeliness { get; set; } = 1.04;
    public double DurationMicroparam { get; set; } = 2.5;
    public double ExtraSentimentWeight { get; set; } = 50;
    public bool PosSentimentOnly { get; set; } = false;
    public double ScoreWeight { get; set; } = 0.5;
    public double CoherenceBuffer { get; set; } = 8;
    public double CohWeight { get; set; } = 0.3;
    public double BetasplitTimepreferenceWeight { get; set; } = 0.1;
    public double ExtraPosPromptWeight { get; set; } = 0;
    public string ModelSize { get; set; } = "medium.en";
    public bool UseGpt { get; set; } = false;
    public bool Autonomous { get; set; } = true;
    public bool RenderEnabled { get; set; } = true;
    public bool BurnInSubtitles { get; set; } = true;
    public bool Quick { get; set; } = false;
    public bool BetaMode { get; set; } = false;
    public double VideoFadeLen { get; set; } = 0.25;
    public double AudioFadeLen { get; set; } = 0.1;
    public string? Nonce { get; set; }

    // Outputs
    public string? EditedSrt { get; set; }
    public string? PacedSrt { get; set; }
    public double? EditedDuration { get; set; }
    public string? ErrorMessage { get; set; }

    public List<Comment> Comments { get; set; } = new();
}

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RunId { get; set; }
    public string Author { get; set; } = "anon";
    public string Body { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}