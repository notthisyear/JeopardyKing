using CommunityToolkit.Mvvm.ComponentModel;
using JeopardyKing.Common;

namespace JeopardyKing.GameComponents
{
    public enum QuestionType
    {
        Text,
        Image,
        Audio,
        Video
    }

    public class Question : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private bool _isAnswered = false;
        private bool _isBeingEdited = false;
        private string _categoryName = string.Empty;
        private decimal _value;
        private QuestionType _type;
        private CurrencyType _currency;
        private bool _isBonus;

        private bool _hasMediaLink = false;
        private bool _isEmbeddedMedia;
        private string _content = string.Empty;
        private string _multimediaContentLink = string.Empty;
        private string _youtubeVideoId = string.Empty;
        #endregion

        public int Id { get; }

        public int CategoryId { get; }

        public bool IsAnswered
        {
            get => _isAnswered;
            private set => SetProperty(ref _isAnswered, value);
        }

        public bool IsBeingEdited
        {
            get => _isBeingEdited;
            set => SetProperty(ref _isBeingEdited, value);
        }

        public string CategoryName
        {
            get => _categoryName;
            set => SetProperty(ref _categoryName, value);
        }

        public decimal Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public QuestionType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public CurrencyType Currency
        {
            get => _currency;
            set => SetProperty(ref _currency, value);
        }

        public bool IsBonus
        {
            get => _isBonus;
            set => SetProperty(ref _isBonus, value);
        }

        public bool HasMediaLink
        {
            get => _hasMediaLink;
            private set => SetProperty(ref _hasMediaLink, value);
        }

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public bool IsEmbeddedMedia
        {
            get => _isEmbeddedMedia;
            private set => SetProperty(ref _isEmbeddedMedia, value);
        }

        public string MultimediaContentLink
        {
            get => _multimediaContentLink;
            private set => SetProperty(ref _multimediaContentLink, value);
        }

        public string YoutubeVideoId
        {
            get => _youtubeVideoId;
            private set => SetProperty(ref _youtubeVideoId, value);
        }

        public string OriginalYoutubeUrl { get; private set; } = string.Empty;
        #endregion

        #region Private fields
        private const string YoutubeEmbeddedRootUrl = "https://www.youtube.com/embed";
        private int _startVideoAtSeconds = 0;
        #endregion

        public Question(int id, int categoryId, string categoryName, QuestionType type, decimal value, CurrencyType currency)
        {
            Id = id;
            CategoryId = categoryId;
            CategoryName = categoryName;
            Type = type;
            Value = value;
            Currency = currency;
            IsBonus = false;
            Content = string.Empty;
        }

        #region Public methods
        public (int minutes, int seconds) GetCurrentStartAtForVideo()
        {
            var minutes = _startVideoAtSeconds / 60;
            return (minutes, _startVideoAtSeconds - (minutes * 60));
        }

        public void SetYoutubeVideoParameters(string originalUrl, string youtubeVideoId, bool autoplay, bool showControls)
        {
            YoutubeVideoId = youtubeVideoId;
            OriginalYoutubeUrl = originalUrl;
            MultimediaContentLink = GetYoutubeVideoUrl(youtubeVideoId, autoplay, showControls, 0);
            HasMediaLink = true;
            IsEmbeddedMedia = false;
        }

        public void SetStartAtForCurrentVideo(int minutes, int seconds)
        {
            if (Type != QuestionType.Video || minutes < 0 || seconds < 0)
                return;

            _startVideoAtSeconds = minutes * 60 + seconds;
        }

        public void RefreshYoutubeVideoUrl(bool autoplay, bool showControls)
        {
            if (Type != QuestionType.Video || string.IsNullOrEmpty(YoutubeVideoId))
                return;

            MultimediaContentLink = GetYoutubeVideoUrl(YoutubeVideoId, autoplay, showControls, _startVideoAtSeconds);
        }

        #endregion

        private static string GetYoutubeVideoUrl(string videoId, bool autoplay, bool showControls, int startAtSeconds)
            => $"{YoutubeEmbeddedRootUrl}/{videoId}?" +
            $"autoplay={GetValueForBooleanInLink(autoplay)}" +
            $"&amp;controls={GetValueForBooleanInLink(showControls)}" +
            $"{(startAtSeconds > 0 ? $"&amp;start={startAtSeconds}" : string.Empty)}";

        private static string GetValueForBooleanInLink(bool b)
            => b ? "1" : "0";
    }
}
