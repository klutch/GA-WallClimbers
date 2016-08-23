using UnityEngine;
using System.Collections.Generic;

public class RouletteWheel
{
    public struct RouletteWheelRange
    {
        public float Min;
        public float Max;
        public ClimberGenetics Genetics;

        public RouletteWheelRange(float min, float max, ClimberGenetics genetics)
        {
            Min = min;
            Max = max;
            Genetics = genetics;
        }
    }

    private float totalScore = 0;

    public List<RouletteWheelRange> Ranges;

    public RouletteWheel(List<ClimberGenetics> source)
    {
        Ranges = new List<RouletteWheelRange>();

        foreach (ClimberGenetics genetics in source)
        {
            float min = totalScore;
            float max = totalScore + genetics.FitnessScore;

            Ranges.Add(new RouletteWheelRange(min, max, genetics));
            totalScore = max;
        }
    }

    public ClimberGenetics GetResult()
    {
        float score = Random.Range(0f, totalScore);

        foreach (RouletteWheelRange range in Ranges)
        {
            if (score >= range.Min && score <= range.Max)
                return range.Genetics;
        }
        return null;
    }
}
