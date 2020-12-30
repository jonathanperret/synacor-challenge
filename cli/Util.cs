using System;
using System.Collections.Generic;

public static class Util
{
    public static void W<T>(T data)
    {
        Console.WriteLine(data);
    }

    public static Func<TIn, TOut> Memoize<TIn, TOut>(Func<TIn, TOut> fn)
    {
        var cache = new Dictionary<TIn, TOut>();
        return (x) =>
        {
            TOut result;
            if (cache.TryGetValue(x, out result))
            {
                return result;
            }
            result = fn(x);
            cache.Add(x, result);
            return result;
        };
    }

    public static Func<TIn, TOut> Memoize<TIn, TOut>(Func<TIn, Func<TIn, TOut>, TOut> fn)
    {
        var cache = new Dictionary<TIn, TOut>();
        Func<TIn, TOut> memoized = default;
        memoized = (x) =>
        {
            TOut result;
            if (cache.TryGetValue(x, out result))
            {
                return result;
            }
            result = fn(x, memoized);
            cache.Add(x, result);
            return result;
        };
        return memoized;
    }

}