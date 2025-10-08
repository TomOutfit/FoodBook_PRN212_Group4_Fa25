using System.Windows.Input;
using System.Windows.Media.Imaging;
using Foodbook.Business.Interfaces;
using Foodbook.Presentation.Services;

namespace Foodbook.Presentation.ViewModels
{
    public class ImageViewModel : BaseViewModel
    {
        private readonly IAIService _aiService;
        private readonly ImageService _imageService;
        private BitmapImage? _selectedImage;
        private string _imagePath = string.Empty;
        private string _aiResult = string.Empty;
        private bool _isProcessing;

        public ImageViewModel(IAIService aiService, ImageService imageService)
        {
            _aiService = aiService;
            _imageService = imageService;
            
            SelectImageCommand = new RelayCommand(async () => await SelectImageAsync());
            JudgeImageCommand = new RelayCommand(async () => await JudgeImageAsync());
        }

        public BitmapImage? SelectedImage
        {
            get => _selectedImage;
            set => SetProperty(ref _selectedImage, value);
        }

        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        public string AiResult
        {
            get => _aiResult;
            set => SetProperty(ref _aiResult, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public ICommand SelectImageCommand { get; }
        public ICommand JudgeImageCommand { get; }

        private async Task SelectImageAsync()
        {
            try
            {
                var imagePath = await _imageService.SelectAndSaveImageAsync();
                if (!string.IsNullOrEmpty(imagePath))
                {
                    ImagePath = imagePath;
                    SelectedImage = await _imageService.LoadImageAsync(imagePath);
                    AiResult = "Image selected successfully. Click 'Judge Dish' to analyze.";
                }
            }
            catch (Exception ex)
            {
                AiResult = $"Error selecting image: {ex.Message}";
            }
        }

        private async Task JudgeImageAsync()
        {
            if (string.IsNullOrEmpty(ImagePath))
            {
                AiResult = "Please select an image first.";
                return;
            }

            try
            {
                IsProcessing = true;
                AiResult = "AI is analyzing your dish... Please wait.";

                var result = await _aiService.JudgeDishAsync(ImagePath);
                
                AiResult = $"🍽️ AI Chef Judge Results:\n\n" +
                          $"Score: {result.Score}/10\n" +
                          $"Rating: {result.OverallRating}\n\n" +
                          $"Comment: {result.Comment}\n\n" +
                          $"Suggestions:\n" +
                          string.Join("\n", result.Suggestions.Select((s, i) => $"{i + 1}. {s}"));
            }
            catch (Exception ex)
            {
                AiResult = $"Error analyzing image: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}
