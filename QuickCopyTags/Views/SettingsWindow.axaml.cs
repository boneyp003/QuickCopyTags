using Avalonia.Controls;
using Avalonia.Input;
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

    public SettingsWindow(SettingsViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        _viewModel.TagMoved += OnTagMoved;
    }

    private void OnTagMoved(Tag tag)
    {
        TagListBox.ScrollIntoView(tag);
        TagListBox.UpdateLayout();

        // SelectedTag doesn't change reference across a reorder, so the binding never
        // re-fires and the newly recycled container never gets told it's selected.
        // Reassigning it explicitly forces the selection visual onto that container.
        TagListBox.SelectedItem = tag;

        // NavigationMethod.Tab (rather than the default/pointer method) is what makes
        // FluentTheme actually render the focus-visible outline on the item.
        TagListBox.ContainerFromItem(tag)?.Focus(NavigationMethod.Tab);
    }

    private void OnFieldLostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
