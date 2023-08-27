using System.Windows;

namespace JeopardyKing.WpfComponents
{
    internal enum TitleBarButton
    {
        Minimize,
        Maximize,
        Restore,
        Close
    }
    internal class TitleBarButtonClickedEventArgs : RoutedEventArgs
    {
        public TitleBarButton ButtonClicked { get; }

        public TitleBarButtonClickedEventArgs(RoutedEvent routedEvent, TitleBarButton buttonClicked) : base(routedEvent)
            => ButtonClicked = buttonClicked;
    }
}
