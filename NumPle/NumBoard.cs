using System;
using System.Collections.Generic;
using Project.Extention;

namespace NumPle
{
    public class NumBoard
    {
        private const Int32 GRID_LENGTH = 9;
        private const Int32 BOX_LENGTH = 3;

        private struct VPos
        {
            public const UInt32 TOP = 0b0001;
            public const UInt32 MID = 0b0010;
            public const UInt32 BOT = 0b0100;
        }

        private struct HPos
        {
            public const UInt32 LEFT   = 0b0001;
            public const UInt32 CENTER = 0b0010;
            public const UInt32 RIGHT  = 0b0100;
        }

        private HashSet<Int32>[][] _solvTable = null;

        /// <summary>問題データ</summary>
        public Int32[][] BaseData { get; } = null;

        /// <summary>回答データ</summary>
        public Int32[][] SolveData { get; } = null;

        /// <summary>行数</summary>
        private Int32 Rows { get { return this.BaseData.Length; } }
        /// <summary>列数</summary>
        private Int32 Columns { get; }

        public HashSet<Int32>[][] DebugValue { get { return this._solvTable; } }

        #region Initialize
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NumBoard(String[][] numData)
        {
            this.BaseData = new Int32[GRID_LENGTH][];
            this.SolveData = new Int32[GRID_LENGTH][];
            this._solvTable = new HashSet<Int32>[GRID_LENGTH][];

            // 読み込んだデータをテーブルに配置する
            for (Int32 r = 0; r < this.BaseData.Length; r++)
            {
                this.BaseData[r] = new Int32[GRID_LENGTH];
                this.SolveData[r] = new Int32[GRID_LENGTH];
                this._solvTable[r] = new HashSet<Int32>[GRID_LENGTH];

                if (numData.Length <= r) { continue; }
                if (this.BaseData[r].Length < numData[r].Length)
                {
                    numData[r].SkipTake(0, GRID_LENGTH)
                              .ConvertAll<String, Int32>()
                              .CopyTo(this.BaseData[r], 0);
                }
                else
                {
                    numData[r].ConvertAll<String, Int32>()
                              .CopyTo(this.BaseData[r], 0);
                }
            }

            // ハッシュの初期化と回答用テーブルへデータコピー
            InnerTableClear(this.BaseData);
            this.Columns = this.BaseData.ColumnsMax();
        }

        /// <summary>
        /// ハッシュの初期化
        /// </summary>
        /// <remarks>
        /// 各マスに対する入力可能な数値を入力する
        /// </remarks>
        private void InnerTableClear(Int32[][] baseMatrix)
        {
            // 計算用の配列を作成
            for (Int32 r = 0; r < this.BaseData.Length; r++)
            {
                for (Int32 c = 0; c < this.BaseData[r].Length; c++)
                {
                    if (baseMatrix[r][c] == 0)
                    {
                        this._solvTable[r][c] = new HashSet<Int32>(
                                                    new Int32[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                    }
                    else
                    {
                        this._solvTable[r][c] = new HashSet<Int32>();
                    }
                }
            }
            this.BaseData.CopyTo(this.SolveData, 0);
        }
        #endregion

        #region 確認関数
        /// <summary>
        /// すべての入力が完了したか確認する
        /// </summary>
        /// <returns></returns>
        public Boolean IsCompleted()
        {
            // 各行に 0(未入力)が無いか確認する
            foreach (var row in this.SolveData)
            {
                if (0 < row.CountIf(c => c == 0))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 未入力になっている項目数を取得
        /// </summary>
        /// <returns></returns>
        private Int32 EmptyCount()
        {
            Int32 result = 0;
            foreach (var row in this.SolveData)
            {
                result += row.CountIf(c => c == 0);
            }
            return result;
        }
        #endregion

        #region メイン関数
        /// <summary>
        /// メイン関数
        /// </summary>
        public void Solve()
        {
            Int32 prevCount = 0;
            Int32 count = 1;
            while (0 < count)
            {
                prevCount = EmptyCount();

                // 重複削除
                Distinct();
                SetAnswer();

                count = EmptyCount();
                // カウントの変化がなくなった場合終了
                if(prevCount == count) { break; }
            }

            if(count == 0) { return; }

            // 候補が一つに絞れるときに確定させる
            OnlyOneInTheBox();
            SetAnswer();
#if true
            // 他行を見て候補を絞る
            SideOnlyNumRow();
            SetAnswer();
            SideOnlyNumCol();
            SetAnswer();
#endif
        }

        /// <summary>
        /// デバッグ用
        /// </summary>
        public void Debug()
        {
            // todo: 検証中
            // 3x3のボックス内でその行にしか入らない数字以外の数字を削除する
            SideOnlyExceptNum();
        }
#endregion

        #region GetValues
        /// <summary>
        /// 列を取得
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        private IEnumerable<Int32> GetVarticalNums(Int32[][] matrix, Int32 col)
        {
            for (Int32 row = 0; row < GRID_LENGTH; row++)
            {
                if (matrix[row][col] == 0) { continue; }
                yield return matrix[row][col];
            }
        }

        /// <summary>
        /// Get 3x3 Box Nums
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private IEnumerable<Int32> GetBoxNums(Int32[][] matrix, Int32 row, Int32 col)
        {
            Int32 startRow = row / 3 * BOX_LENGTH;
            Int32 startCol = col / 3 * BOX_LENGTH;

            for (Int32 y = 0; y < BOX_LENGTH; y++)
            {
                for (Int32 x = 0; x < BOX_LENGTH; x++)
                {
                    if (matrix[startRow + y][startCol + x] == 0) { continue; }
                    yield return matrix[startRow + y][startCol + x];
                }
            }
        }

        /// <summary>
        /// Get 3x3 Box HashSet values
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private IEnumerable<Int32> GetBoxHash(Int32 row, Int32 col)
        {
            Int32 startRow = row / 3 * BOX_LENGTH;
            Int32 startCol = col / 3 * BOX_LENGTH;

            for (Int32 y = 0; y < BOX_LENGTH; y++)
            {
                for (Int32 x = 0; x < BOX_LENGTH; x++)
                {
                    if (this._solvTable[startRow + y][startCol + x].Count == 0) { continue; }

                    foreach (var h in this._solvTable[startRow + y][startCol + x])
                    {
                        yield return h;
                    }
                }
            }
        }

        /// <summary>
        /// それぞれの列挙を一つにまとめる
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private IEnumerable<Int32> Concasts<Int32>(params IEnumerable<Int32>[] values)
        {
            foreach (var val in values)
            {
                foreach (var v in val)
                {
                    yield return v;
                }
            }
        }
        #endregion

        #region Solve Methods
        /// <summary>
        /// 候補が一つだけの場合に回答用の配列に値を設定する
        /// </summary>
        private void SetAnswer()
        {
            // Set Answer
            for (Int32 r = 0; r < GRID_LENGTH; r++)
            {
                for (Int32 c = 0; c < GRID_LENGTH; c++)
                {
                    if (this._solvTable[r][c].Count != 1) { continue; }

                    this.SolveData[r][c] = this._solvTable[r][c].First();
                    this._solvTable[r][c] = new HashSet<Int32>();
                }
            }
        }

        #region Distinct
        /// <summary>
        /// search and destroy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hash"></param>
        /// <param name="value"></param>
        private void ContaintsRemove<T>(ref HashSet<T> hash, IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                hash.RemoveWhere(h => h.Equals(value));
            }
        }

        /// <summary>
        /// 同じ行と列に有る数字を削除して候補を絞る
        /// </summary>
        private void Distinct()
        {
            for (Int32 row = 0; row < GRID_LENGTH; row++)
            {
                var rowNums = this.SolveData[row].Filter(c => 0 < c);
                // length 0 value
                IEnumerable<Int32> boxNums = new Int32[] { };

                for (Int32 col = 0; col < GRID_LENGTH; col++)
                {
                    if (col % 3 == 0) { boxNums = GetBoxNums(this.SolveData, row, col); }

                    if (this.SolveData[row][col] != 0) { continue; }

                    // search and destroy
                    ContaintsRemove(ref this._solvTable[row][col],
                                    // Combine lists
                                    Concasts(rowNums, boxNums,
                                             GetVarticalNums(this.SolveData, col))
                                             .SameFilter());
                }
            }
        }
        #endregion

        #region OnlyOneInTheBox
        /// <summary>
        /// 候補が一つだけの時に確定させる
        /// </summary>
        private void OnlyOneInTheBox()
        {
            Int32 count = 0;
            for (Int32 row = 0; row < GRID_LENGTH; row += 3)
            {
                for (Int32 col = 0; col < GRID_LENGTH; col += 3)
                {
                    // ボックス内の候補をすべて取得して数をカウントする
                    var values = GetBoxHash(row, col);

                    for (Int32 n = 1; n <= GRID_LENGTH; n++)
                    {
                        count = values.CountIf(v => v == n);
                        if (count == 1)
                        {
                            AdjustBoxHash(ref this._solvTable, n, row, col);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ボックス内を調整して確定させる
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        private void AdjustBoxHash(ref HashSet<Int32>[][] hash, Int32 num, 
                                   Int32 row, Int32 col)
        {
            for (Int32 y = 0; y < BOX_LENGTH; y++)
            {
                for (Int32 x = 0; x < BOX_LENGTH; x++)
                {
                    if (hash[row + y][col + x].Contains(num))
                    {
                        hash[row + y][col + x] = new HashSet<Int32>(new Int32 [] { num });
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Box内の3行を見てほかのボックスの同行の候補を削除する
        /// </summary>
        /// <remarks>
        /// 3行の内、1行のみに候補が絞られている場合、
        /// 他のボックス内の同じ行に同じ数値がある場合は削除する。
        /// </remarks>
        private void SideOnlyNumRow()
        {
            for (Int32 row = 0; row < GRID_LENGTH; row += 3)
            {
                UInt32 flag = 0;
                for (Int32 col = 0; col < GRID_LENGTH; col += 3)
                {
                    var top = new HashSet<Int32>(GetHashSetInBox(row, col, true).SameFilter());
                    var mid = new HashSet<Int32>(GetHashSetInBox(row + 1, col, true).SameFilter());
                    var bot = new HashSet<Int32>(GetHashSetInBox(row + 2, col, true).SameFilter());

                    for (Int32 n = 1; n <= GRID_LENGTH; n++)
                    {
                        flag = HasNumHash(n, top, mid, bot);

                        if (BitCount(flag) == 1)
                        {
                            // 候補を削除する
                            RemoveHashSelectH(row + GetBitONNum(flag), n,
                                              ConvHorizontalFlag(col));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SideOnlyNumCol()
        {
            for (Int32 row = 0; row < GRID_LENGTH; row += 3)
            {
                UInt32 flag = 0;
                for (Int32 col = 0; col < GRID_LENGTH; col += 3)
                {
                    var left   = new HashSet<Int32>(GetHashSetInBox(row, col, false).SameFilter());
                    var center = new HashSet<Int32>(GetHashSetInBox(row, col + 1, false).SameFilter());
                    var right  = new HashSet<Int32>(GetHashSetInBox(row, col + 2, false).SameFilter());

                    for (Int32 n = 1; n <= GRID_LENGTH; n++)
                    {
                        flag = HasNumHash(n, left, center, right);

                        if (BitCount(flag) == 1)
                        {
                            RemoveHashSelectR(col + GetBitONNum(flag), n, ConvHorizontalFlag(row));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private IEnumerable<Int32> GetHashSetInBox(Int32 row, Int32 col, Boolean vectol)
        {
            var result = new List<Int32>();

            for (Int32 n = 0; n < BOX_LENGTH; n++)
            {
                result.AddRange(this._solvTable[vectol ? row : row + n][!vectol ? col : col + n]);
            }
            return result;
        }

        #region BitFlag
        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="top"></param>
        /// <param name="mid"></param>
        /// <param name="bot"></param>
        /// <returns></returns>
        private UInt32 HasNumHash(Int32 n, HashSet<Int32> top,
                                  HashSet<Int32> mid, HashSet<Int32> bot)
            => (top.Contains(n) ? VPos.TOP : 0u)
             | (mid.Contains(n) ? VPos.MID : 0u)
             | (bot.Contains(n) ? VPos.BOT : 0u);


        /// <summary>
        /// ビット数カウント
        /// </summary>
        /// <param name="value">カウントする数値</param>
        /// <remarks>
        /// マイナス値が入らないように
        /// 符号無での値を設定
        /// </remarks>
        private Int32 BitCount(UInt32 value)
        {
            var bits = (Int32)value;
            bits = (bits & 0x55555555) + (bits >> 1  & 0x55555555);
            bits = (bits & 0x33333333) + (bits >> 2  & 0x33333333);
            bits = (bits & 0x0f0f0f0f) + (bits >> 4  & 0x0f0f0f0f);
            bits = (bits & 0x00ff00ff) + (bits >> 8  & 0x00ff00ff);
            return (bits & 0x0000ffff) + (bits >> 16 & 0x0000ffff);
        }

        private Int32 GetBitONNum(UInt32 value)
        {
            switch (value)
            {
                case VPos.TOP: return 0;
                case VPos.MID: return 1;
                case VPos.BOT: return 2;
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private UInt32 ConvHorizontalFlag(Int32 val)
        {
            switch (val)
            {
                case 0: return HPos.LEFT;
                case 3: return HPos.CENTER;
                case 6: return HPos.RIGHT;
            }
            return HPos.LEFT;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="num"></param>
        /// <param name="hPos"></param>
        private void RemoveHashSelectH(Int32 row, Int32 num, UInt32 hPos)
        {
            Int32 AStart = 0;
            Int32 BStart = 0;
            switch (hPos)
            {
                case HPos.LEFT:
                    AStart = 3; BStart = 6;
                    break;
                case HPos.CENTER:
                    AStart = 0; BStart = 6;
                    break;
                case HPos.RIGHT:
                    AStart = 0; BStart = 3;
                    break;
            }
            RemoveHash(num, row, 1, AStart, BOX_LENGTH);
            RemoveHash(num, row, 1, BStart, BOX_LENGTH);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <param name="num"></param>
        /// <param name="vPos"></param>
        private void RemoveHashSelectR(Int32 col, Int32 num, UInt32 vPos)
        {
            Int32 AStart = 0;
            Int32 BStart = 0;
            switch (vPos)
            {
                case VPos.TOP:
                    AStart = 3; BStart = 6;
                    break;
                case VPos.MID:
                    AStart = 0; BStart = 6;
                    break;
                case VPos.BOT:
                    AStart = 0; BStart = 3;
                    break;
            }
            RemoveHash(num, AStart, BOX_LENGTH, col, 1);
            RemoveHash(num, BStart, BOX_LENGTH, col, 1);
        }

        /// <summary>
        /// 候補削除
        /// </summary>
        /// <param name="num">対象数字</param>
        /// <param name="row">開始行</param>
        /// <param name="rowLen">行の長さ</param>
        /// <param name="col">開始列</param>
        /// <param name="colLen">列の長さ</param>
        private void RemoveHash(Int32 num, Int32 row, Int32 rowLen, Int32 col, Int32 colLen)
        {
            for (Int32 r = 0; r < rowLen; r++)
            {
                for (Int32 c = 0; c < colLen; c++)
                {
                    this._solvTable[r + row][c + col].RemoveWhere(h => h == num);
                }
            }
        }

        /// <summary>
        /// Box内の3行を見てその行に確定している以外の数字を削除する
        /// </summary>
        private void SideOnlyExceptNum()
        {
            for (Int32 row = 0; row < GRID_LENGTH; row += 3)
            {
                
                for (Int32 col = 0; col < GRID_LENGTH; col += 3)
                {
                    var top = new HashSet<Int32>(GetHashSetInBox(row, col, true).SameFilter());
                    var mid = new HashSet<Int32>(GetHashSetInBox(row + 1, col, true).SameFilter());
                    var bot = new HashSet<Int32>(GetHashSetInBox(row + 2, col, true).SameFilter());

                    ExcludeExceptForFixed(new HashSet<Int32>[] { top, mid, bot }, row, col);
                }
            }
        }

        /// <summary>
        /// ボックス無いの行で確定している候補以外を削除する
        /// </summary>
        /// <remarks>※要デバッグ(何問か解いて検証の必要あり)</remarks>
        private void ExcludeExceptForFixed(HashSet<Int32>[] hashList, Int32 row, Int32 col)
        {
            UInt32 flag = 0;
            Int32 r = -1;
            foreach (var list in hashList)
            {
                r++;
                var candidate = new List<Int32>();
                // その行にしかない数字を絞り込む
                foreach (var n in list)
                {
                    // Todo: マジックナンバー
                    flag = HasNumHash(n, hashList[0], hashList[1], hashList[2]);
                    if (BitCount(flag) == 1)
                    {
                        candidate.Add(n);
                    }
                }
                // 候補が二つ以上か？
                if (candidate.Count <= 1) { continue; }

                for (Int32 c = 0; c < BOX_LENGTH; c++)
                {
                    if (!HasCandidate(this._solvTable[row + r][c + col], candidate)) { continue; }
                    
                    // 確定候補にマッチしない数値を削除
                    this._solvTable[row + r][col + c].RemoveWhere(h => !IsListMatch(h, candidate));
                }
            }
        }

        /// <summary>
        /// ベースのハッシュセットに候補が含まれているか？
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private Boolean HasCandidate(HashSet<Int32> hash, IEnumerable<Int32> list)
        {
            foreach (var item in list)
            {
                if (hash.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// リストに含まれている数字か？
        /// </summary>
        /// <param name="num"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private Boolean IsListMatch(Int32 num, IEnumerable<Int32> list)
        {
            foreach (var item in list)
            {
                if (item == num)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
