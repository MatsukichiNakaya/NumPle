using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Project.Extention;

namespace Project.Text
{
    /// <summary>CSVファイル操作クラス</summary>
    public class CSV : Text
    {
        /// <summary>
        /// 行単位での読み込み
        /// </summary>
        /// <param name="filePath">読込みファイルのパス</param>
        public static String[] ReadLine(String filePath) => Read(filePath)?.Split('\n');

        /// <summary>
        /// 行単位での読み込み
        /// </summary>
        /// <param name="filePath">読込みファイルのパス</param>
        /// <param name="enc">エンコード情報</param>
        public static String[] ReadLine(String filePath, Encoding enc)
        {
            // ファイル有無の確認
            if (!File.Exists(filePath)) { return null; }
            // 改行文字を統一
            String result = File.ReadAllText(filePath, enc).Replace("\r\n", "\n");
            while (result.EndsWith("\n"))
            {   // 末尾が改行ならば、削除する。
                result = result.Remove(result.Length - 1, 1);
            }
            return result.Split('\n');
        }

        /// <summary>
        /// 区切り文字で各行の要素を切り分ける読込み
        /// </summary>
        /// <param name="filePath">読込みファイルのパス</param>
        public static String[][] ReadCell(String filePath) => ReadCell(filePath, ',');

        /// <summary>
        /// 区切り文字で各行の要素を切り分ける読込み
        /// </summary>
        /// <param name="filePath">読込みファイルのパス</param>
        /// <param name="separator">区切り文字</param>
        public static String[][] ReadCell(String filePath, Char separator)
        {
            String[] lines = ReadLine(filePath);

            if(lines == null) { return null; }

            // 区切り文字で分割
            var result = new String[lines?.Length ?? 0][];
            for (Int32 row = 0; row < lines.Length; row++)
            {
                result[row] = lines[row].Split(separator);
            }
            return result;
        }

        /// <summary>
        /// 区切り文字で各行の要素を切り分ける読込み
        /// </summary>
        /// <param name="filePath">読込みファイルのパス</param>
        /// <param name="enc">エンコード情報</param>
        public static String[][] ReadCell(String filePath, Encoding enc) => ReadCell(filePath, enc, ',');

        /// <summary>
        /// 区切り文字で各行の要素を切り分ける読込み
        /// </summary>
        /// <param name="filePath">読込みファイルのパス</param>
        /// <param name="enc">エンコード情報</param>
        /// <param name="separator">区切り文字</param>
        public static String[][] ReadCell(String filePath, Encoding enc, Char separator)
        {
            // ファイル有無の確認
            if (!File.Exists(filePath)) { return null; }

            String[] lines = ReadLine(filePath);
            var result = new String[lines.Length][];

            for (Int32 row = 0; row < lines.Length; row++)
            {
                // カンマで区切って返す
                result[row] = lines[row].Split(separator);
            }
            return result;
        }

        /// <summary>
        /// CSVファイルへ値の書き出し
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="enc">エンコード情報</param>
        /// <param name="separator">区切り文字</param>
        /// <param name="values">String型に変換できる値</param>
        public static void WriteCell<T>(String filePath, Encoding enc, Char separator, T[][] values)
        {
            // 渡された値を文字列に変換
            String[][] writeVal = values.ConvertAll<T, String>();

            // 文字配列を文字列に変換
            var outputText = new StringBuilder();
            for (var row = 0; row < writeVal.Length; row++)
            {   // 最初の列を設定
                outputText.Append(writeVal[row][0]);
                for (var col = 1; col < writeVal[row].Length; col++)
                {   // 後ろに区切り文字と列の値を設定
                    outputText.Append(separator).Append(writeVal[row][col]);
                }
                // 行端を設定
                outputText.Append("\r\n");
            }
            // 書き出し
            Write(filePath, outputText.ToString(), Text.OVER_WRITE, Encoding.UTF8);
        }

        /// <summary>
        /// 配列を区切り文字で区切った文字列に変換する(","固定)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">配列</param>
        /// <returns>文字列</returns>
        public static String ToCsvLine<T>(IEnumerable<T> src) => ToCsvLine(src, ",");

        /// <summary>
        /// 配列を区切り文字で区切った文字列に変換する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">配列</param>
        /// <param name="separator">区切り文字</param>
        /// <returns>文字列</returns>
        public static String ToCsvLine<T>(IEnumerable<T> src, String separator)
        {   // 配列を文字列に変換
            var result = new StringBuilder();
            foreach (var word in src) { result.Append(word).Append(separator); }
            return result.ToString().TrimEnd(',');
        }

        /// <summary>
        /// 配列を区切り文字で区切った文字列に変換する。
        /// 各要素は指定のフォーマットが適用される
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">配列</param>
        /// <param name="separator">区切り文字</param>
        /// <param name="format">フォーマット文字列</param>
        /// <returns>文字列</returns>
        public static String ToCsvLine<T>(IEnumerable<T> src, String separator, String format)
        {
            // 配列を文字列に変換
            var result = new StringBuilder();
            foreach (var word in src)
            {
                result.Append(String.Format(format, word)).Append(separator);
            }
            return result.ToString().TrimEnd(',');
        }
    }
}
