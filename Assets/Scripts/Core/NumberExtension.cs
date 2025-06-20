using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NumberExtensions
{
    public static string ToShortString(this long value)
    {
        const long THOUSAND = 1_000L;
        const long MILLION = THOUSAND * THOUSAND;         // 1_000_000
        const long BILLION = MILLION * THOUSAND;         // 1_000_000_000
        const long TRILLION = BILLION * THOUSAND;         // 1_000_000_000_000

        if (value >= TRILLION)
            return value.ToString("0.##e+0");

        if (value >= BILLION)
            return (value / (double)BILLION).ToString("#.##0") + "b";

        if (value >= MILLION)
            return (value / (double)MILLION).ToString("#.##0") + "m";

        // < 1 000 000 → default grouping
        return value.ToString("#,##0");
    }

    // <summary>
    /// Rounds this long to an adaptive step based on its number of digits:
    /// 2-digit → nearest 5, 3-digit → nearest 10, 4-digit → nearest 50, 5-digit → nearest 100, etc.
    /// 1-digit values (0–9) are returned unchanged.
    /// </summary>
    public static long RoundToAdaptiveStep(this long value)
    {
        if (value == 0)
            return 0;

        long abs = Math.Abs(value);
        // how many digits?
        int digits = (int)Math.Floor(Math.Log10(abs)) + 1;
        if (digits < 2)
            return value;  // leave 1-digit values as-is

        int n = digits - 2;
        long step;
        if (n % 2 == 0)
        {
            // even n: step = 5 * 10^(n/2)
            step = 5L * (long)Math.Pow(10, n / 2);
        }
        else
        {
            // odd n:  step = 10^((n+1)/2)
            step = (long)Math.Pow(10, (n + 1) / 2);
        }

        // now round to nearest 'step', ties away from zero
        decimal quotient = (decimal)value / step;
        long roundedQuotient = (long)Math.Round(quotient, MidpointRounding.AwayFromZero);
        return roundedQuotient * step;
    }
}
