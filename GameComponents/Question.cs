using System;
using System.Drawing;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JeopardyKing.GameComponents
{
    public class Question : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private bool _isAnswered = false;
        private bool _isSelected = false;
        private string _categoryName = string.Empty;
        private decimal _value = 0M;
        private QuestionType _type;
        private CurrencyType _currency = CurrencyType.SwedishKrona;
        private MediaQuestionFlow _mediaQuestionFlow = MediaQuestionFlow.None;
        private bool _isBonus = false;
        private bool _isGamble = false;

        private bool _hasMediaLink = false;
        private string _mediaName = string.Empty;
        private int _videoOrAudioLengthSeconds = 0;
        private double _startVideoOrAudioAtSeconds = 0.0;
        private double _endVideoOrAudioAtSeconds = 0.0;
        private int _imageOrVideoWidth = 0;
        private int _imageOrVideoHeight = 0;
        private bool _imageOrVideoTallerThanWide = false;
        private bool _isEmbeddedMedia = false;
        private string _content = string.Empty;
        private string _multimediaContentLink = string.Empty;
        private string _youtubeVideoId = string.Empty;
        #endregion

        public int Id { get; }

        [JsonIgnore]
        public int CategoryId { get; set; }

        [JsonIgnore]
        public bool IsAnswered
        {
            get => _isAnswered;
            set => SetProperty(ref _isAnswered, value);
        }

        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        [JsonIgnore]
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

        [JsonConverter(typeof(StringEnumConverter))]
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

        [JsonConverter(typeof(StringEnumConverter))]
        public MediaQuestionFlow MediaQuestionFlow
        {
            get => _mediaQuestionFlow;
            set => SetProperty(ref _mediaQuestionFlow, value);
        }

        [JsonIgnore]
        public CurrencyType Currency
        {
            get => _currency;
            set => SetProperty(ref _currency, value);
        }

        public bool IsBonus
        {
            get => _isBonus;
            set
            {
                if (value && IsGamble)
                    IsGamble = false;
                SetProperty(ref _isBonus, value);
            }
        }

        public bool IsGamble
        {
            get => _isGamble;
            set
            {
                if (value && IsBonus)
                    IsBonus = false;
                SetProperty(ref _isGamble, value);
            }
        }

        public bool HasMediaLink
        {
            get => _hasMediaLink;
            set => SetProperty(ref _hasMediaLink, value);
        }

        public string MediaName
        {
            get => _mediaName;
            set => SetProperty(ref _mediaName, value);
        }

        public int VideoOrAudioLengthSeconds
        {
            get => _videoOrAudioLengthSeconds;
            set => SetProperty(ref _videoOrAudioLengthSeconds, value);
        }

        public double StartVideoOrAudioAtSeconds
        {
            get => _startVideoOrAudioAtSeconds;
            set => SetProperty(ref _startVideoOrAudioAtSeconds, value);
        }

        public double EndVideoOrAudioAtSeconds
        {
            get => _endVideoOrAudioAtSeconds;
            set => SetProperty(ref _endVideoOrAudioAtSeconds, value);
        }

        public int ImageOrVideoWidth
        {
            get => _imageOrVideoWidth;
            set => SetProperty(ref _imageOrVideoWidth, value);
        }

        public int ImageOrVideoHeight
        {
            get => _imageOrVideoHeight;
            set => SetProperty(ref _imageOrVideoHeight, value);
        }

        public bool ImageOrVideoTallerThanWide
        {
            get => _imageOrVideoTallerThanWide;
            set => SetProperty(ref _imageOrVideoTallerThanWide, value);
        }

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public bool IsEmbeddedMedia
        {
            get => _isEmbeddedMedia;
            set => SetProperty(ref _isEmbeddedMedia, value);
        }

        public string MultimediaContentLink
        {
            get => _multimediaContentLink;
            set => SetProperty(ref _multimediaContentLink, value);
        }

        public string YoutubeVideoId
        {
            get => _youtubeVideoId;
            set => SetProperty(ref _youtubeVideoId, value);
        }

        public string OriginalYoutubeUrl { get; set; } = string.Empty;
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
        }

        #region Public methods
        public void SetMultimediaParameters(string pathToMedia)
        {
            StartVideoOrAudioAtSeconds = 0;
            MultimediaContentLink = pathToMedia;
            MediaName = Path.GetFileName(pathToMedia);
            HasMediaLink = true;

            if (Type == QuestionType.Image)
            {
                try
                {
                    using var img = Image.FromFile(pathToMedia);
                    SetImageOrVideoWidthAndHeight(img.Width, img.Height);
                }
                catch (Exception e) when (e is OutOfMemoryException)
                { }
            }
        }

        public void SetImageOrVideoWidthAndHeight(int width, int height)
        {
            ImageOrVideoWidth = width;
            ImageOrVideoHeight = height;
            ImageOrVideoTallerThanWide = ImageOrVideoHeight > ImageOrVideoWidth;
        }

        public void SetYoutubeVideoParameters(string originalUrl, string youtubeVideoId, bool autoplay, bool showControls)
        {
            if (youtubeVideoId.Equals(YoutubeVideoId, StringComparison.Ordinal))
                return;

            YoutubeVideoId = youtubeVideoId;
            OriginalYoutubeUrl = originalUrl;
            MultimediaContentLink = GetYoutubeVideoUrl(youtubeVideoId, autoplay, showControls, 0, 0);
            HasMediaLink = true;
            StartVideoOrAudioAtSeconds = 0;
            EndVideoOrAudioAtSeconds = 0;
        }

        public void RefreshYoutubeVideoUrl(bool autoplay, bool showControls)
        {
            if (Type != QuestionType.YoutubeVideo || string.IsNullOrEmpty(YoutubeVideoId))
                return;

            MultimediaContentLink = GetYoutubeVideoUrl(YoutubeVideoId, autoplay, showControls, (int)StartVideoOrAudioAtSeconds, (int)EndVideoOrAudioAtSeconds);
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
            StartVideoOrAudioAtSeconds = 0;
            EndVideoOrAudioAtSeconds = 0;
            VideoOrAudioLengthSeconds = 0;
        }

        private static string GetYoutubeVideoUrl(string videoId, bool autoplay, bool showControls, int startAtSeconds, int endAtSeconds)
            => $"{YoutubeEmbeddedRootUrl}/{videoId}?" +
            $"autoplay={GetValueForBooleanInLink(autoplay)}" +
            $"&amp;controls={GetValueForBooleanInLink(showControls)}" +
            $"{(startAtSeconds > 0 ? $"&amp;start={startAtSeconds}" : string.Empty)}" +
            $"{(endAtSeconds > 0 ? $"&amp;end={endAtSeconds}" : string.Empty)}";

        private static string GetValueForBooleanInLink(bool b)
            => b ? "1" : "0";
        #endregion

    }
}
