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
        var total = choices.Sum(c => c.ticket);
        var r = UnityEngine.Random.Range(0, total);
        foreach (var (item, w) in choices)
        {
            r -= w;
            if (r <= 0) return item;
        }
        throw new InvalidOperationException();
    }
}