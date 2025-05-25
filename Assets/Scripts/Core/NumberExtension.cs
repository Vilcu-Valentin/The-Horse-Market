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

        // ≥ 1 000 000 000 000 → scientific
        if (value >= TRILLION)
            return value.ToString("0.##e+0");

        // ≥ 1 000 000 000 → millions suffix
        if (value >= BILLION)
            return (value / (double)MILLION).ToString("#,##0") + "m";

        // ≥ 1 000 000 → thousands suffix
        if (value >= MILLION)
            return (value / (double)THOUSAND).ToString("#,##0") + "k";

        // < 1 000 000 → default grouping
        return value.ToString("#,##0");
    }
}
