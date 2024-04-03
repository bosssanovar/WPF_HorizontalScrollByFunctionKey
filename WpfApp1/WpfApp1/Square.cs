using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1
{
    class Square : Control
    {
    }

    class Squares : Control
    {
        public ScrollViewer? ScrollViewer { get; set; }
        public List<Square> Objects { get; } = [];

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (ScrollViewer == null || Objects == null)
            {
                return;
            }

            // ScrollViewerで表示されている領域
            var viewRect = new Rect(ScrollViewer.HorizontalOffset, ScrollViewer.VerticalOffset, ScrollViewer.ViewportWidth, ScrollViewer.ViewportHeight);

            foreach (Square s in Objects)
            {
                var rect = new Rect(Canvas.GetLeft(s), Canvas.GetTop(s), s.Width, s.Height);
                // 四角形が表示領域内に含まれる場合のみ描画する
                if (viewRect.IntersectsWith(rect))
                {
                    drawingContext.DrawRectangle(Brushes.LightSkyBlue, new Pen(Brushes.LightSkyBlue, 1), rect);
                }
            }
        }
    }
}