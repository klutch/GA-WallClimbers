using UnityEngine;
using System.Collections.Generic;

public class GeneticAlgorithm : MonoBehaviour
{
    public static GeneticAlgorithm Instance;

    private List<ClimberGenetics> population;
    private bool isRunningGenerations;
    private bool isRunningFitnessTest;
    private int currentGeneration;
    private int currentPopulationIndex;
    private Climber currentClimber;
    private Transform objectsContainer;
    private float highestFitness;
    private bool paused;

    public GameObject ClimberPrefab;
    public Vector3 SpawnPosition;
    public float CrossoverRate = 0.7f;
    public int PopulationSize = 20;
    public int NumGenerations = 100;
    public Camera Camera;
    public int MinActions = 3;
    public int MaxActions = 10;
    public float MutationRate = 0.05f;
    public float MutationStrength = 0.1f;
    public float MinNonActionTime = 0.05f;
    public float MaxNonActionTime = 2f;
    public float MinBodyWidth = 0.2f;
    public float MaxBodyWidth = 2f;
    public float MinBodyHeight = 0.2f;
    public float MaxBodyHeight = 2f;
    public float MinSwingStrength = 0.05f;
    public float MaxSwingStrength = 10f;
    public float Floor = -2f;
    public float Ceiling = 42f;

    private GeneType RandomGeneType(GeneType[] options)
    {
        return options[Random.Range(0, options.Length)];
    }

    private Gene CreateNewActionGene(ClimberGenetics genetics, Gene previousGene)
    {
        /*
        GeneType actionType = GeneType.NonAction;
        Gene actionGene = null;

        if (previousGene == null)
        {
            actionType = RandomGeneType(new[] {
                GeneType.GrabAction,
                GeneType.ReleaseAction,
                GeneType.SwingAction,
                GeneType.NonAction,
            });
        }
        else if (previousGene.Type == GeneType.GrabAction)
        {
            actionType = RandomGeneType(new[] {
                GeneType.SwingAction,
                GeneType.NonAction,
            });
        }
        else if (previousGene.Type == GeneType.ReleaseAction)
        {
            actionType = GeneType.NonAction;
        }
        else if (previousGene.Type == GeneType.SwingAction)
        {
            actionType = GeneType.NonAction;
        }
        else if (previousGene.Type == GeneType.NonAction)
        {
            actionType = RandomGeneType(new[] {
                GeneType.GrabAction,
                GeneType.ReleaseAction,
            });
        }*/

        Gene actionGene = null;
        GeneType actionType = RandomGeneType(new[] {
            GeneType.GrabAction,
            GeneType.ReleaseAction,
            GeneType.SwingAction,
            GeneType.NonAction,
        });

        if (actionType == GeneType.GrabAction)
        {
            Vector3 localPoint = Vector3.zero;
            int index = Random.Range(0, 4);

            if (index == 0)
                localPoint = new Vector3(-0.5f, -0.5f, 0f);
            else if (index == 1)
                localPoint = new Vector3(0.5f, -0.5f, 0f);
            else if (index == 2)
                localPoint = new Vector3(0.5f, 0.5f, 0f);
            else if (index == 3)
                localPoint = new Vector3(-0.5f, 0.5f, 0f);

            actionGene = new GrabActionGene(localPoint);
        }
        else if (actionType == GeneType.ReleaseAction)
        {
            actionGene = new ReleaseActionGene();
        }
        else if (actionType == GeneType.SwingAction)
        {
            BodyShapeGene bodyShapeGene = (BodyShapeGene)genetics.Chromosome[0];
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

        return actionGene;
    }

    private void CreateFirstGeneration()
    {
        population = new List<ClimberGenetics>(PopulationSize);

        for (int i = 0; i < PopulationSize; i++)
        {
            ClimberGenetics genetics = new ClimberGenetics();
            int numActions = Random.Range(MinActions, MaxActions + 1);
            BodyShapeGene bodyShapeGene = new BodyShapeGene(
                Random.Range(MinBodyWidth, MaxBodyWidth),
                Random.Range(MinBodyHeight, MaxBodyHeight));
            Gene previousAction = null;

            genetics.Chromosome.Add(bodyShapeGene);

            for (int j = 0; j < numActions; j++)
            {
                Gene actionGene = CreateNewActionGene(genetics, previousAction);

                previousAction = actionGene;
                genetics.Chromosome.Add(actionGene);
            }

            population.Add(genetics);
        }

        Logger.Add("Created random genetic information for a population of " + population.Count);
    }

    private void CreateNextGeneration()
    {
        List<ClimberGenetics> newPopulation = new List<ClimberGenetics>(PopulationSize);
        List<ClimberGenetics> sourcePopulation = new List<ClimberGenetics>(population);
        float averageFitness = 0f;
        float highestGenFitness = 0f;

        foreach (ClimberGenetics genetics in population)
        {
            averageFitness += genetics.FitnessScore;
            highestGenFitness = Mathf.Max(highestGenFitness, genetics.FitnessScore);
        }
        averageFitness /= population.Count;

        while (newPopulation.Count < PopulationSize)
        {
            ClimberGenetics a = SelectGenetics(sourcePopulation);
            ClimberGenetics b = SelectGenetics(sourcePopulation);
            bool crossover = Random.Range(0f, 1f) < CrossoverRate;

            if (crossover)
            {
                int maxCrossoverIndex = Mathf.Min(a.Chromosome.Count, b.Chromosome.Count) - 1;
                int crossoverIndex = Random.Range(0, maxCrossoverIndex + 1);
                int maxIndex = Mathf.Max(a.Chromosome.Count, b.Chromosome.Count);
                List<Gene> aChromosome = new List<Gene>(a.Chromosome);
                List<Gene> bChromosome = new List<Gene>(b.Chromosome);

                a.Chromosome.Clear();
                b.Chromosome.Clear();

                for (int i = 0; i < maxIndex; i++)
                {
                    if (i <= crossoverIndex)
                    {
                        a.Chromosome.Add(aChromosome[i]);
                        b.Chromosome.Add(bChromosome[i]);
                    }
                    else
                    {
                        if (i < bChromosome.Count)
                            a.Chromosome.Add(bChromosome[i]);
                        if (i < aChromosome.Count)
                            b.Chromosome.Add(aChromosome[i]);
                    }
                }
            }

            foreach (Gene gene in a.Chromosome)
            {
                if (Random.Range(0f, 1f) < MutationRate)
                    MutateGene(a, gene);
            }
            foreach (Gene gene in b.Chromosome)
            {
                if (Random.Range(0f, 1f) < MutationRate)
                    MutateGene(b, gene);
            }

            newPopulation.Add(a);
            newPopulation.Add(b);
        }

        Logger.Add("Average fitness for generation " + currentGeneration + ": " + averageFitness);
        Logger.Add("Highest fitness for generation " + currentGeneration + ": " + highestGenFitness);
        Logger.Add("Highest all-time fitness: " + highestFitness);

        currentGeneration++;
        population.Clear();
        population = newPopulation;
        Logger.Add("Created new genetic information for generation " + currentGeneration);
    }

    private ClimberGenetics SelectGenetics(List<ClimberGenetics> source)
    {
        RouletteWheel rouletteWheel = new RouletteWheel(source);
        ClimberGenetics result;
        float sumScores = 0;
        float randScore = 0;

        foreach (ClimberGenetics genetics in source)
            sumScores += genetics.FitnessScore;

        randScore = Random.Range(0f, sumScores);
        result = rouletteWheel.GetResult(randScore);
        source.Remove(result);

        return result;
    }

    private void MutateGene(ClimberGenetics genetics, Gene gene)
    {
        if (gene.Type == GeneType.BodyShape)
        {
            BodyShapeGene bodyShapeGene = (BodyShapeGene)gene;

            bodyShapeGene.Width += bodyShapeGene.Width * Random.Range(-MutationStrength, MutationStrength);
            bodyShapeGene.Height += bodyShapeGene.Height * Random.Range(-MutationStrength, MutationStrength);
        }
        else if (gene.Type == GeneType.GrabAction)
        {
            if (Random.Range(0f, 1f) < MutationStrength)
            {
                GrabActionGene grabActionGene = (GrabActionGene)gene;
                BodyShapeGene bodyShapeGene = (BodyShapeGene)genetics.Chromosome[0];
                Vector3 localPoint = Vector3.zero;
                int index = Random.Range(0, 4);

                if (index == 0)
                    localPoint = new Vector3(-0.5f, -0.5f, 0f);
                else if (index == 1)
                    localPoint = new Vector3(0.5f, -0.5f, 0f);
                else if (index == 2)
                    localPoint = new Vector3(0.5f, 0.5f, 0f);
                else if (index == 3)
                    localPoint = new Vector3(-0.5f, 0.5f, 0f);

                grabActionGene.LocalPoint = localPoint;
            }
        }
        else if (gene.Type == GeneType.ReleaseAction)
        {
            // No properties to mutate
        }
        else if (gene.Type == GeneType.SwingAction)
        {
            SwingActionGene swingActionGene = (SwingActionGene)gene;
            BodyShapeGene bodyShapeGene = (BodyShapeGene)genetics.Chromosome[0];
            float localX = swingActionGene.LocalPoint.x + swingActionGene.LocalPoint.x * Random.Range(-MutationStrength, MutationStrength);
            float localY = swingActionGene.LocalPoint.y + swingActionGene.LocalPoint.y * Random.Range(-MutationStrength, MutationStrength);

            swingActionGene.LocalPoint = new Vector3(
                Mathf.Clamp(localX, -bodyShapeGene.Width * 0.5f, bodyShapeGene.Width * 0.5f),
                Mathf.Clamp(localY, -bodyShapeGene.Height * 0.5f, bodyShapeGene.Height * 0.5f),
                swingActionGene.LocalPoint.z);

            if (Random.Range(0f, 1f) < MutationStrength)
                swingActionGene.Direction = Random.Range(0, 2) == 0 ? -1 : 1;

            swingActionGene.Strength = Mathf.Clamp(
                swingActionGene.Strength + swingActionGene.Strength * Random.Range(-MutationStrength, MutationStrength),
                MinSwingStrength,
                MaxSwingStrength);
        }
        else if (gene.Type == GeneType.NonAction)
        {
            NonActionGene nonActionGene = (NonActionGene)gene;

            nonActionGene.Time = Mathf.Clamp(
                nonActionGene.Time + nonActionGene.Time * Random.Range(-MutationStrength, MutationStrength),
                MinNonActionTime,
                MaxNonActionTime);
        }
    }

    private void StartFitnessTest()
    {
        GameObject climberObj;
        Climber climber;
        ClimberGenetics genetics = population[currentPopulationIndex];
        BodyShapeGene bodyShapeGene = (BodyShapeGene)genetics.Chromosome[0];

        isRunningFitnessTest = true;

        climberObj = Instantiate<GameObject>(ClimberPrefab);
        climberObj.transform.parent = objectsContainer;
        climberObj.transform.position = SpawnPosition - new Vector3(0f, bodyShapeGene.Height * 0.5f, 0f);
        climber = climberObj.GetComponentInChildren<Climber>();
        climber.Genetics = genetics;
        currentClimber = climber;
    }

    private float GetFitnessScore(Climber climber)
    {
        float climbScore = Mathf.Pow(climber.ClimbAmount * 10f, 1.2f);
        float nonGrabTimeScore = Mathf.Pow(climber.NonGrabTime * 10f, 1.2f);
        float maxYScore = Mathf.Pow(climber.MaxY, 2.5f);
        float score = climbScore + nonGrabTimeScore + maxYScore;

        highestFitness = Mathf.Max(highestFitness, score);
        return score;
    }

    public void EndFitnessTest()
    {
        float score = GetFitnessScore(currentClimber);

        Logger.Add("[" + currentPopulationIndex + "] fitness: " + (int)score);
        isRunningFitnessTest = false;
        currentClimber.Genetics.FitnessScore = score;
        Destroy(currentClimber.transform.parent.gameObject);
        currentClimber = null;
        currentPopulationIndex++;
    }

    private void HandleInput()
    {
        if (!isRunningGenerations && Input.GetKeyDown(KeyCode.Return))
        {
            isRunningGenerations = true;
            CreateFirstGeneration();
            StartFitnessTest();
        }

        if (isRunningGenerations && !paused)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                paused = true;
                Time.timeScale = 0f;
                Logger.Add("Genes for climber [" + currentPopulationIndex + "]");

                foreach (Gene gene in currentClimber.Genetics.Chromosome)
                    Logger.Add("   " + gene.ToString());
            }
        }
        else if (isRunningFitnessTest && paused)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                paused = false;
                Time.timeScale = 1f;
            }
        }
    }

    void Start()
    {
        Instance = this;
        objectsContainer = GameObject.FindGameObjectWithTag("Objects").transform;
        Application.runInBackground = true;
        Logger.Add("Press enter to begin simulation");
    }

    void Update()
    {
        HandleInput();

        if (isRunningGenerations)
        {
            if (!isRunningFitnessTest)
            {
                if (currentPopulationIndex < population.Count)
                {
                    StartFitnessTest();
                }
                else
                {
                    if (currentGeneration < NumGenerations)
                        CreateNextGeneration();
                    else
                        isRunningGenerations = false;

                    foreach (ClimberGenetics genetics in population)
                        genetics.FitnessScore = 0;
                    currentPopulationIndex = 0;
                }
            }
        }
    }
}
