using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using JeopardyKing.Common;

namespace JeopardyKing.GameComponents
{
    public enum QuestionType
    {
        Text,
        Image,
        Audio,
        Video,
        YoutubeVideo
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
        private string _mediaName = string.Empty;
        private int _videoOrAudioLengthSeconds = 0;
        private int _startVideoOrAudioAtSeconds = 0;
        private int _endVideoOrAudioAtSeconds = 0;
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
            set
            {
                if (value != _type)
                {
                    if (HasMediaLink)
                        ResetAllMediaParameters();
                    Content = string.Empty;
                }
                SetProperty(ref _type, value);
            }
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

        public string MediaName
        {
            get => _mediaName;
            private set => SetProperty(ref _mediaName, value);
        }

        public int VideoOrAudioLengthSeconds
        {
            get => _videoOrAudioLengthSeconds;
            set => SetProperty(ref _videoOrAudioLengthSeconds, value);
        }

        public int StartVideoOrAudioAtSeconds
        {
            get => _startVideoOrAudioAtSeconds;
            set => SetProperty(ref _startVideoOrAudioAtSeconds, value);
        }

        public int EndVideoOrAudioAtSeconds
        {
            get => _endVideoOrAudioAtSeconds;
            set => SetProperty(ref _endVideoOrAudioAtSeconds, value);
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
        public void SetMultimediaParameters(string pathToMedia)
        {
            MultimediaContentLink = pathToMedia;
            MediaName = Path.GetFileName(pathToMedia);
            HasMediaLink = true;
        }

        public void SetYoutubeVideoParameters(string originalUrl, string youtubeVideoId, bool autoplay, bool showControls)
        {
            if (youtubeVideoId.Equals(YoutubeVideoId, StringComparison.Ordinal))
                return;

            YoutubeVideoId = youtubeVideoId;
            OriginalYoutubeUrl = originalUrl;
            MultimediaContentLink = GetYoutubeVideoUrl(youtubeVideoId, autoplay, showControls, 0);
            HasMediaLink = true;
            StartVideoOrAudioAtSeconds = 0;
        }
        public void RefreshYoutubeVideoUrl(bool autoplay, bool showControls)
        {
            if (Type != QuestionType.Video || string.IsNullOrEmpty(YoutubeVideoId))
                return;

            MultimediaContentLink = GetYoutubeVideoUrl(YoutubeVideoId, autoplay, showControls, StartVideoOrAudioAtSeconds);
        }

        #endregion

        #region Private methods
        private void ResetAllMediaParameters()
        {
            HasMediaLink = false;
            MediaName = string.Empty;
            IsEmbeddedMedia = false;
            MultimediaContentLink = string.Empty;
            YoutubeVideoId = string.Empty;
            OriginalYoutubeUrl = string.Empty;
        }

        private static string GetYoutubeVideoUrl(string videoId, bool autoplay, bool showControls, int startAtSeconds)
            => $"{YoutubeEmbeddedRootUrl}/{videoId}?" +
            $"autoplay={GetValueForBooleanInLink(autoplay)}" +
            $"&amp;controls={GetValueForBooleanInLink(showControls)}" +
            $"{(startAtSeconds > 0 ? $"&amp;start={startAtSeconds}" : string.Empty)}";

        private static string GetValueForBooleanInLink(bool b)
            => b ? "1" : "0";
        #endregion

    }
}
