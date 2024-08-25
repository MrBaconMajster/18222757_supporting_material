using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Unity.Burst.CompilerServices;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static UnityEditorInternal.VersionControl.ListControl;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class SpaceShooterAgent : Agent
{
    private Transform agentTransform;
    private Transform gunTransform;
    public Transform prefabTransform;
    private Agent agent;
    public GameObject exitObj;
    private Rigidbody agentRB;

    public Material winMaterial;
    public Material loseMaterial;

    public MeshRenderer topWindow;

    private float lastDistance;

    public GameObject[] enemyArr;

    private float currentSpeed = 1f;

    private int enemyCount;
    private bool inSight = false;

    RaycastHit hit;
    public float weaponRange = 10f;

    private DateTime dt_prev;
    private DateTime dt_next;

    readonly float[] prefabRotationArr = { -180f, -90f, 0f, 90f, 180f };

    //Action Space 
    //cont 0 = Speed
    //cont 1 = Tilt
    //cont 2 = RotateAgent
    //cont 3 = TiltWeapon
    //cont 4 = RotateWeapon

    public override void Initialize()
    {
        DateTime dt_prev = DateTime.Now;
        DateTime dt_next = DateTime.Now;

        agentTransform = GetComponent<Transform>();
        agent = GetComponent<Agent>();
        agentRB = GetComponent<Rigidbody>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(enemyCount);
        sensor.AddObservation(agentRB.velocity);
    }

    private float norm(float vector)
    {
        return (vector - -3.75f) / (3.75f - -3.75f);
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RequestDecision();
    }

    //Action Space 
    //cont 0 = Speed
    //cont 1 = Tilt
    //cont 2 = RotateAgent
    //cont 3 = TiltWeapon
    //cont 4 = RotateWeapon

    public override void OnActionReceived(ActionBuffers actions)
    {
        var totalReward = 0f;

        AddReward(-1f / agent.MaxStep);
        totalReward += -1f / agent.MaxStep;

        float actionSpeed = actions.ContinuousActions[0];
        float actionTilt = actions.ContinuousActions[1];
        float actionRotateAgent = actions.ContinuousActions[2];
        //float actionRotateWeaponUD = actions.ContinuousActions[3];
        //float actionRotateWeaponLR = actions.ContinuousActions[4];
        int actionFireWeapon = actions.DiscreteActions[0];

        var dirToGo = agentTransform.forward;

        currentSpeed = actions.ContinuousActions[0] + 1.001f;
        currentSpeed = currentSpeed / 8f + 0.01f;                                  

        if(actionFireWeapon == 1)
        {
            if (Physics.Raycast(transform.position, transform.forward, out hit, weaponRange))
            {
                if (hit.transform.tag == "Enemy")
                {
                    EnemyScript enemy = hit.transform.GetComponent<EnemyScript>();
                    enemy.gameObject.SetActive(false);
                    enemyCount--;
                    AddReward(0.2f);
                    totalReward += 0.2f;

                    lastDistance = getDistanceToTarget();
                    startingDistance = getDistanceToTarget();

                }
                else
                {
                 
                }
            }
        }

        //Reward if agent looking at enemies while count > 0 
      

        //Gun AIM TO BE IMPLEMENTED

        agentTransform.Rotate(new Vector3( actions.ContinuousActions[1], 0 ,0 ),
            Time.fixedDeltaTime * 150f);
        agentTransform.Rotate(0, actions.ContinuousActions[2] * (Time.fixedDeltaTime * 150f), 0, Space.World) ;

        //Clamping X-Rotation
        agentTransform.eulerAngles = new Vector3(Mathf.Clamp((transform.eulerAngles.x <= 180) ? transform.eulerAngles.x : -(360 - transform.eulerAngles.x),-80f , 80f), agentTransform.eulerAngles.y, 0);
        
        
        agentRB.AddForce(dirToGo * currentSpeed,ForceMode.VelocityChange);

        if (enemyCount > 0)
        {
            if (Physics.Raycast(transform.position, transform.forward, out hit, weaponRange))
            {
                if (inSight == false)
                {
                    if (hit.transform.tag == "Enemy")
                    {
                        
                        AddReward(0.05f);
                        totalReward += 0.05f;
                        inSight = true;
                    }
                }
                else
                {
                    if (hit.transform.tag != "Enemy")
                    {
                        
                        AddReward(-0.05f);
                        totalReward += -0.05f;
                        inSight = false;
                    }
                }
            }
        }
        else
        {
            //Door Distance reward
            var newDistance = getDistanceToTarget();
            var deltaDistance = lastDistance - newDistance;
            var deltaReward = deltaDistance / startingDistance;
            AddReward(deltaReward);
            totalReward += deltaReward;
            lastDistance = newDistance;
        }


        DateTime dt_next = DateTime.Now;

        Academy.Instance.StatsRecorder.Add("Time/Step : Millis", (float)dt_next.Subtract(dt_prev).TotalMilliseconds, StatAggregationMethod.Average);
        dt_prev = dt_next;

        totalEpisodeReward += totalReward;
        totalEpisodeSteps++;
        //Debug.Log(totalReward);

    }

    public float startingDistance;

    public float getDistanceToTarget()
    {
        return (exitObj.transform.localPosition - agentTransform.localPosition).magnitude;
    }

    private GameObject currentTarget;

    public void setTarget(GameObject obj)
    {
        currentTarget = obj;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        var continuousActionsOut = actionsOut.ContinuousActions;
        //LR RROT
        if (Input.GetKey(KeyCode.D))
        {
            continuousActionsOut[2] = 1f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            continuousActionsOut[2] = -1f;
        }

        //ACC DECC
        if (Input.GetKey(KeyCode.W))
        {
            continuousActionsOut[0] = 1f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            continuousActionsOut[0] = -1f;
        }

        if (Input.GetKey(KeyCode.W) == false && Input.GetKey(KeyCode.S) == false)
        {
            continuousActionsOut[0] = 0f;
        }

        //TILT
        if (Input.GetKey(KeyCode.E))
        {
            continuousActionsOut[1] = 1f;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            continuousActionsOut[1] = -1f;
        }


        if (Input.GetKey(KeyCode.Mouse0))
        {
            discreteActionsOut[0] = 1;
        }
        else
        {
            discreteActionsOut[0] = 0;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            continuousActionsOut[3] = 1f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            continuousActionsOut[3] = -1f;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            continuousActionsOut[4] = 1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            continuousActionsOut[4] = -1f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Door" && enemyCount == 0)
        {
            AddReward(1f);

            EndEpisode();

            Debug.Log("Goal");
            topWindow.material = winMaterial;
        }

        if (other.gameObject.tag == "Wall")
        {
            AddReward(-0.5f);

            EndEpisode();
            Debug.Log("Dead");
            topWindow.material = loseMaterial;
        }

    }

    private float totalEpisodeReward = 0;
    private int totalEpisodeSteps = 0;
    private int demoEpisode = 0;
    List<HistStats> history = new List<HistStats>();

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

        //if (demoEpisode > 50)
        //{
        //    WriteCSV(history);

        //}

        //totalEpisodeSteps = 0;
        //totalEpisodeReward = 0;
        /////////////////////////////////////////////////////////

        //Reset Env
        float x = Random.Range(-70, 70);
        float y = Random.Range(-179, 179);
        float z = Random.Range(-179, 179);

        agentRB.velocity = Vector3.zero;

        transform.rotation = Quaternion.Euler(x, y, 0f);
        transform.localPosition = new Vector3(0f, 0f, 0f);

        enemyCount = enemyArr.Length;

        //limits == 5.3
        foreach(GameObject enemy in enemyArr)
        {
            enemy.transform.localPosition = new Vector3(Random.Range(-5.3f, 5.3f), Random.Range(-5.3f, 5.3f), Random.Range(-5.3f, 5.3f));
            enemy.SetActive(true);
        }

        prefabTransform.rotation = Quaternion.Euler(prefabRotationArr[Random.Range(0, 5)], prefabRotationArr[Random.Range(0, 5)], 0f);
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
