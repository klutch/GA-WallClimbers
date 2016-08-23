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
    private Vector3 upperBounds;
    private Vector3 lowerBounds;
    private Vector3 boundsV1;
    private Vector3 boundsV2;
    private Vector3 boundsV3;
    private Vector3 boundsV4;
    private LineRenderer lineRenderer;

    public ClimberGenetics Genetics;
    public GameObject Pin;
    public float TimeToLive = 3f;

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

        // Climb amount
        if (yDiff > 0 && grabJoint == null)
            climbAmount += yDiff;

        // Non grab time
        if (grabJoint == null)
            nonGrabTime += Time.fixedDeltaTime;

        // Average Y
        sumY += transform.position.y;
        sumYCount++;

        // Lower/upper bounds (aabb)
        boundsV1 = transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0.1f));
        boundsV2 = transform.TransformPoint(new Vector3(0.5f, 0.5f, 0.1f));
        boundsV3 = transform.TransformPoint(new Vector3(0.5f, -0.5f, 0.1f));
        boundsV4 = transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0.1f));
        lowerBounds = new Vector3(
            Mathf.Min(boundsV1.x, boundsV2.x, boundsV3.x, boundsV4.x),
            Mathf.Min(boundsV1.y, boundsV2.y, boundsV3.y, boundsV4.y),
            0.1f);
        upperBounds = new Vector3(
            Mathf.Max(boundsV1.x, boundsV2.x, boundsV3.x, boundsV4.x),
            Mathf.Max(boundsV1.y, boundsV2.y, boundsV3.y, boundsV4.y),
            0.1f);

        // Max Y
        //maxY = Mathf.Max(maxY, transform.position.y);
        maxY = Mathf.Max(maxY, upperBounds.y);

        previousPosition = transform.position;
    }

    private void DrawBounds()
    {
        Vector3 v1 = new Vector3(upperBounds.x, upperBounds.y, 0.1f);
        Vector3 v2 = new Vector3(upperBounds.x, lowerBounds.y, 0.1f);
        Vector3 v3 = new Vector3(lowerBounds.x, lowerBounds.y, 0.1f);
        Vector3 v4 = new Vector3(lowerBounds.x, upperBounds.y, 0.1f);

        lineRenderer.SetPosition(0, v1);
        lineRenderer.SetPosition(1, v2);
        lineRenderer.SetPosition(2, v3);
        lineRenderer.SetPosition(3, v4);
        lineRenderer.SetPosition(4, v1);
    }

    void Start()
    {
        bodyShapeGene = (BodyShapeGene)Genetics.Chromosome[0];
        actionGenes = new List<Gene>();
        body = GetComponent<Rigidbody>();
        lineRenderer = transform.parent.GetComponent<LineRenderer>();

        for (int i = 1; i < Genetics.Chromosome.Count; i++)
            actionGenes.Add(Genetics.Chromosome[i]);

        transform.localScale = new Vector3(bodyShapeGene.Width, bodyShapeGene.Height, 1f);
        previousPosition = transform.position;
    }

    void Update()
    {
        DrawBounds();
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
