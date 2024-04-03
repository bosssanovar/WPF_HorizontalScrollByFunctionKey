using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp1
{
    internal static class DataGridHelper
    {
        public static int GetSelectedColumnIndex(DataGrid grid)
        {
            return grid.CurrentCell.Column.DisplayIndex;
        }

        public static int GetSelectedRowIndex(DataGrid grid)
        {
            return grid.Items.IndexOf(grid.CurrentItem);
        }

        public static DataGridCell? GetCellAtMousePosition(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && cell is not System.Windows.Controls.DataGridCell) cell = VisualTreeHelper.GetParent(cell);
            System.Windows.Controls.DataGridCell? targetCell = cell as System.Windows.Controls.DataGridCell;
            return targetCell;
        }

        public static List<RowColumnIndex> GetSelectedCellsIndex(DataGrid grid)
        {
            var ret = new List<RowColumnIndex>();

            var cells = grid.SelectedCells;
            foreach ( var cell in cells )
            {
                ret.Add(new(grid.Items.IndexOf(cell.Item), cell.Column.DisplayIndex));
            }

            return ret;
        }

        public static void MoveScrollTo(DataGrid grid, double horizontalRatio,  double verticalRatio)
        {
            MoveVerticalScrollTo(grid, verticalRatio);
            MoveHorizontalScrollTo(grid, horizontalRatio);
        }

        public static void MoveVerticalScrollTo(DataGrid grid, double verticalRatio)
        {
            var scroll = GetScrollViewer(grid);
            if (scroll is null) return;

            var gridHeight = scroll.ScrollableHeight;

            scroll.ScrollToVerticalOffset(gridHeight * verticalRatio);
        }
        public static void MoveHorizontalScrollTo(DataGrid grid, double horizontalRatio)
        {
            var scroll = GetScrollViewer(grid);
            if (scroll is null) return;

            var gridWidth = scroll.ScrollableWidth;

            scroll.ScrollToHorizontalOffset(gridWidth * horizontalRatio);
        }

        public static ScrollViewer? GetScrollViewer(DataGrid grid)
        {
            var child = VisualTreeHelper.GetChild(grid, 0) as Decorator;
            var scroll = child?.Child as ScrollViewer;

            return scroll;
        }

        public static (double HorizontalRatio, double VerticalRatio) GetScrollAreaRaio(DataGrid grid)
        {
            var scroll = GetScrollViewer(grid);
            if (scroll is null) return (0, 0);

            var gridWidth = scroll.ScrollableWidth;
            var gridHeight = scroll.ScrollableHeight;

            return ((scroll.HorizontalOffset / gridWidth), (scroll.VerticalOffset / gridHeight));
        }

        public static RowColumnIndex GetRowColumnIndex(DataGridCell cell)
        {
            DataGridRow r2 = DataGridRow.GetRowContainingElement(cell);
            var rowIndex = r2.GetIndex();

            var columnIndex = cell.Column.DisplayIndex;

            return  new RowColumnIndex(rowIndex, columnIndex);
        }

        public class RowColumnIndex(int rowIndex, int columnIndex)
        {
            public int RowIndex { get; } = rowIndex;
            public int ColumnIndex { get; } = columnIndex;
        }
    }
}
