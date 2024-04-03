using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {
        private const int InitCulumnCount = 640;

        public ObservableCollection<Detail> Items { get; private set; } = [];

#pragma warning disable IDE0052 // 読み取られていないプライベート メンバーを削除
        private ScrollSynchronizer? _scrollSynchronizer;
#pragma warning restore IDE0052 // 読み取られていないプライベート メンバーを削除

        public MainWindow2()
        {
            InitializeComponent();

            InitData(InitCulumnCount);
        }

        #region 画面の初期化

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            Cursor = Cursors.Wait;

            InitColumns(InitCulumnCount);

            squares.ScrollViewer = previewScroll;

            grid.Visibility = Visibility.Visible;

            Dispatcher.InvokeAsync(() =>
            {
                Cursor = null;

                InitScrollSynchronizer();
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void InitScrollSynchronizer()
        {
            var scrollList = new List<ScrollViewer>
            {
                previewScroll
            };
            var gridScroll = DataGridHelper.GetScrollViewer(grid);
            if (gridScroll is not null)
            {
                scrollList.Add(gridScroll);
            }
            _scrollSynchronizer = new ScrollSynchronizer(scrollList);
        }
        #endregion

        #region 行列の初期化

        private void InitColumns(int count)
        {
            grid.Columns.Clear();

            var converter = new BooleanToVisibilityConverter();
            for (int columnIndex = 0; columnIndex < count; ++columnIndex)
            {
                var binding = new Binding($"Values[{columnIndex}].Value")
                {
                    Converter = converter
                };

                var factory = new FrameworkElementFactory(typeof(Rectangle));
                factory.SetValue(Rectangle.HeightProperty, 10.0);
                factory.SetValue(Rectangle.WidthProperty, 10.0);
                factory.SetValue(Rectangle.FillProperty, Brushes.LightSkyBlue);
                factory.SetBinding(Rectangle.VisibilityProperty, binding);

                var dataTemplate = new DataTemplate
                {
                    VisualTree = factory
                };

                var column = new DataGridTemplateColumn
                {
                    CellTemplate = dataTemplate
                };

                grid.Columns.Add(column);
            }
        }

        private void InitData(int count)
        {
            // バインドを切断
            Binding b = new("Items")
            {
                Source = null
            };
            grid.SetBinding(DataGrid.ItemsSourceProperty, b);

            var list = new List<Detail>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new Detail(InitCulumnCount));
            }

            Items = new ObservableCollection<Detail>(list);

            b = new Binding("Items")
            {
                Source = this
            };
            grid.SetBinding(DataGrid.ItemsSourceProperty, b);
        }
        #endregion

        #region 設定値変更

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var columnIndex = DataGridHelper.GetSelectedColumnIndex(grid);
            var rowIndex = DataGridHelper.GetSelectedRowIndex(grid);

            Items[rowIndex].Invert(columnIndex);

            UpdatePreview();
        }

        #endregion

        #region 一括設定

        private void DataGridCell_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            grid.Focus();
            grid.SelectedCells.Clear();

            DataGridCell? targetCell = DataGridHelper.GetCellAtMousePosition(sender, e);

            if (targetCell is null) return;
            grid.CurrentCell = new DataGridCellInfo(targetCell);
            grid.SelectedCells.Add(grid.CurrentCell);

            ShowContextMenu();
        }

        private void ShowContextMenu()
        {
            ContextMenu contextMenu = new();

            MenuItem menuItem = new()
            {
                Header = "行全部設定"
            };
            menuItem.Click += new RoutedEventHandler(AllOn);
            contextMenu.Items.Add(menuItem);

            contextMenu.IsOpen = true;
        }

        private void AllOn(object sender, RoutedEventArgs e)
        {
            var rowIndex = DataGridHelper.GetSelectedRowIndex(grid);
            Items[rowIndex].SetAll(true);

            UpdatePreview();
        }

        #endregion

        #region ドラッグ設定

        private bool _isDragging;
        private int _startRowIndex;
        private int _startColumnIndex;

        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;

            if (e.OriginalSource is not DependencyObject dependencyObj) return;

            var cell = FindVisualParent<DataGridCell>(dependencyObj);
            if (cell != null)
            {
                var index = DataGridHelper.GetRowColumnIndex(cell);
                _startRowIndex = index.RowIndex;
                _startColumnIndex = index.ColumnIndex;
            }
        }

        private void Grid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _startRowIndex = -1;
            _startColumnIndex = -1;

            UpdatePreview();
        }

        private void Grid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (_isDragging)
            {
                var currentColumnIndex = grid.CurrentCell.Column.DisplayIndex;

                var startIndex = _startColumnIndex < currentColumnIndex ? _startColumnIndex : currentColumnIndex;
                var endIndex = _startColumnIndex > currentColumnIndex ? _startColumnIndex : currentColumnIndex;

                for (int columnIndex = startIndex; columnIndex <= endIndex; columnIndex++)
                {
                    Items[_startRowIndex].SetOn(columnIndex);
                }
            }
        }

        private T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            if (parentObject is T parent)
            {
                return parent;
            }
            else
            {
                return FindVisualParent<T>(parentObject);
            }
        }

        #endregion

        #region ミニマップ

        #region 表示位置

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            if (sender is Thumb thumb)
            {
                if (thumb.Template.FindName("Thumb_Border", thumb) is Border border)
                {
                    border.BorderThickness = new Thickness(1);

                    e.Handled = true;
                }
            }
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (sender is Thumb thumb)
            {
                if (thumb.Template.FindName("Thumb_Border", thumb) is Border border)
                {
                    border.BorderThickness = new Thickness(0);

                    e.Handled = true;
                }
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb)
            {
                var x = Canvas.GetRight(thumb) - e.HorizontalChange;
                var y = Canvas.GetBottom(thumb) - e.VerticalChange;

                if (thumb.Parent is Canvas canvas)
                {
                    x = Math.Max(x, 0);
                    y = Math.Max(y, 0);
                    x = Math.Min(x, canvas.ActualWidth - thumb.ActualWidth);
                    y = Math.Min(y, canvas.ActualHeight - thumb.ActualHeight);
                }

                Canvas.SetRight(thumb, x);
                Canvas.SetBottom(thumb, y);

                e.Handled = true;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetRight(map, 0);
            Canvas.SetBottom(map, 0);

            squares.InvalidateVisual();
            MoveScroll();
        }

        #endregion

        #region スクロール

        private void Thumb_DragStarted2(object sender, DragStartedEventArgs e)
        {
            if (sender is Thumb thumb)
            {
                if (thumb.Template.FindName("Area_Thumb_Border", thumb) is Border border)
                {
                    border.BorderThickness = new Thickness(1);
                }

                gridPanel.Children.Remove(grid);

                previewScroll.Visibility = Visibility.Visible;

                squares.InvalidateVisual();

                e.Handled = true;
            }
        }

        private void Thumb_DragCompleted2(object sender, DragCompletedEventArgs e)
        {
            Cursor = Cursors.Wait;

            if (sender is Thumb thumb)
            {
                if (thumb.Template.FindName("Area_Thumb_Border", thumb) is Border border)
                {
                    border.BorderThickness = new Thickness(0);
                }

                gridPanel.Children.Insert(1, grid);

                Dispatcher.InvokeAsync(() =>
                {
                    if (gridPanel.Children.Contains(grid))  // ドラッグ開始終了の連続動作時にプレビュー表示されなくなる問題回避のため
                    {
                        previewScroll.Visibility = Visibility.Collapsed;
                    }

                    Cursor = null;
                }, System.Windows.Threading.DispatcherPriority.Background);

                e.Handled = true;
            }
        }

        private void Thumb_DragDelta2(object sender, DragDeltaEventArgs e)
        {
            VerticalScrollDelta(sender, e);
            HorizontalScrollDelta(sender, e);

            e.Handled = true;
        }

        private void MoveMiniMapThumb((double HorizontalRatio, double VerticalRatio) ratios)
        {
            if (map.Template.FindName("Area_Canvas", map) is not Canvas mapCanvas) return;
            if (map.Template.FindName("Area_Thumb", map) is not Thumb mapThumb) return;

            var x = mapCanvas.ActualWidth * ratios.HorizontalRatio;
            var y = mapCanvas.ActualHeight * ratios.VerticalRatio;

            Canvas.SetLeft(mapThumb, x);
            Canvas.SetTop(mapThumb, y);
        }

        #endregion

        #endregion

        #region ダミー表示

        private void UpdatePreview()
        {
            squares.Objects.Clear();

            var rowMax = Items.Count;
            var colmunMax = Items[0].Values.Count;

            for (int rowIndex = 0; rowIndex < rowMax; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < colmunMax; columnIndex++)
                {
                    if (Items[rowIndex].Values[columnIndex].Value)
                    {
                        Square square = CreateSquare(rowIndex, columnIndex);

                        squares.Objects.Add(square);
                    }
                }
            }
        }

        private static Square CreateSquare(int rowIndex, int columnIndex)
        {
            var square = new Square
            {
                Width = 16,
                Height = 16
            };
            Canvas.SetTop(square, 28 * rowIndex + 6);
            Canvas.SetLeft(square, 28 * columnIndex + 6);
            return square;
        }

        private void PreviewScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            squares.InvalidateVisual();

            MoveScroll();
        }

        private void MoveScroll()
        {
            var ratios = GetScrollRatio(previewScroll);
            MoveMiniMapThumb(ratios);
            MoveHorizontalScrollThumb(ratios);
            MoveVerticalScrollThumb(ratios);
        }

        private static (double HorizontalRatio, double VerticalRatio) GetScrollRatio(ScrollViewer scroll)
        {
            var gridWidth = scroll.ScrollableWidth;
            var gridHeight = scroll.ScrollableHeight;

            return ((scroll.HorizontalOffset / gridWidth), (scroll.VerticalOffset / gridHeight));
        }

        #endregion

        #region スクロールバー　垂直

        private void MoveVerticalScrollThumb((double HorizontalRatio, double VerticalRatio) ratios)
        {
            var y = verticalScrollCanvas.ActualHeight * ratios.VerticalRatio;
            Canvas.SetTop(verticalScrollThumb, y);
        }

        private void VerticalScrollThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            VerticalScrollDelta(sender, e);

            e.Handled = true;
        }

        private void VerticalScrollDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb)
            {
                var y = Canvas.GetTop(thumb) + e.VerticalChange;

                if (thumb.Parent is Canvas canvas)
                {

                    y = Math.Max(y, 0);
                    y = Math.Min(y, canvas.ActualHeight - thumb.ActualHeight);

                    Canvas.SetTop(thumb, y);

                    DataGridHelper.MoveVerticalScrollTo(grid, Canvas.GetTop(thumb) / canvas.ActualHeight);

                    previewScroll.ScrollToVerticalOffset(previewScroll.ScrollableHeight * (Canvas.GetTop(thumb) / canvas.ActualHeight));
                }
            }
        }

        #endregion

        #region スクロールバー　水平

        private void MoveHorizontalScrollThumb((double HorizontalRatio, double VerticalRatio) ratios)
        {
            var x = horizontalScrollCanvas.ActualWidth * ratios.HorizontalRatio;
            Canvas.SetLeft(horizontalScrollThumb, x);
        }

        private void HorizontalScrollThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            HorizontalScrollDelta(sender, e);

            e.Handled = true;
        }

        private void HorizontalScrollDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb)
            {
                var x = Canvas.GetLeft(thumb) + e.HorizontalChange;

                if (thumb.Parent is Canvas canvas)
                {

                    x = Math.Max(x, 0);
                    x = Math.Min(x, canvas.ActualWidth - thumb.ActualWidth);

                    Canvas.SetLeft(thumb, x);

                    DataGridHelper.MoveHorizontalScrollTo(grid, Canvas.GetLeft(thumb) / canvas.ActualWidth);

                    previewScroll.ScrollToHorizontalOffset(previewScroll.ScrollableWidth * (Canvas.GetLeft(thumb) / canvas.ActualWidth));
                }
            }
        }

        #endregion
    }
}