using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using QuickCopyTags.Models;
using QuickCopyTags.ViewModels;

namespace QuickCopyTags.Views;

public partial class SettingsWindow : Window
{
    private static readonly DataFormat<string> TagIdFormat = DataFormat.CreateInProcessFormat<string>("application/x-quickcopytags-tag-id");
    private static readonly IBrush DragOverBrush = new SolidColorBrush(Color.FromArgb(60, 100, 149, 237));

    private readonly SettingsViewModel _viewModel;
    private CategoriesWindow? _categoriesWindow;

    public SettingsWindow(SettingsViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        _viewModel.TagMoved += OnTagMoved;
    }

    private void OnManageCategoriesClick(object? sender, RoutedEventArgs e)
    {
        if (_categoriesWindow is null || !_categoriesWindow.IsVisible)
        {
            _categoriesWindow = new CategoriesWindow(_viewModel.CreateCategoriesViewModel());
            _categoriesWindow.Show(this);
        }
        else
        {
            _categoriesWindow.Activate();
        }
    }

    private void OnCategorySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _viewModel.Save();
    }

    private void OnFontSizeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _viewModel.Save();
    }

    private void OnTagMoved(Tag tag)
    {
        // SelectedTag doesn't change reference across a reorder, so the binding never
        // re-fires and the newly recycled container never gets told it's selected.
        // Reassigning it explicitly forces the selection visual onto that container.
        TagListBox.SelectedItem = tag;

        ScrollToAndFocus(tag);
    }

    private void ScrollToAndFocus(Tag tag)
    {
        try
        {
            // ScrollIntoView/UpdateLayout can throw ("Invalid Arrange rectangle") from an
            // Avalonia VirtualizingStackPanel layout bug under some list states. This is a
            // purely cosmetic scroll/focus affordance, so a failure here shouldn't crash
            // the whole app or block the reorder itself, which already succeeded above.
            TagListBox.ScrollIntoView(tag);
            TagListBox.UpdateLayout();

            // NavigationMethod.Tab (rather than the default/pointer method) is what makes
            // FluentTheme actually render the focus-visible outline on the item.
            TagListBox.ContainerFromItem(tag)?.Focus(NavigationMethod.Tab);
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void OnFieldLostFocus(object? sender, RoutedEventArgs e)
    {
        _viewModel.Save();
    }

    private async void OnItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(sender as Control).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (sender is not Control { DataContext: Tag tag })
        {
            return;
        }

        var transfer = new DataTransfer();
        transfer.Add(DataTransferItem.Create(TagIdFormat, tag.Id));
        await DragDrop.DoDragDropAsync(e, transfer, DragDropEffects.Move);
    }

    private void OnItemDragEnter(object? sender, DragEventArgs e)
    {
        if (sender is Border border)
        {
            border.Background = DragOverBrush;
        }
    }

    private void OnItemDragLeave(object? sender, DragEventArgs e)
    {
        if (sender is Border border)
        {
            border.Background = Brushes.Transparent;
        }
    }

    private void OnItemDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DataTransfer.Contains(TagIdFormat) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnItemDrop(object? sender, DragEventArgs e)
    {
        if (sender is Border border)
        {
            border.Background = Brushes.Transparent;
        }

        if (sender is not Control { DataContext: Tag target })
        {
            return;
        }

        var draggedId = e.DataTransfer.TryGetValue(TagIdFormat);
        if (draggedId is null)
        {
            return;
        }

        _viewModel.ReorderTag(draggedId, target);
        e.Handled = true;
    }
}
