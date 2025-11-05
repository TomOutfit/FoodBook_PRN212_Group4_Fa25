using System.Windows;
using System.Text.RegularExpressions;

namespace Foodbook.Presentation.Views
{
    public partial class JudgeResultDialog : Window
    {
        public JudgeResultDialog()
        {
            InitializeComponent();
            ShowAnalyzingState();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowAnalyzingState()
        {
            ScoreText.Text = "--/10";
            RatingText.Text = "Analyzing...";
            CommentText.Text = "AI is analyzing your dish...";
            PresentationScoreText.Text = "--/10";
            ColorScoreText.Text = "--/10";
            TextureScoreText.Text = "--/10";
            PlatingScoreText.Text = "--/10";
            HealthNotesText.Text = "Health analysis in progress...";
            ChefTipsText.Text = "Chef tips will be generated based on AI analysis...";
            SuggestionsText.Text = "AI suggestions will appear here after analysis...";
        }

        public void SetJudgeResult(double score, string overallRating, string comment, string cookingMethods, string flavors, string ingredients, string suggestions)
        {
            ScoreText.Text = $"{score:0.0}/10";
            RatingText.Text = overallRating;
            CommentText.Text = NormalizeComment(comment);
            SuggestionsText.Text = ToBulletedList(suggestions);
        }

        public void SetJudgeResult(double score, string overallRating, string comment, double presentationScore, double colorScore, double textureScore, double platingScore, string healthNotes, string chefTips, string suggestions)
        {
            ScoreText.Text = $"{score:0.0}/10";
            RatingText.Text = overallRating;
            CommentText.Text = NormalizeComment(comment);
            PresentationScoreText.Text = $"{presentationScore:0.0}/10";
            ColorScoreText.Text = $"{colorScore:0.0}/10";
            TextureScoreText.Text = $"{textureScore:0.0}/10";
            PlatingScoreText.Text = $"{platingScore:0.0}/10";
            HealthNotesText.Text = ToBulletedList(healthNotes);
            ChefTipsText.Text = ToBulletedList(chefTips);
            SuggestionsText.Text = ToBulletedList(suggestions);
        }

        private static string NormalizeComment(string text)
        {
            return FormatAsParagraphs(text);
        }

        private static string NormalizeParagraphs(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
            // Ensure sentences are separated properly if AI returns bullet-like separators
            normalized = normalized.Replace(" • ", "\n• ");
            return normalized.Trim();
        }

        private static string FormatAsParagraphs(string text)
        {
            var normalized = NormalizeParagraphs(text);
            // If there's no line break at all, insert breaks after sentence-ending punctuation
            if (!normalized.Contains("\n"))
            {
                normalized = Regex.Replace(normalized, @"(?<=[\.\!\?])\s+", "\n");
            }
            // Collapse multiple blank lines
            normalized = Regex.Replace(normalized, "\n{3,}", "\n\n");
            return normalized.Trim();
        }

        private static string ToBulletedList(string text)
        {
            var normalized = NormalizeParagraphs(text);
            IEnumerable<string> lines;
            if (!normalized.Contains("\n"))
            {
                // No explicit breaks: split into sentences for bullets
                lines = Regex.Split(normalized, @"(?<=[\.\!\?])\s+")
                    .Select(l => l.Trim());
            }
            else
            {
                lines = normalized.Split('\n').Select(l => l.Trim());
            }
            lines = lines.Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.StartsWith("•") || l.StartsWith("-") ? l : $"• {l}");
            return string.Join("\n", lines);
        }
    }
}
