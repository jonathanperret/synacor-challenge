using System;
using System.Linq;
using System.Collections.Generic;
using MoreLinq;
using Sprache;
using System.Numerics;

public static class Extensions
{
    public static long Product(this IEnumerable<long> seq) => seq.Aggregate((a, b) => a * b);
    public static BigInteger Product(this IEnumerable<BigInteger> seq) => seq.Aggregate((a, b) => a * b);

    public static IEnumerable<IEnumerable<TResult>> Matrix<TRow, TColumn, TResult>(this IEnumerable<TRow> rows, IEnumerable<TColumn> columns, Func<TRow, TColumn, TResult> fn)
    {
        return rows.Select((row) => columns.Select(column => fn(row, column)));
    }

    public static IEnumerable<IEnumerable<TResult>> Matrix<TRow, TColumn, TResult>(this IEnumerable<TRow> rows, IEnumerable<TColumn> columns, Func<TRow, TColumn, int, int, TResult> fn)
    {
        return rows.Select((row, rowIndex) => columns.Select((column, columnIndex) => fn(row, column, rowIndex, columnIndex)));
    }

    public static void Deconstruct<T>(this IEnumerable<T> source, out T item1, out T item2) => (item1, item2) = source.Fold(ValueTuple.Create<T, T>);
    public static void Deconstruct<T>(this IEnumerable<T> source, out T item1, out T item2, out T item3) => (item1, item2, item3) = source.Fold(ValueTuple.Create<T, T, T>);
    public static void Deconstruct<T>(this IEnumerable<T> source, out T item1, out T item2, out T item3, out T item4) => (item1, item2, item3, item4) = source.Fold(ValueTuple.Create<T, T, T, T>);

    public static IEnumerable<T> Debug<T>(this IEnumerable<T> source) => source.Debug(x => x.ToString());
    public static IEnumerable<T> Debug<T>(this IEnumerable<T> source, Func<T, string> formatter)
    {
        return source.Pipe(x => Console.WriteLine(formatter(x)));
    }

    public static string Str<T>(this IEnumerable<T> source, string delimiter = "") => source.ToDelimitedString(delimiter);
}