using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeopardyKing.Common;
using JeopardyKing.GameComponents;
using JeopardyKing.Windows;
using Ookii.Dialogs.Wpf;

namespace JeopardyKing.ViewModels
{
    public partial class EditQuestionBoxViewModel : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private int _startVideoAtMinutes = 0;
        private int _startVideoAtSeconds = 0;
        private string _selectedMediaFlow = string.Empty;
        #endregion

        public int StartVideoAtMinutes
        {
            get => _startVideoAtMinutes;
            set
            {
                if (ModeManager.CurrentlySelectedQuestion != default)
                    ModeManager.CurrentlySelectedQuestion.StartVideoOrAudioAtSeconds = value * 60 + StartVideoAtSeconds;
                SetProperty(ref _startVideoAtMinutes, value);
            }
        }

        public int StartVideoAtSeconds
        {
            get => _startVideoAtSeconds;
            set
            {
                if (ModeManager.CurrentlySelectedQuestion != default)
                    ModeManager.CurrentlySelectedQuestion.StartVideoOrAudioAtSeconds = StartVideoAtMinutes * 60 + value;
                SetProperty(ref _startVideoAtSeconds, value);
            }
        }

        public string SelectedMediaFlow
        {
            get => _selectedMediaFlow;
            set
            {
                if (ModeManager.CurrentlySelectedQuestion != default)
                    ModeManager.CurrentlySelectedQuestion.MediaQuestionFlow = _mediaFlowMap[value];
                SetProperty(ref _selectedMediaFlow, value);
            }
        }

        public QuestionModeManager ModeManager { get; }

        public List<string> MediaFlowTypes { get; }
        #endregion

        #region Events
        public event EventHandler? NewMediaLoadedEvent;
        public event EventHandler<QuestionType>? QuestionTypeChangedEvent;
        #endregion

        #region Commands
        private RelayCommand? _deleteQuestionCommand;
        private RelayCommand? _loadMediaCommand;
        private RelayCommand? _getYoutubeLinkCommand;
        private RelayCommand? _refreshYoutubeVideoLink;

        public ICommand DeleteQuestionCommand
        {
            get
            {
                _deleteQuestionCommand ??= new RelayCommand(() =>
                {
                    PopupWindowModal confirmationDialog = new(
                        Application.Current.MainWindow,
                        "Are you sure?",
                        "Delete question",
                        x =>
                        {
                            if (x == ModalWindowButton.OK)
                            {
                                if (ModeManager.CurrentlySelectedQuestion == default)
                                    return;

                                _createWindowViewModel.GameBoard.DeleteQuestion(
                                    ModeManager.CurrentlySelectedQuestion.CategoryId,
                                    ModeManager.CurrentlySelectedQuestion.Id);

                                ModeManager.DeselectQuestion();
                            }
                        },
                        $"You are about to delete a question. Are you sure?");
                    _ = confirmationDialog.ShowDialog();
                });
                return _deleteQuestionCommand;
            }
        }

        public ICommand LoadMediaCommand
        {
            get
            {
                _loadMediaCommand ??= new RelayCommand(() =>
                {
                    if (ModeManager.CurrentlySelectedQuestion == default)
                        return;

                    VistaOpenFileDialog dialog = new()
                    {
                        Title = $"Select {ModeManager.CurrentlySelectedQuestion.Type} resource",
                        Multiselect = false,
                        Filter = GetFileExtensionsForType(ModeManager.CurrentlySelectedQuestion.Type),
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        ModeManager.CurrentlySelectedQuestion.SetMultimediaParameters(dialog.FileName);
                        NewMediaLoadedEvent?.Invoke(this, EventArgs.Empty);
                    }
                });
                return _loadMediaCommand;
            }
        }

        public ICommand GetYoutubeLinkCommand
        {
            get
            {
                _getYoutubeLinkCommand ??= new RelayCommand(() =>
                {
                    if (ModeManager.CurrentlySelectedQuestion == default)
                        return;

                    PopupWindowModal confirmationDialog =
                    new(Application.Current.MainWindow,
                        "Add YouTube link",
                        string.Empty,
                        (response, link) =>
                        {
                            if (response == ModalWindowButton.OK && TryExtractVideoId(link, out var videoId))
                                ModeManager.CurrentlySelectedQuestion.SetYoutubeVideoParameters(link, videoId, false, true);
                        },
                        ModeManager.CurrentlySelectedQuestion.OriginalYoutubeUrl,
                        ValidateYoutubeLink);

                    _ = confirmationDialog.ShowDialog();
                });
                return _getYoutubeLinkCommand;
            }
        }

        public ICommand RefreshYoutubeVideoLink
        {
            get
            {
                _refreshYoutubeVideoLink ??= new RelayCommand(() =>
                {
                    if (ModeManager.CurrentlySelectedQuestion == default)
                        return;
                    ModeManager.CurrentlySelectedQuestion.RefreshYoutubeVideoUrl(false, true);
                });
                return _refreshYoutubeVideoLink;
            }
        }
        #endregion

        #region Private fields
        private readonly Dictionary<string, MediaQuestionFlow> _mediaFlowMap = new();
        private readonly Dictionary<MediaQuestionFlow, string> _mediaFlowNameMap = new();
        private readonly CreateWindowViewModel _createWindowViewModel;
        #endregion

        public EditQuestionBoxViewModel(CreateWindowViewModel createWindowViewModel)
        {
            _createWindowViewModel = createWindowViewModel;
            ModeManager = _createWindowViewModel.ModeManager;

            MediaFlowTypes = new();

            EnumerationUtilities.ActOnEnumMembersWithAttribute<MediaQuestionFlow, MediaQuestionFlowInfoAttribute>((c, a) =>
            {
                if (c != MediaQuestionFlow.None)
                {
                    var displayName = a.DisplayText;
                    MediaFlowTypes!.Add(displayName);
                    _mediaFlowMap.Add(displayName, c);
                    _mediaFlowNameMap.Add(c, displayName);
                }
            });

            SelectedMediaFlow = MediaFlowTypes.First();

            ModeManager.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ModeManager.CurrentlySelectedQuestion) && ModeManager.CurrentlySelectedQuestion != default)
                {
                    if (ModeManager.CurrentlySelectedQuestion.Type != QuestionType.Text)
                    {
                        if (ModeManager.CurrentlySelectedQuestion.MediaQuestionFlow == MediaQuestionFlow.None)
                            SelectedMediaFlow = MediaFlowTypes.First();
                        else
                            SelectedMediaFlow = _mediaFlowNameMap[ModeManager.CurrentlySelectedQuestion.MediaQuestionFlow];
                    }

                    if (ModeManager.CurrentlySelectedQuestion.Type == QuestionType.YoutubeVideo)
                    {
                        StartVideoAtMinutes = (int)(ModeManager.CurrentlySelectedQuestion.StartVideoOrAudioAtSeconds) / 60;
                        StartVideoAtSeconds = (int)(ModeManager.CurrentlySelectedQuestion.StartVideoOrAudioAtSeconds) - (StartVideoAtMinutes * 60);
                    }
                    ModeManager.CurrentlySelectedQuestion.PropertyChanged += CurrentlySelectedQuestionPropertyChanged;
                }
            };

            ModeManager.PropertyChanging += (s, e) =>
            {
                if (e.PropertyName == nameof(ModeManager.CurrentlySelectedQuestion) && ModeManager.CurrentlySelectedQuestion != default)
                    ModeManager.CurrentlySelectedQuestion.PropertyChanged -= CurrentlySelectedQuestionPropertyChanged;
            };
        }

        private void CurrentlySelectedQuestionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not Question q)
                return;

            if (e.PropertyName == nameof(q.Type))
                QuestionTypeChangedEvent?.Invoke(this, q.Type);
        }

        #region Private methods
        private static string GetFileExtensionsForType(QuestionType type)
           => type switch
           {
               QuestionType.Image => "Image files (*.png, *.jpg, *.jpeg, *.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
               QuestionType.Audio => "Audio files (*.aac, *.aiff, *.flac, *.mid, *.mp1, *.mp2, *.mp3, *.ogg, *.wav, *.wma)|*.aac;*.aiff;*.flac;*.mid;*.mp1;*.mp2;*.mp3;*.ogg;*.wav;*.wma|All files (*.*)|*.*",
               QuestionType.Video => "Video files (*.3gpp, *.avi, *.divx, *.m1v, *.m2v, *.m4v, *.mkv, *.mov, *.mp2, *.mp2v, *.mp4, *.mp4v, *.mpeg, *.mpeg1, *.mpeg2, *.mpeg4, *.webm, *.wmv)|*.3gpp;*.avi;*.divx;*.m1v;*.m2v;*.m4v;*.mkv;*.mov;*.mp2;*.mp2v;*.mp4;*.mp4v;*.mpeg;*.mpeg1;*.mpeg2;*.mpeg4;*.webm;*.wmv|All files (*.*)|*.*",
               _ => throw new NotSupportedException(),
           };

        private static (bool isValid, string errorMessage) ValidateYoutubeLink(string link)
        {
            if (string.IsNullOrEmpty(link))
                return (false, "YouTube link invalid - input is empty");

            var linkValid = TryExtractVideoId(link, out _);
            return (linkValid, linkValid ? string.Empty : "YouTube link invalid - could not extract video ID");
        }

        private static bool TryExtractVideoId(string link, out string videoId)
        {
            var m = ExtractVideoIdRegex().Match(link);
            videoId = m.Success ? m.Value : string.Empty;
            return m.Success;
        }

        [GeneratedRegex(@"((?<=(v|V)/)|(?<=(\\?|\\&)v=)|(?<=be/)|(?<=embed/)|(?<=shorts/))([a-zA-Z0-9-_]{5,30})")]
        private static partial Regex ExtractVideoIdRegex();
        #endregion
    }
}
