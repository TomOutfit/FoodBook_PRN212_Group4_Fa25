namespace Foodbook.Business.Interfaces
{
    public interface ILocalizationService
    {
        string CurrentLanguage { get; set; }
        string GetString(string key);
        void ChangeLanguage(string language);
    }
}

