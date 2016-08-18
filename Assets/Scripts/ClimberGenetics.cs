using UnityEngine;
using System.Collections.Generic;

public class ClimberGenetics
{
    public List<Gene> Chromosome;

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
}

public class GrabActionGene : Gene
{
    public Vector3 LocalGrabPoint;

    public GrabActionGene(Vector3 localGrabPoint)
    {
        Type = GeneType.GrabAction;
        LocalGrabPoint = localGrabPoint;
    }
}

public class ReleaseActionGene : Gene
{
    public ReleaseActionGene()
    {
        Type = GeneType.ReleaseAction;
    }
}

public class SwingActionGene : Gene
{
    public Vector3 LocalApplyAtPoint;
    public int Direction; // -1 for left, 1 for right
    public float Strength;

    public SwingActionGene(Vector3 localApplyAtPoint, int direction, float strength)
    {
        Type = GeneType.SwingAction;
        Direction = direction;
        Strength = strength;
        LocalApplyAtPoint = localApplyAtPoint;
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
}