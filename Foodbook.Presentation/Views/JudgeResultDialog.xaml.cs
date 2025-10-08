using System.Windows;

namespace Foodbook.Presentation.Views
{
    public partial class JudgeResultDialog : Window
    {
        public JudgeResultDialog()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void SetJudgeResult(int score, string overallRating, string comment, string cookingMethods, string flavors, string ingredients, string suggestions)
        {
            ScoreText.Text = $"{score}/10";
            RatingText.Text = overallRating;
            CommentText.Text = comment;
            SuggestionsText.Text = suggestions;
        }

        public void SetJudgeResult(int score, string overallRating, string comment, int presentationScore, int colorScore, int textureScore, int platingScore, string healthNotes, string chefTips, string suggestions)
        {
            ScoreText.Text = $"{score}/10";
            RatingText.Text = overallRating;
            CommentText.Text = comment;
            PresentationScoreText.Text = $"{presentationScore}/10";
            ColorScoreText.Text = $"{colorScore}/10";
            TextureScoreText.Text = $"{textureScore}/10";
            PlatingScoreText.Text = $"{platingScore}/10";
            HealthNotesText.Text = healthNotes;
            ChefTipsText.Text = chefTips;
            SuggestionsText.Text = suggestions;
        }
    }
}
