using UnityEngine;
using System.Collections.Generic;

public class GeneticAlgorithm : MonoBehaviour
{
    private List<ClimberGenetics> Population;

    public Vector3 SpawnPosition;
    public float CrossoverRate = 0.7f;
    public int StartingPopulationSize = 20;
    public int NumGenerations = 100;
    public Camera Camera;
    public int MinActions = 3;
    public int MaxActions = 10;
    public float BodyShapeMutationRate = 0.05f;
    public float GrabActionMutationRate = 0.05f;
    public float ReleaseActionMutationRate = 0.05f;
    public float SwingActionMutationRate = 0.05f;
    public float NonActionMutationRate = 0.05f;
    public float MinNonActionTime = 0.05f;
    public float MaxNonActionTime = 2f;
    public float MinBodyWidth = 0.2f;
    public float MaxBodyWidth = 2f;
    public float MinBodyHeight = 0.2f;
    public float MaxBodyHeight = 2f;
    public float MinSwingStrength = 0.05f;
    public float MaxSwingStrength = 10f;

    private void CreateRandomPopulation()
    {
        Population = new List<ClimberGenetics>(StartingPopulationSize);

        for (int i = 0; i < StartingPopulationSize; i++)
        {
            ClimberGenetics genetics = new ClimberGenetics();
            int numActions = Random.Range(MinActions, MaxActions + 1);
            BodyShapeGene bodyShapeGene = new BodyShapeGene(
                Random.Range(MinBodyWidth, MaxBodyWidth),
                Random.Range(MinBodyHeight, MaxBodyHeight));

            genetics.Chromosome.Add(bodyShapeGene);

            for (int j = 0; j < numActions; j++)
            {
                GeneType actionType = (GeneType)Random.Range(1, 5);
                Gene actionGene = null;

                if (actionType == GeneType.GrabAction)
                {
                    Vector3 localGrabPoint = new Vector3(
                        Random.Range(-bodyShapeGene.Width * 0.5f, bodyShapeGene.Width * 0.5f),
                        Random.Range(-bodyShapeGene.Height * 0.5f, bodyShapeGene.Height * 0.5f),
                        0f);

                    actionGene = new GrabActionGene(localGrabPoint);
                }
                else if (actionType == GeneType.ReleaseAction)
                {
                    actionGene = new ReleaseActionGene();
                }
                else if (actionType == GeneType.SwingAction)
                {
                    Vector3 localApplyAtPoint = new Vector3(
                        Random.Range(-bodyShapeGene.Width * 0.5f, bodyShapeGene.Width * 0.5f),
                        Random.Range(-bodyShapeGene.Height * 0.5f, bodyShapeGene.Height * 0.5f),
                        0f);
                    int direction = Random.Range(0, 2) == 0 ? -1 : 1;
                    float strength = Random.Range(MinSwingStrength, MaxSwingStrength);

                    actionGene = new SwingActionGene(localApplyAtPoint, direction, strength);
                }
                else if (actionType == GeneType.NonAction)
                {
                    actionGene = new NonActionGene(Random.Range(MinNonActionTime, MaxNonActionTime));
                }

                genetics.Chromosome.Add(actionGene);
            }

            Population.Add(genetics);
        }

        Logger.Add("Created random genetic information for a population of " + Population.Count);
    }

    void Start()
    {
        CreateRandomPopulation();
    }

    void Update()
    {
    }
}
