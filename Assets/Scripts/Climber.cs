using UnityEngine;
using System.Collections.Generic;

public class Climber : MonoBehaviour
{
    private BodyShapeGene bodyShapeGene;
    private List<Gene> actionGenes;
    private int currentActionIndex;
    private float nextActionTime;
    private float timeToLive = 10f;

    public ClimberGenetics Genetics;

    public float TimeToLive { get { return timeToLive; } }

    private bool IsDone()
    {
        if (transform.position.y < GeneticAlgorithm.Instance.Floor)
            return true;
        else if (transform.position.y > GeneticAlgorithm.Instance.Ceiling)
            return true;
        else if (timeToLive < 0f)
            return true;
        return false;
    }

    void Start()
    {
        bodyShapeGene = (BodyShapeGene)Genetics.Chromosome[0];
        actionGenes = new List<Gene>();

        for (int i = 1; i < Genetics.Chromosome.Count; i++)
            actionGenes.Add(Genetics.Chromosome[i]);

        transform.localScale = new Vector3(bodyShapeGene.Width, bodyShapeGene.Height, 1f);
    }

    void FixedUpdate()
    {
        if (IsDone())
            GeneticAlgorithm.Instance.EndFitnessTest();
        else
            timeToLive = Mathf.Max(timeToLive - Time.fixedDeltaTime, 0f);
    }
}
