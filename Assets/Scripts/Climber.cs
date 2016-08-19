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
    private float maxY = 0;
    private float sumY = 0;
    private int sumYCount = 0;
    private float climbAmount;
    private Vector3 previousPosition;
    private float nonGrabTime;

    public ClimberGenetics Genetics;
    public GameObject Pin;
    public float TimeToLive = 5f;

    public float MaxY { get { return maxY; } }
    public float AverageY { get { return sumY / (float)sumYCount; } }
    public float ClimbAmount { get { return climbAmount; } }
    public float NonGrabTime { get { return nonGrabTime; } }

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

            if (grabJoint == null && transform.position.y > 0)
            {
                grabJoint = gameObject.AddComponent<HingeJoint>();
                grabJoint.axis = new Vector3(0f, 0f, 1f);
                grabJoint.enablePreprocessing = false;
                grabJoint.anchor = grabActionGene.LocalPoint;
                Pin.SetActive(true);
                Pin.transform.position = transform.TransformPoint(grabActionGene.LocalPoint);
            }
        }
        else if (actionGene.Type == GeneType.ReleaseAction)
        {
            if (grabJoint != null)
            {
                Destroy(grabJoint);
                grabJoint = null;
                Pin.SetActive(false);
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

    private void TrackStatus()
    {
        float yDiff = transform.position.y - previousPosition.y;

        if (yDiff > 0 && grabJoint == null)
            climbAmount += yDiff;

        if (grabJoint == null)
            nonGrabTime += Time.fixedDeltaTime;

        maxY = Mathf.Max(maxY, transform.position.y);

        sumY += transform.position.y;
        sumYCount++;

        previousPosition = transform.position;
    }

    void Start()
    {
        bodyShapeGene = (BodyShapeGene)Genetics.Chromosome[0];
        actionGenes = new List<Gene>();
        body = GetComponent<Rigidbody>();

        for (int i = 1; i < Genetics.Chromosome.Count; i++)
            actionGenes.Add(Genetics.Chromosome[i]);

        transform.localScale = new Vector3(bodyShapeGene.Width, bodyShapeGene.Height, 1f);
        previousPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (IsDone())
            GeneticAlgorithm.Instance.EndFitnessTest();
        else
        {
            TrackStatus();
            ProcessActions();
            TimeToLive = Mathf.Max(TimeToLive - Time.fixedDeltaTime, 0f);
        }
    }
}
