using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CompetitionSystem 
{
    /// <summary>
    /// Calculates the finishing order with detailed debug logs and random tie-breaking.
    /// Returns a list of horse‐indexes in finish order.
    /// </summary>
    public static List<int> CalculateOutcome(
        Horse horse,
        List<HorseAI> competitors,
        CompetitionDef competition)
    {
        Debug.Log($"--- Competition '{competition.CompetitionName}' Debug Start: {competitors.Count} AI + player ---");

        // 1) Build scored list with random tieBreaker, player first
        int playerIdx = competitors.Count;
        long playerPower = CalculateHorseIndex(horse, competition);
        float playerTie = Random.value;
        var scored = new List<(int index, long power, float tieBreaker)>
        {
            (index: playerIdx, power: playerPower, tieBreaker: playerTie)
        };
        Debug.Log($"Player[{playerIdx}] power={playerPower}, tie={playerTie:F3} inserted first");

        // Append AI competitors
        for (int i = 0; i < competitors.Count; i++)
        {
            long aiPower = CalculateHorseIndex(competitors[i], competition);
            float aiTie = Random.value;
            scored.Add((index: i, power: aiPower, tieBreaker: aiTie));
            Debug.Log($"AI[{i}] power={aiPower}, tie={aiTie:F3} appended");
        }

        Debug.Log("Initial scored list (unsorted):");
        LogScores(scored.Select(x => (x.index, x.power, x.tieBreaker)).ToList());

        // 2) Base sort descending by power, then tieBreaker
        var baseScored = scored
            .OrderByDescending(x => x.power)
            .ThenByDescending(x => x.tieBreaker)
            .ToList();
        Debug.Log("Base order after sorting by power + tieBreaker:");
        LogScores(baseScored.Select(x => (x.index, x.power, x.tieBreaker)).ToList());

        // 3) Prepare dynamic list for upsets (drop tieBreaker now)
        var dynamicScored = baseScored
            .Select(x => (index: x.index, power: x.power))
            .ToList();

        // 4) Iterate challengers weakest to strongest
        Debug.Log("Beginning per-challenger upset checks:");
        for (int bi = baseScored.Count - 1; bi >= 0; bi--)
        {
            var challenger = baseScored[bi];
            int oldPos = dynamicScored.FindIndex(x => x.index == challenger.index);
            Debug.Log($"Challenger idx={challenger.index} startPos={oldPos}, power={challenger.power}");

            int newPos = oldPos;
            // bubble up as long as upset rolls succeed
            while (newPos > 0)
            {
                var above = dynamicScored[newPos - 1];
                var below = dynamicScored[newPos]; // challenger
                float r = (above.power > 0)
                    ? (float)below.power / above.power
                    : 1f;
                float upsetChance = (r <= 1f)
                    ? Mathf.Min(0.5f, Mathf.Pow(0.978f * r, 30f))
                    : Mathf.Clamp(1f - Mathf.Pow(0.978f * (1f / r), 30f), 0.5f, 0.999f);

                float roll = Random.value;
                Debug.Log($"  Compare with above idx={above.index} (power={above.power}), r={r:F3}, upsetChance={upsetChance:F3}, roll={roll:F3}");

                if (roll < upsetChance)
                {
                    newPos--;
                    Debug.Log($"    Upset success: moving challenger to pos {newPos}");
                }
                else
                {
                    Debug.Log("    Upset failed: stopping challenger movement");
                    break;
                }
            }

            if (newPos != oldPos)
            {
                dynamicScored.RemoveAt(oldPos);
                dynamicScored.Insert(newPos, (challenger.index, challenger.power));
                Debug.Log($"Challenger idx={challenger.index} inserted at newPos={newPos}");
            }
        }

        Debug.Log("After all upset passes, final dynamic order:");
        LogScores(dynamicScored.Select(x => (x.index, x.power, 0f)).ToList());

        var finishOrder = dynamicScored.Select(x => x.index).ToList();
        Debug.Log("Final finish indices: " + string.Join(", ", finishOrder));
        Debug.Log("--- Competition Debug End ---");

        return finishOrder;
    }

    private static void LogScores(List<(int index, long power, float tieBreaker)> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var e = list[i];
            Debug.Log($"  Pos {i}: idx={e.index}, power={e.power}, tie={e.tieBreaker:F3}");
        }
    }

    public static HorseAI GenerateAIHorse(TierDef tier, float difficulty, float leagueModifier) // add the league modifier
    {
        int speed;
        int stamina;
        int jump;
        int strength;

        int statAvg = Mathf.RoundToInt((tier.StatCap * 1.4f * Mathf.Pow(difficulty, 0.15f) + 0.05f) * leagueModifier * 1.5f);

        float roll = Random.value;
        if(roll < difficulty)
        {
            speed = Mathf.Max(1, Random.Range(Mathf.FloorToInt(statAvg * 0.95f), Mathf.FloorToInt(statAvg * 1.25f)));
            stamina = Mathf.Max(1, Random.Range(Mathf.FloorToInt(statAvg * 0.95f), Mathf.FloorToInt(statAvg * 1.25f)));
            jump = Mathf.Max(1, Random.Range(Mathf.FloorToInt(statAvg * 0.95f), Mathf.FloorToInt(statAvg * 1.25f)));
            strength = Mathf.Max(1, Random.Range(Mathf.FloorToInt(statAvg * 0.95f), Mathf.FloorToInt(statAvg * 1.25f)));
        }
        else
        {
            speed =Mathf.Max(1, Random.Range(Mathf.FloorToInt(statAvg * 0.75f), Mathf.FloorToInt(statAvg * 1.05f)));
            stamina = Mathf.Max(1,Random.Range(Mathf.FloorToInt(statAvg * 0.75f), Mathf.FloorToInt(statAvg * 1.05f)));
            jump = Mathf.Max(1, Random.Range(Mathf.FloorToInt(statAvg * 0.75f), Mathf.FloorToInt(statAvg * 1.05f)));
            strength = Mathf.Max(1, Random.Range(Mathf.FloorToInt(statAvg * 0.75f), Mathf.FloorToInt(statAvg * 1.05f)));
        }

        HorseAI generatedHorse = new HorseAI()
        {
            horseName = HorseNameGenerator.GetRandomHorseName(),
            staminaStat = stamina,
            speedStat = speed,
            jumpStat = jump,  
            strengthStat = strength
        };

        return generatedHorse;
    }

    public static long CalculateHorseIndex(Horse horse, CompetitionDef competitionDef)
    {
        int powerIndex = 0;

        foreach(var stat in competitionDef.competitionStats)
        {
            powerIndex += Mathf.RoundToInt(horse.GetCurrent(stat.Stat) * stat.weight);
        }

        powerIndex = Mathf.RoundToInt(powerIndex * horse.GetCompetitionMultiplier());

        return powerIndex;
    }

    public static long CalculateHorseIndex(HorseAI horse, CompetitionDef competitionDef)
    {
        int powerIndex = 0;

        foreach (var stat in competitionDef.competitionStats)
        {
            if(stat.Stat == StatType.Stamina)
                powerIndex += Mathf.RoundToInt(horse.staminaStat * stat.weight);
            if (stat.Stat == StatType.Speed)
                powerIndex += Mathf.RoundToInt(horse.speedStat * stat.weight);
            if (stat.Stat == StatType.JumpHeight)
                powerIndex += Mathf.RoundToInt(horse.jumpStat * stat.weight);
            if (stat.Stat == StatType.Strength)
                powerIndex += Mathf.RoundToInt(horse.strengthStat * stat.weight);
        }

        return powerIndex;
    }

    public static (string, int) GetHorseRating(long horseIndex, long competitionIndex)
    {
        float ratio = horseIndex / (float)competitionIndex;

        if (ratio >= 1.15f)
            return ("Outclassed", 4);
        else if (ratio >= 1.05f)
            return ("Weaker", 3);

        else if (ratio <= 1f / 1.15f)  
            return ("Superior", 0);
        else if (ratio <= 1f / 1.05f) 
            return ("Strong", 1);

        else if (ratio > 0.95f && ratio < 1.05f)
            return ("Comparable", 2);
        else
            return ("Comparable", 2);
    }

}

public struct HorseAI
{
    public string horseName;
    public int staminaStat;
    public int speedStat;
    public int jumpStat;
    public int strengthStat;
}
