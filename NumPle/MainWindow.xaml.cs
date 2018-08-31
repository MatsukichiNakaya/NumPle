using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NumPle
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private const Int32 GRID_LENGTH = 9;
        private NumBoard _board;

        public MainWindow()
        {
            InitializeComponent();

            try
            {   // Data set.
                this._board = new NumBoard(Project.Text.CSV.ReadCell(@".\data.csv"));
                Console.WriteLine("Read csv.");
            }
            catch (Exception) { throw; }

            GuideCreateGridCell();
            DesignCreateGridCell();
            DesignCellSet(this._board.BaseData);
            Console.WriteLine("Data set.");

            Console.WriteLine("Initialize application complete.");
        }

        #region Initialize
        /// <summary>
        /// 9 x 9 のセルを作成する
        /// </summary>
        private void DesignCreateGridCell()
        {
            for (Int32 i = 0; i < GRID_LENGTH; i++)
            {
                this.mainGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(1, GridUnitType.Star)
                });
                this.mainGrid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
            }
        }

        private void GuideCreateGridCell()
        {
            for (Int32 y = 1; y < this.guideGrid.RowDefinitions.Count; y++)
            {
                var txtY = new TextBlock() {
                    Text = (this.guideGrid.RowDefinitions.Count - y).ToString(),
                    Style = this.FindResource("TextBlockStyle") as Style,
                };
                Grid.SetRowSpan(txtY, 1);
                Grid.SetColumnSpan(txtY, 1);
                Grid.SetColumn(txtY, 0);
                Grid.SetRow(txtY, y);
                this.guideGrid.Children.Insert(0, txtY);
            }

            Int32 ascii = 'A';
            for (Int32 x = 1; x < this.guideGrid.RowDefinitions.Count; x++)
            {
                var txtX = new TextBlock() {
                    Text = ((Char)ascii).ToString(),
                    Style = this.FindResource("TextBlockStyle") as Style,
                };
                Grid.SetRowSpan(txtX, 1);
                Grid.SetColumnSpan(txtX, 1);
                Grid.SetColumn(txtX, x);
                Grid.SetRow(txtX, 0);
                this.guideGrid.Children.Insert(0, txtX);
                ascii++;
            }
        }

        /// <summary>
        /// セルに読み込んだデータを設定する
        /// </summary>
        private void DesignCellSet(Int32[][] data)
        {
            if(data == null) { return; }

            for (Int32 y = 0; y < this.mainGrid.RowDefinitions.Count; y++)
            {
                if(data.Length <= y) { continue; }

                for (Int32 x = 0; x < this.mainGrid.ColumnDefinitions.Count; x++)
                {
                    if (data[y].Length <= x) { continue; }
                    var ruledLine = new Border()
                    {
                        BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                        BorderThickness = new Thickness(x == 0 ? 2 : 0,
                                                        y == 0 ? 2 : 0, 
                                                        (x + 1) % 3 == 0 ? 2 : 1,
                                                        (y + 1) % 3 == 0 ? 2 : 1),
                        Background = new SolidColorBrush(data[y][x] != 0
                                                        ? Color.FromRgb(212, 212, 212)
                                                        : Color.FromRgb(255, 255, 255))
                    };
                    var txt = new TextBlock()
                    {
                        Text = data[y][x] == 0 ? "" : data[y][x].ToString(),
                        Style = this.FindResource("TextBlockStyle") as Style,
                    };
                    ruledLine.Child = txt;
                    Grid.SetRowSpan(ruledLine, 1);
                    Grid.SetColumnSpan(ruledLine, 1);
                    Grid.SetColumn(ruledLine, x);
                    Grid.SetRow(ruledLine, y);
                    this.mainGrid.Children.Insert(0, ruledLine);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void SetAnswer(Int32[][] data)
        {
            Int32 row = 0;
            Int32 col = 0;

            foreach (Object txt in this.mainGrid.Children)
            {
                row = Grid.GetRow(txt as UIElement);
                col = Grid.GetColumn(txt as UIElement);
                var txtBlock = ((txt as Border).Child as TextBlock);
                txtBlock.FontSize = 16;
                txtBlock.Text = data[row][col] == 0 ? "" : data[row][col].ToString();
            }
        }
        #endregion

        #region Ctrl + S
        /// <summary>
        /// ナンプレを解く処理を実行する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecutedCustomCommand(Object sender, ExecutedRoutedEventArgs e)
        {
            // 計算
            this._board.Solve();

            // 出力
            SetAnswer(this._board.SolveData);

            if (this._board.IsCompleted())
            {
                Console.WriteLine("Numple input complete!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanExecuteCustomCommand(Object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source is Control target)
                { e.CanExecute = true; }
            else
                { e.CanExecute = false; }
        }
        #endregion

        #region Ctrl + V
        /// <summary>
        /// 候補数字をすべて表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecutedViewCommand(Object sender, ExecutedRoutedEventArgs e)
        {
            Int32 row = 0;
            Int32 col = 0;

            foreach (Object txt in this.mainGrid.Children)
            {
                row = Grid.GetRow(txt as UIElement);
                col = Grid.GetColumn(txt as UIElement);

                var txtBlock = ((txt as Border).Child as TextBlock);

                if (0 < this._board.DebugValue[row][col].Count
                    && this._board.SolveData[row][col] == 0)
                {
                    txtBlock.FontSize = 10;
                    txtBlock.Text = ArrayToDispFormat(this._board.DebugValue[row][col]);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private String ArrayToDispFormat(System.Collections.Generic.IEnumerable<Int32> src)
        {
            var buffer = new String[GRID_LENGTH];
            var result = new StringBuilder(16);

            foreach (Int32 val in src)
            {
                buffer[val - 1] = val.ToString();
            }
            for (Int32 i = 0; i < buffer.Length; i++)
            {
                if (i % 3 == 2)
                {
                    result.Append(buffer[i] ?? " ").Append("\r\n");
                }
                else
                {
                    result.Append(buffer[i] ?? " ");
                }
            }
            return result.ToString().Remove(result.Length - 2, 2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanExecuteViewCommand(Object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source is Control target)
                { e.CanExecute = true; }
            else
                { e.CanExecute = false; }
        }
        #endregion

        #region Ctrl + D
        /// <summary>
        /// デバッグ中の処理を実行する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecutedDebugCommand(Object sender, ExecutedRoutedEventArgs e)
        {
            this._board.Debug();

            // 出力
            SetAnswer(this._board.SolveData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanExecuteDebugCommand(Object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source is Control target)
            { e.CanExecute = true; }
            else
            { e.CanExecute = false; }
        }
        #endregion

    }
}
