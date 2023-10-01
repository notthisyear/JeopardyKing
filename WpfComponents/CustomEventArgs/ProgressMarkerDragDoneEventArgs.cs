using System.Windows;

namespace JeopardyKing.WpfComponents.CustomEventArgs
{
    public class ProgressMarkerDragDoneEventArgs : RoutedEventArgs
    {
        public double ProgressMarkerValue { get; }

        public ProgressMarkerDragDoneEventArgs(RoutedEvent routedEvent, double progressMarkerValue) : base(routedEvent)
        {
            ProgressMarkerValue = progressMarkerValue;
        }
    }
}
