using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using QuickCopyTags.Models;
using QuickCopyTags.ViewModels;

namespace QuickCopyTags.Views;

public partial class CategoriesWindow : Window
{
    private static readonly DataFormat<string> CategoryIdFormat = DataFormat.CreateInProcessFormat<string>("application/x-quickcopytags-category-id");
    private static readonly IBrush DragOverBrush = new SolidColorBrush(Color.FromArgb(60, 100, 149, 237));

    private readonly CategoriesViewModel _viewModel;

    public CategoriesWindow(CategoriesViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
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

        if (sender is not Control { DataContext: Category category })
        {
            return;
        }

        var transfer = new DataTransfer();
        transfer.Add(DataTransferItem.Create(CategoryIdFormat, category.Id));
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
        e.DragEffects = e.DataTransfer.Contains(CategoryIdFormat) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnItemDrop(object? sender, DragEventArgs e)
    {
        if (sender is Border border)
        {
            border.Background = Brushes.Transparent;
        }

        if (sender is not Control { DataContext: Category target })
        {
            return;
        }

        var draggedId = e.DataTransfer.TryGetValue(CategoryIdFormat);
        if (draggedId is null)
        {
            return;
        }

        _viewModel.ReorderCategory(draggedId, target);
        e.Handled = true;
    }
}
