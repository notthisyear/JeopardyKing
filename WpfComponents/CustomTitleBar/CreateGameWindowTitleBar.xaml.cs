using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace JeopardyKing.WpfComponents
{
    public partial class CreateGameWindowTitleBar : UserControl
    {
        #region Dependency properties
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(CreateGameWindowTitleBar),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool GameHasUnsavedChanges
        {
            get => (bool)GetValue(GameHasUnsavedChangesProperty);
            set => SetValue(GameHasUnsavedChangesProperty, value);
        }
        public static readonly DependencyProperty GameHasUnsavedChangesProperty = DependencyProperty.Register(
            nameof(GameHasUnsavedChanges),
            typeof(bool),
            typeof(CreateGameWindowTitleBar),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool CanAddNewCategory
        {
            get => (bool)GetValue(CanAddNewCategoryProperty);
            set => SetValue(CanAddNewCategoryProperty, value);
        }
        public static readonly DependencyProperty CanAddNewCategoryProperty = DependencyProperty.Register(
            nameof(CanAddNewCategory),
            typeof(bool),
            typeof(CreateGameWindowTitleBar),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public string ApplicationDescription
        {
            get => (string)GetValue(ApplicationDescriptionProperty);
            set => SetValue(ApplicationDescriptionProperty, value);
        }
        public static readonly DependencyProperty ApplicationDescriptionProperty = DependencyProperty.Register(
            nameof(ApplicationDescription),
            typeof(string),
            typeof(CreateGameWindowTitleBar),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool ShowMaximizeButton
        {
            get => (bool)GetValue(ShowMaximizeButtonProperty);
            set => SetValue(ShowMaximizeButtonProperty, value);
        }
        public static readonly DependencyProperty ShowMaximizeButtonProperty = DependencyProperty.Register(
            nameof(ShowMaximizeButton),
            typeof(bool),
            typeof(CreateGameWindowTitleBar),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool ShowMinimizeButton
        {
            get => (bool)GetValue(ShowMinimizeButtonProperty);
            set => SetValue(ShowMinimizeButtonProperty, value);
        }
        public static readonly DependencyProperty ShowMinimizeButtonProperty = DependencyProperty.Register(
            nameof(ShowMinimizeButton),
            typeof(bool),
            typeof(CreateGameWindowTitleBar),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public WindowState CurrentWindowState
        {
            get => (WindowState)GetValue(CurrentWindowStateProperty);
            set => SetValue(CurrentWindowStateProperty, value);
        }
        public static readonly DependencyProperty CurrentWindowStateProperty = DependencyProperty.Register(
            nameof(CurrentWindowState),
            typeof(WindowState),
            typeof(CreateGameWindowTitleBar),
            new FrameworkPropertyMetadata(WindowState.Normal, WindowStateChangedCallback));

        private static void WindowStateChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CreateGameWindowTitleBar titleBar && e.NewValue is WindowState state)
                titleBar.SetMaximizeOrRestoreVisibility(state);
        }
        #endregion

        public event RoutedEventHandler TitleBarButtonPressed
        {
            add { AddHandler(TitleBarButtonPressedEvent, value); }
            remove { RemoveHandler(TitleBarButtonPressedEvent, value); }
        }

        public static readonly RoutedEvent TitleBarButtonPressedEvent = EventManager.RegisterRoutedEvent(
            nameof(TitleBarButtonPressed),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(CreateGameWindowTitleBar));

        public event RoutedEventHandler MenuItemButtonPressed
        {
            add { AddHandler(MenuItemButtonPressedEvent, value); }
            remove { RemoveHandler(MenuItemButtonPressedEvent, value); }
        }

        public static readonly RoutedEvent MenuItemButtonPressedEvent = EventManager.RegisterRoutedEvent(
            nameof(MenuItemButtonPressed),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(CreateGameWindowTitleBar));

        public CreateGameWindowTitleBar()
        {
            InitializeComponent();
            SetMaximizeOrRestoreVisibility(CurrentWindowState);
        }

        private void SetMaximizeOrRestoreVisibility(WindowState state)
        {
            maximizeButton.Visibility = state == WindowState.Maximized ? Visibility.Collapsed : Visibility.Visible;
            restoreButton.Visibility = state == WindowState.Maximized ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MinimizeButtonPressed(object sender, RoutedEventArgs e)
        {
            var eventArgs = new TitleBarButtonClickedEventArgs(TitleBarButtonPressedEvent, TitleBarButton.Minimize);
            RaiseEvent(eventArgs);
        }

        private void MaximizeButtonPressed(object sender, RoutedEventArgs e)
        {
            var eventArgs = new TitleBarButtonClickedEventArgs(TitleBarButtonPressedEvent, TitleBarButton.Maximize);
            RaiseEvent(eventArgs);
        }

        private void RestoreButtonPressed(object sender, RoutedEventArgs e)
        {
            var eventArgs = new TitleBarButtonClickedEventArgs(TitleBarButtonPressedEvent, TitleBarButton.Restore);
            RaiseEvent(eventArgs);
        }

        private void CloseButtonPressed(object sender, RoutedEventArgs e)
        {
            var eventArgs = new TitleBarButtonClickedEventArgs(TitleBarButtonPressedEvent, TitleBarButton.Close);
            RaiseEvent(eventArgs);
        }

        // TODO: When pressing the same menu item again, the menu should close.
        //       Right now, it closes and the immediately opens again.
        //       Figure out fix for that.
        private void FileMenuItemPressed(object sender, RoutedEventArgs e)
        {
            if (sender is not Button b || mainGrid.FindResource("fileMenu") is not ContextMenu menu)
                return;

            menu.PlacementTarget = b;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
        }

        private void EditMenuItemPressed(object sender, RoutedEventArgs e)
        {
            if (sender is not Button b || mainGrid.FindResource("editMenu") is not ContextMenu menu)
                return;

            menu.PlacementTarget = b;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
        }

        private void SaveMenuItemPressed(object sender, RoutedEventArgs e)
        {
            var eventArgs = new MenuItemButtonClickedEventArgs(MenuItemButtonPressedEvent, MenuItemButton.Save);
            RaiseEvent(eventArgs);
        }

        private void SaveAsMenuItemPressed(object sender, RoutedEventArgs e)
        {
            var eventArgs = new MenuItemButtonClickedEventArgs(MenuItemButtonPressedEvent, MenuItemButton.SaveAs);
            RaiseEvent(eventArgs);
        }

        private void OpenMenuItemPressed(object sender, RoutedEventArgs e)
        {
            var eventArgs = new MenuItemButtonClickedEventArgs(MenuItemButtonPressedEvent, MenuItemButton.Open);
            RaiseEvent(eventArgs);
        }

        private void ExitMenuItemPressed(object sender, RoutedEventArgs e)
            => CloseButtonPressed(sender, e);

        private void AddCategoryMenuItemPressed(object sender, RoutedEventArgs e)
        {
            var eventArgs = new MenuItemButtonClickedEventArgs(MenuItemButtonPressedEvent, MenuItemButton.AddCategory);
            RaiseEvent(eventArgs);
        }
    }
}
