using UnityEngine;
using System.Collections.Generic;

public class Climber : MonoBehaviour
{
    private BodyShapeGene bodyShapeGene;
    private List<Gene> actionGenes;
    private int currentActionIndex;
    private float nextActionTime;
    private HingeJoint grabJoint;
    private Rigidbody body;

    public ClimberGenetics Genetics;
    public float TimeToLive = 5f;

    private bool IsDone()
    {
        if (transform.position.y < GeneticAlgorithm.Instance.Floor)
            return true;
        else if (transform.position.y > GeneticAlgorithm.Instance.Ceiling)
            return true;
        else if (TimeToLive <= 0f)
            return true;
        return false;
    }

    private void ProcessActions()
    {
        if (nextActionTime > Time.fixedTime)
            return;

        Gene actionGene = actionGenes[currentActionIndex];

        if (actionGene.Type == GeneType.GrabAction)
        {
            GrabActionGene grabActionGene = (GrabActionGene)actionGene;

            if (grabJoint == null)
            {
                grabJoint = gameObject.AddComponent<HingeJoint>();
                grabJoint.axis = new Vector3(0f, 0f, 1f);
                grabJoint.enablePreprocessing = false;
                grabJoint.anchor = grabActionGene.LocalPoint;
            }
        }
        else if (actionGene.Type == GeneType.ReleaseAction)
        {
            if (grabJoint != null)
            {
                Destroy(grabJoint);
                grabJoint = null;
            }
        }
        else if (actionGene.Type == GeneType.SwingAction)
        {
            SwingActionGene swingActionGene = (SwingActionGene)actionGene;

            if (grabJoint != null)
            {
                Vector3 worldPosition = transform.TransformPoint(swingActionGene.LocalPoint);
                Vector3 localNormal = new Vector3((float)swingActionGene.Direction, 0f, 0f);
                Vector3 worldNormal = transform.TransformDirection(localNormal);
                Vector3 worldForce = worldNormal * swingActionGene.Strength;

                body.AddForceAtPosition(worldForce, worldPosition);
            }
        }
        else if (actionGene.Type == GeneType.NonAction)
        {
            NonActionGene nonActionGene = (NonActionGene)actionGene;

            nextActionTime = Time.fixedTime + nonActionGene.Time;
        }

        currentActionIndex = (currentActionIndex + 1) % actionGenes.Count;
    }

    void Start()
    {
        bodyShapeGene = (BodyShapeGene)Genetics.Chromosome[0];
        actionGenes = new List<Gene>();
        body = GetComponent<Rigidbody>();

        for (int i = 1; i < Genetics.Chromosome.Count; i++)
            actionGenes.Add(Genetics.Chromosome[i]);

        transform.localScale = new Vector3(bodyShapeGene.Width, bodyShapeGene.Height, 1f);
    }

    void FixedUpdate()
    {
        if (IsDone())
            GeneticAlgorithm.Instance.EndFitnessTest();
        else
        {
            ProcessActions();
            TimeToLive = Mathf.Max(TimeToLive - Time.fixedDeltaTime, 0f);
        }
    }
}
