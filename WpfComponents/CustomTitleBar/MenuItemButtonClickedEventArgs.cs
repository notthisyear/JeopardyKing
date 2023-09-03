using System.Windows;

namespace JeopardyKing.WpfComponents
{
    internal enum MenuItemButton
    {
        Save,
        SaveAs,
        Open,
        AddCategory
    }
    internal class MenuItemButtonClickedEventArgs : RoutedEventArgs
    {
        public MenuItemButton ItemClicked { get; }

        public MenuItemButtonClickedEventArgs(RoutedEvent routedEvent, MenuItemButton itemClicked) : base(routedEvent)
            => ItemClicked = itemClicked;
    }
}
