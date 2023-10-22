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

        private long _currentAudioOrVideoMs = 0;
        private long _startAudioOrVideoAtMs = 0;
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
                        audioVideoPlayer.MediaPlayer!.Stop();
                        audioVideoPlayer.MediaPlayer!.Play();
                        audioVideoPlayer.MediaPlayer!.Time = _currentAudioOrVideoMs;
                        ViewModel.CurrentPlayingMediaPositionSeconds = Math.Max((int)(_currentAudioOrVideoMs - _startAudioOrVideoAtMs), 0);
                    }
                }
                else if (e.PropertyName == nameof(ViewModel.InPlayerAnswering) && ViewModel.InPlayerAnswering)
                {
                    if (audioVideoPlayer.MediaPlayer!.IsPlaying)
                    {
                        _currentAudioOrVideoMs = audioVideoPlayer.MediaPlayer!.Time;
                        ViewModel.SetMediaContentPlaybackStatus(PlayWindowViewModel.MediaPlaybackStatus.Paused);
                    }
                    audioVideoPlayer.MediaPlayer!.Stop();
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
                if (ViewModel.CurrentQuestion == default)
                    return;

                var vm = ViewModel;
                var mediaPlayer = audioVideoPlayer.MediaPlayer!;
                Task.Run(() =>
                {
                    StopMediaPlayerAndPrimeForRestart(vm, mediaPlayer);
                });
            });
        }

        private void MediaPlayerPositionChanged(object? sender, MediaPlayerPositionChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ViewModel.CurrentQuestion == default)
                    return;

                var mediaPlayer = audioVideoPlayer.MediaPlayer!;
                var vm = ViewModel;
                _ = Task.Run(() =>
                {
                    var t = mediaPlayer.Time;
                    vm.CurrentPlayingMediaPositionSeconds = (int)((t - _startAudioOrVideoAtMs) / 1000);
                    if (_currentClipHasCustomEnd && t >= _endAudioOrVideoAtMs)
                        StopMediaPlayerAndPrimeForRestart(vm, mediaPlayer);
                });
            });
        }

        private void StopMediaPlayerAndPrimeForRestart(PlayWindowViewModel viewModel, VlcMediaPlayer mediaPlayer)
        {
            mediaPlayer.Pause();
            viewModel.SetMediaContentPlaybackStatus(PlayWindowViewModel.MediaPlaybackStatus.Stopped);
            _currentAudioOrVideoMs = _startAudioOrVideoAtMs;
        }

        private void LoadNewMedia(Question q)
        {
            using var media = new Media(_libVlc, q.MultimediaContentLink);
            audioVideoPlayer.MediaPlayer!.Media = media;
            audioVideoPlayer.MediaPlayer.Stop();
            _startAudioOrVideoAtMs = Convert.ToInt64(q.StartVideoOrAudioAtSeconds * 1000.0);
            _endAudioOrVideoAtMs = Convert.ToInt64(q.EndVideoOrAudioAtSeconds * 1000.0);
            _currentAudioOrVideoMs = _startAudioOrVideoAtMs;
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
