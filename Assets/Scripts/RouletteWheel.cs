using UnityEngine;
using System.Collections.Generic;

public class RouletteWheel
{
    public List<RouletteWheelRange> Ranges;

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

    public RouletteWheel(List<ClimberGenetics> source)
    {
        float scoreCounter = 0;

        Ranges = new List<RouletteWheelRange>();

        foreach (ClimberGenetics genetics in source)
        {
            float min = scoreCounter;
            float max = scoreCounter + genetics.FitnessScore;

            Ranges.Add(new RouletteWheelRange(min, max, genetics));
            scoreCounter = max;
        }
    }

    public ClimberGenetics GetResult(float score)
    {
        foreach (RouletteWheelRange range in Ranges)
        {
            if (score >= range.Min && score <= range.Max)
                return range.Genetics;
        }
        return null;
    }
}
