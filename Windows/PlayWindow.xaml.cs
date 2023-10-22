using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using JeopardyKing.GameComponents;
using JeopardyKing.ViewModels;
using LibVLCSharp.Shared;

namespace JeopardyKing.Windows
{
    using VlcMediaPlayer = LibVLCSharp.Shared.MediaPlayer;

    public partial class PlayWindow : Window
    {
        public PlayWindowViewModel ViewModel
        {
            get => (PlayWindowViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(PlayWindowViewModel),
            typeof(PlayWindow),
            new FrameworkPropertyMetadata(null));

        private readonly LibVLC _libVlc;

        public PlayWindow()
        {
            InitializeComponent();

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

            _libVlc = new LibVLC();
            Loaded += PlayWindowLoaded;
        }

        private long _startAudioOrVideoAtMs = long.MinValue;
        private long _endAudioOrVideoAtMs = long.MaxValue;
        private bool _currentClipHasCustomEnd = false;

        private void PlayWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= PlayWindowLoaded;
            var mediaPlayer = new VlcMediaPlayer(_libVlc);
            audioVideoPlayer.MediaPlayer = mediaPlayer;
            audioVideoPlayer.MediaPlayer.EndReached += MediaPlayerEndReached;
            audioVideoPlayer.MediaPlayer.PositionChanged += MediaPlayerPositionChanged;
            ViewModel.PropertyChanged += ViewModelPropertyChanged;
        }

        private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ViewModel.CurrentQuestion == default)
                    return;

                if (e.PropertyName == nameof(ViewModel.CurrentQuestion))
                {
                    if (ViewModel.CurrentQuestion.Type == QuestionType.Audio || ViewModel.CurrentQuestion.Type == QuestionType.Video)
                        LoadNewMedia(ViewModel.CurrentQuestion);
                    return;
                }

                if (ViewModel.CurrentQuestion.Type != QuestionType.Audio && ViewModel.CurrentQuestion.Type != QuestionType.Video)
                    return;

                if (e.PropertyName == nameof(ViewModel.InMediaContentPlaying) && ViewModel.InMediaContentPlaying)
                {
                    if (audioVideoPlayer.MediaPlayer!.Media == default)
                        LoadNewMedia(ViewModel.CurrentQuestion);

                    if (!audioVideoPlayer.MediaPlayer!.IsPlaying)
                    {
                        ViewModel.CurrentPlayingMediaPositionSeconds = 0;
                        audioVideoPlayer.MediaPlayer!.Stop();
                        audioVideoPlayer.MediaPlayer!.Play();
                        audioVideoPlayer.MediaPlayer!.Time = _startAudioOrVideoAtMs;
                    }
                }
                else if (e.PropertyName == nameof(ViewModel.InPlayerAnswering) && ViewModel.InPlayerAnswering)
                {
                    if (audioVideoPlayer.MediaPlayer!.IsPlaying)
                    {
                        _startAudioOrVideoAtMs = audioVideoPlayer.MediaPlayer!.Time;
                        audioVideoPlayer.MediaPlayer!.Stop();
                        ViewModel.InMediaContentPlaying = false;
                    }
                }
                else if (e.PropertyName == nameof(ViewModel.InShowMediaContent) && !ViewModel.InShowMediaContent)
                {
                    ClearMediaPlayerMedia();
                }
            });
        }

        private void MediaPlayerEndReached(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ViewModel.InMediaContentPlaying = false;
                var mediaPlayer = audioVideoPlayer.MediaPlayer!;

                Task.Run(() =>
                {
                    mediaPlayer.Pause();
                });
            });
        }

        private void MediaPlayerPositionChanged(object? sender, MediaPlayerPositionChangedEventArgs e)
        {
            if (!_currentClipHasCustomEnd)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var mediaPlayer = audioVideoPlayer.MediaPlayer!;
                var vm = ViewModel;
                _ = Task.Run(() =>
                {
                    var t = mediaPlayer.Time;
                    vm.CurrentPlayingMediaPositionSeconds = (int)((t - _startAudioOrVideoAtMs) / 1000);
                    if (t >= _endAudioOrVideoAtMs)
                    {
                        mediaPlayer.Pause();
                        vm.InMediaContentPlaying = false;
                    }
                });
            });
        }

        private void LoadNewMedia(Question q)
        {
            using var media = new Media(_libVlc, q.MultimediaContentLink);
            audioVideoPlayer.MediaPlayer!.Media = media;
            audioVideoPlayer.MediaPlayer.Stop();
            _startAudioOrVideoAtMs = (long)q.StartVideoOrAudioAtSeconds * 1000;
            _endAudioOrVideoAtMs = (long)q.EndVideoOrAudioAtSeconds * 1000;
            _currentClipHasCustomEnd = _endAudioOrVideoAtMs < (1000 * q.VideoOrAudioLengthSeconds);
        }

        private void ClearMediaPlayerMedia()
        {
            if (audioVideoPlayer.MediaPlayer!.IsPlaying)
                audioVideoPlayer.MediaPlayer!.Stop();
            audioVideoPlayer.MediaPlayer!.Media?.Dispose();
            Application.Current.Dispatcher.Invoke(() => { audioVideoPlayer.MediaPlayer!.Media = default; });
        }
    }
}
