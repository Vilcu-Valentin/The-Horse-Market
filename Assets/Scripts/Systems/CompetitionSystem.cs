using UnityEngine;

public static class CompetitionSystem 
{
   public static HorseAI GenerateAIHorse(TierDef tier, float difficulty) // add the league modifier
    {
        int speed;
        int stamina;
        int jump;
        int strength;

        int statAvg = Mathf.RoundToInt(tier.StatCap * 1.4f * Mathf.Pow(difficulty, 0.15f) + 0.05f);

        float roll = Random.value;
        if(roll < difficulty)
        {
            speed = Random.Range(Mathf.FloorToInt(statAvg * 0.9f), Mathf.FloorToInt(statAvg * 1.35f));
            stamina = Random.Range(Mathf.FloorToInt(statAvg * 0.9f), Mathf.FloorToInt(statAvg * 1.35f));
            jump = Random.Range(Mathf.FloorToInt(statAvg * 0.9f), Mathf.FloorToInt(statAvg * 1.35f));
            strength = Random.Range(Mathf.FloorToInt(statAvg * 0.9f), Mathf.FloorToInt(statAvg * 1.35f));
        }
        else
        {
            speed = Random.Range(Mathf.FloorToInt(statAvg * 0.65f), Mathf.FloorToInt(statAvg * 1.1f));
            stamina = Random.Range(Mathf.FloorToInt(statAvg * 0.65f), Mathf.FloorToInt(statAvg * 1.1f));
            jump = Random.Range(Mathf.FloorToInt(statAvg * 0.65f), Mathf.FloorToInt(statAvg * 1.1f));
            strength = Random.Range(Mathf.FloorToInt(statAvg * 0.65f), Mathf.FloorToInt(statAvg * 1.1f));
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

        if (ratio >= 1.125f)
            return ("Outclassed", 4);
        else if (ratio >= 1.05f)
            return ("Weaker", 3);

        else if (ratio <= 1f / 1.125f)  
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
