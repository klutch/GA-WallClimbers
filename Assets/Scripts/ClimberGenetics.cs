using UnityEngine;
using System.Collections.Generic;

public class ClimberGenetics
{
    public List<Gene> Chromosome;
    public float FitnessScore;

    public ClimberGenetics()
    {
        Chromosome = new List<Gene>();
    }
}

public enum GeneType
{
    BodyShape,
    GrabAction,
    ReleaseAction,
    SwingAction,
    NonAction,
}

public abstract class Gene
{
    public GeneType Type;
}

public class BodyShapeGene : Gene
{
    public float Width;
    public float Height;

    public BodyShapeGene(float width, float height)
    {
        Type = GeneType.BodyShape;
        Width = width;
        Height = height;
    }

    public override string ToString()
    {
        return "BodyShapeGene - Width: " + Width + ", Height: " + Height;
    }
}

public class GrabActionGene : Gene
{
    public Vector3 LocalPoint;

    public GrabActionGene(Vector3 localGrabPoint)
    {
        Type = GeneType.GrabAction;
        LocalPoint = localGrabPoint;
    }

    public override string ToString()
    {
        return "GrabActionGene - LocalPoint: " + LocalPoint;
    }
}

public class ReleaseActionGene : Gene
{
    public ReleaseActionGene()
    {
        Type = GeneType.ReleaseAction;
    }

    public override string ToString()
    {
        return "ReleaseActionGene";
    }
}

public class SwingActionGene : Gene
{
    public Vector3 LocalPoint;
    public int Direction; // -1 for left, 1 for right
    public float Strength;

    public SwingActionGene(Vector3 localPoint, int direction, float strength)
    {
        Type = GeneType.SwingAction;
        Direction = direction;
        Strength = strength;
        LocalPoint = localPoint;
    }

    public override string ToString()
    {
        return "SwingActionGene - LocalPoint: " + LocalPoint +
            ", Direction: " + (Direction == -1 ? "left" : "right") +
            ", Strength: " + Strength;
    }
}

public class NonActionGene : Gene
{
    public float Time;

    public NonActionGene(float time)
    {
        Type = GeneType.NonAction;
        Time = time;
    }

    public override string ToString()
    {
        return "NonActionGene - Time: " + Time;
    }
}