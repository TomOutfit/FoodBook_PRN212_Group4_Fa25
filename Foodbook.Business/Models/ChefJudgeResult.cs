namespace Foodbook.Business.Models
{
    public class ChefJudgeResult
    {
        public string RecipeName { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new List<string>();
        public string OverallRating { get; set; } = string.Empty;
        public double PresentationScore { get; set; }
        public double ColorScore { get; set; }
        public double TextureScore { get; set; }
        public double PlatingScore { get; set; }
        public string HealthNotes { get; set; } = string.Empty;
        public List<string> ChefTips { get; set; } = new List<string>();
    }
}
