using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

public class WalkToGoal : Agent
{

    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform penaltyTransform;
    [SerializeField] private MeshRenderer platform;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;


    public float timeBetweenDecisionsAtInference;
    float m_TimeSinceDecision;
    private DateTime dt_prev;
    private DateTime dt_next;

    // Start is called before the first frame update
    private Rigidbody rBody;
    private Vector3 startingPosition;

    // Action Space
    const int no_Action = 0;
    const int action_Up = 1;
    const int action_Down = 2;
    const int action_Left = 3;
    const int action_Right = 4;

    private float totalEpisodeReward = 0;
    private int totalEpisodeSteps = 0;
    private int demoEpisode = 0;
    List<HistStats> history = new List<HistStats>();

    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        startingPosition = transform.position;
        DateTime dt_prev = DateTime.Now;
        DateTime dt_next = DateTime.Now;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-0.1f);
        totalEpisodeReward += -0.1f;
        totalEpisodeSteps += 1;
        float action = actions.DiscreteActions[0];
       
        if (action == action_Up)
        {
            if (transform.localPosition.z != 2)
            {
                transform.localPosition += new Vector3(0, 0, 1);
            }
        }
        else if (action == action_Down)
        {
            if (transform.localPosition.z != -2)
            {
                transform.localPosition += new Vector3(0, 0, -1);
            }
        }
        else if (action == action_Left)
        {
            if (transform.localPosition.x != -2)
            {
                transform.localPosition += new Vector3(-1, 0, 0);
            }
        }
        else if (action == action_Right)
        {
            if (transform.localPosition.x != 2)
            {
                transform.localPosition += new Vector3(1, 0, 0);
            }
        }

        DateTime dt_next = DateTime.Now;
  
        Academy.Instance.StatsRecorder.Add("Time/Step : Millis", (float)dt_next.Subtract(dt_prev).TotalMilliseconds, StatAggregationMethod.Average);
        dt_prev = dt_next;
       

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {   
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = no_Action;

        if (Input.GetKey("up"))
        {
            discreteActions[0] = action_Up;
        }
        else if (Input.GetKey("down"))
        {
            discreteActions[0] = action_Down;
        }
        else if (Input.GetKey("left"))
        {
            discreteActions[0] = action_Left;
        }
        else if (Input.GetKey("right"))
        {
            discreteActions[0] = action_Right;
        }

        // Pathfinding for demo
        //if(transform.localPosition.x != targetTransform.localPosition.x)
        //{
        //    if(transform.localPosition.x > targetTransform.localPosition.x)
        //    {
        //        discreteActions[0] = action_Left;
        //    }else
        //    {
        //        discreteActions[0] = action_Right;
        //    }
        //}
        //else
        //{
        //    if (transform.localPosition.z > targetTransform.localPosition.z)
        //    {
        //        discreteActions[0] = action_Down;
        //    }
        //    else
        //    {
        //        discreteActions[0] = action_Up;
        //    }
        //}

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
        sensor.AddObservation(penaltyTransform.localPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<GoalGrid>(out GoalGrid goal))
        {
            AddReward(1f);
            totalEpisodeReward += 1f;
            EndEpisode();
            platform.material = winMaterial;
            
        }
        if (other.TryGetComponent<Penalty>(out Penalty wall))
        {
            AddReward(-1f);
            totalEpisodeReward += -1f;
            EndEpisode();
            platform.material = loseMaterial;
            
        }

    }


    public override void OnEpisodeBegin()
    {
        //////FOR DEMO RECORDING ONLY/////////////////////////////////

        demoEpisode = demoEpisode + 1;
        //Debug.Log("Episode : " + demoEpisode);
        ////// Adding Reward, Steps
        //HistStats histStats = new HistStats();
        //histStats.episodeNumber = demoEpisode;
        //histStats.totalSteps = totalEpisodeSteps;
        //histStats.totalReward = totalEpisodeReward;

        //history.Add(histStats);

        //if (demoEpisode > 1500)
        //{
        //    WriteCSV(history);

        //}
        /////////////////////////////////////////////////////////

        //Resetting Historical Data
        totalEpisodeSteps = 0;
        totalEpisodeReward = 0;
        //Randomise placements
        int x = (int)Random.Range(-2, 2);
        int y = (int)Random.Range(-2, 2);
        transform.localPosition = new Vector3(x, 0.25f, y);

        bool done = false;

        int x1 = (int)Random.Range(-2, 2);
        int y1 = (int)Random.Range(-2, 2);
        while (!done)
        {
            if (x1 != x && y1 != y)
            {
                targetTransform.localPosition = new Vector3(x1, 0.25f, y1);
                done = true;
            }
            else
            {
                x1 = (int)Random.Range(-2, 2);
                y1 = (int)Random.Range(-2, 2);
            }
        }
        int x2 = (int)Random.Range(-2, 2);
        int y2 = (int)Random.Range(-2, 2);

        done = false;

        while (!done)
        {
            if ((x2 != x && y2 != y) && (x2 != x1 && y2 != y1))
            {
                penaltyTransform.localPosition = new Vector3(x2, 0.25f, y2);
                done = true;
            }
            else
            {
                x2 = (int)Random.Range(-2, 2);
                y2 = (int)Random.Range(-2, 2);
            }
        }
    }

    public void FixedUpdate()
    {
        WaitTimeInference();
    }

    void WaitTimeInference()
    {

        if (m_TimeSinceDecision >= timeBetweenDecisionsAtInference)
        {
            m_TimeSinceDecision = 0f;
            RequestDecision();
        }
        else
        {
            m_TimeSinceDecision += Time.fixedDeltaTime;
        }

    }



    string filename = Application.dataPath + "/demoStats.csv";

    // Start is called before the first frame update

    public class HistStats
    {
        public int episodeNumber;
        public float totalReward;
        public float totalSteps;
    }

    // Update is called once per frame

    public void WriteCSV(List<HistStats> x)
    {
        TextWriter tw = new StreamWriter(filename, false);
        tw.WriteLine("Episode Number, Total Reward, Total Steps, ");
        tw.Close();

        tw = new StreamWriter(filename, true);

        foreach (HistStats item in x)
        {
            tw.WriteLine(item.episodeNumber + "," + item.totalReward + "," + item.totalSteps);
        }
        tw.Close();
    }

}