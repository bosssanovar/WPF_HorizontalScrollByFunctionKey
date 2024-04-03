using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1
{
    /**
     * @brief スクロールの同期する方向
     */
    [Flags]
    public enum SynchronizeDirection
    {
        //! 水平方向
        Horizontal = 0x01,

        //! 垂直方向
        Vertical = 0x02,

        //! 両方
        Both = 0x03,
    }

    /**
     * @brief データグリッドスクロール同期クラス
     */
    public class ScrollSynchronizer
    {
        //! スクロールビューワーリスト
        private readonly List<ScrollViewer> ScrollViewerList;

        //! スクロール方向
        private SynchronizeDirection Direction { get; set; }

        /**
         * @brier コンストラクタ
         * 
         * @param [in] dataGridList 同期するデータグリッドリスト
         * @param [in] direction 同期するスクロール方向
         */
        public ScrollSynchronizer(List<ScrollViewer> scrollViewerLiset, SynchronizeDirection direction = SynchronizeDirection.Both)
        {
            ScrollViewerList = [];

            // データグリッド数を取得します。
            int count = scrollViewerLiset.Count;

            // 同期するデータグリッド数が1以下の場合、何もしない。
            if (count < 2)
            {
                return;
            }

            // データグリッド数分繰り返します。
            for (int i = 0; i < count; ++i)
            {
                // データグリッドのスクロールビューワーを取得します。
                var scrollViewer = scrollViewerLiset[i];

                // スクロールビューワーにイベントハンドラを設定します。
                scrollViewer.ScrollChanged += ScrollChanged;

                // スクロールビューワーを識別するためタグを設定します。
                scrollViewer.Tag = i;

                // スクロールビューワーリストに保存します。
                ScrollViewerList.Add(scrollViewer);
            }

            // スクロール方向を保存します。
            Direction = direction;
        }

        /**
         * @brief スクロールされた時に呼び出されるます。
         * 
         * @param [in] sender スクロールビューワー
         * @param [in] e スクロールチェンジイベント
         */
        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is not ScrollViewer srcScrollViewer) return;

            // 同期するスクロール方向が水平方向の場合
            if (Direction.HasFlag(SynchronizeDirection.Horizontal))
            {
                // スクロールするオフセットを取得します。
                var offset = srcScrollViewer.HorizontalOffset;

                // スクロールビューワー数分繰り返します。
                foreach (var dstScrollVierwer in ScrollViewerList)
                {
                    // スクロールしたスクロールビューワーは無視します。
                    if (dstScrollVierwer.Tag == srcScrollViewer.Tag)
                    {
                        continue;
                    }

                    // 同期するスクロールビューワーをスクロールします。
                    dstScrollVierwer.ScrollToHorizontalOffset(offset);
                }
            }

            // 同期するスクロール方向が垂直方向の場合
            if (Direction.HasFlag(SynchronizeDirection.Vertical))
            {
                // スクロールするオフセットを取得します。
                var offset = srcScrollViewer.VerticalOffset;

                // スクロールビューワー数分繰り返します。
                foreach (var dstScrollVierwer in ScrollViewerList)
                {
                    // スクロールしたスクロールビューワーは無視します。
                    if (dstScrollVierwer.Tag == srcScrollViewer.Tag)
                    {
                        continue;
                    }

                    // 同期するスクロールビューワーをスクロールします。
                    dstScrollVierwer.ScrollToVerticalOffset(offset);
                }
            }
        }
    }
}