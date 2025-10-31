namespace Foodbook.Business.Models
{
    public class ChefJudgeResult
    {
        public string RecipeName { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new List<string>();
        public string OverallRating { get; set; } = string.Empty;
        public int PresentationScore { get; set; }
        public int ColorScore { get; set; }
        public int TextureScore { get; set; }
        public int PlatingScore { get; set; }
        public string HealthNotes { get; set; } = string.Empty;
        public List<string> ChefTips { get; set; } = new List<string>();
    }
}
