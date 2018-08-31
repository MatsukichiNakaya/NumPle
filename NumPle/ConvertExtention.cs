using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Project.Extention
{
    /// <summary>値の型変換に関する拡張関数クラス</summary>
    public static class ConvertExtention
    {
        /// <summary>
        /// 値の変換
        /// </summary>
        /// <typeparam name="TOutput">変換後の型</typeparam>
        /// <param name="value">変換もとの値</param>
        public static TOutput To<TOutput>(this Object value)
        {
            var result = default(TOutput);
            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(TOutput));
                if (converter != null)
                {
                    result = (TOutput)converter.ConvertTo(value, typeof(TOutput));
                }
            }
            catch (Exception) { result = default(TOutput); }
            return result;
        }

        /// <summary>
        /// 型を指定しての変換
        /// </summary>
        /// <param name="value">変換もとの値</param>
        /// <param name="type">変換後の型</param>
        /// <remarks>戻り値のキャストが必要</remarks>
        public static Object To(this Object value, Type type)
        {
            Object result = type.IsValueType ? Activator.CreateInstance(type) : null;
            // 型コンバータ作成
            TypeConverter converter = TypeDescriptor.GetConverter(type);

            // 型コンバータが作成できていれば型を変換する
            if (converter != null)
            {
                result = converter.ConvertTo(value, type);
            }
            return result;
        }

        /// <summary>
        /// 値の変換(一次配列)
        /// </summary>
        /// <typeparam name="TInput">変換もとの型</typeparam>
        /// <typeparam name="TOutput">変換後の型</typeparam>
        /// <param name="values">変換もとの値</param>
        public static TOutput[] ConvertAll<TInput, TOutput>(this IEnumerable<TInput> values)
        {
            if (values == null) { return null; }
            return Array.ConvertAll(values.ToArray(), val => val.To<TOutput>());
        }

        /// <summary>
        /// 値の変換(二次配列)
        /// </summary>
        /// <typeparam name="TInput">変換もとの型</typeparam>
        /// <typeparam name="TOutput">変換後の型</typeparam>
        /// <param name="values">変換もとの値</param>
        public static TOutput[][] ConvertAll<TInput, TOutput>(this IEnumerable<IEnumerable<TInput>> values)
        {
            if (values == null) { return null; }

            var result = new TOutput[values.Count()][];
            Int32 count = 0;
            foreach (var val in values)
            {
                result[count] = val?.ConvertAll<TInput, TOutput>();
                count++;
            }
            return result;
        }

        /// <summary>
        /// 配列をObject型へ変換する
        /// </summary>
        /// <typeparam name="T">変換もとの型</typeparam>
        /// <param name="src">変換を行う配列</param>
        /// <returns>Object型に変換された配列</returns>
        public static Object ToObject<T>(this T[] src)
        {
            return src;
        }
    }
}


