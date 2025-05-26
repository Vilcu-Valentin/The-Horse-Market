using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class WeightedSelector<T>
{
    /// <summary>
    /// Input a weigthes list (dictionary) of items and returns the selected item based on chance
    /// </summary>
    /// <param name="choices">A dictionary based on an item type and an int - tickets</param>
    /// <returns>A choice based on the weighted input list</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static T Pick(IEnumerable<(T item, int ticket)> choices)
    {
        int total = choices.Sum(c => c.ticket);
        if (total <= 0) throw new InvalidOperationException("No tickets to choose from");

        // r is [1 .. total]
        int r = UnityEngine.Random.Range(1, total + 1);

        foreach (var (item, w) in choices)
        {
            r -= w;
            if (r <= 0)
                return item;
        }

        // should never get here
        throw new InvalidOperationException();
    }
}