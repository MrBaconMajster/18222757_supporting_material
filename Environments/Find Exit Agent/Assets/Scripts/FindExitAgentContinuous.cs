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
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class FindExitAgentContinuous : Agent
{

    public Transform enemyTransform;
    public Transform prefabTransform;
    public MeshRenderer platform;
    public GameObject swordObj;
    public GameObject keyObj;
    public GameObject exitObj;
    public GameObject useObj;
    public GameObject enemyObj;
    public Material winMaterial;
    public Material loseMaterial;
    public Material defaultMaterial;

    RaycastHit hit;
    float maxRange = 10f;

    public float lastDistance;
    private Agent agent;

    public Rigidbody agentRB;
    private Transform transform;

    public float agentRunSpeed;
    public float agentRotationSpeed;
    public float agentJumpForce;
    public float agentJumpCoolDown;

    //private float keyNotObtainedPenalty = 0.01f;

    public bool keyObtained = false;
    public bool seeing = false;

    RayPerceptionSensorComponent3D raycast;

    private DateTime dt_prev;
    private DateTime dt_next;

    //Action Space
    const int noAction = 0;
    const int forwardAction = 1;
    const int backwardAction = 2;
    const int leftAction = 3;
    const int rightAction = 4;
    const int useAction = 5;
    const int attackAction = 6;

    readonly float[] prefabRotationArr = { -180f, -90f, 0f, 90f, 180f };

    public override void Initialize()
    {
        DateTime dt_prev = DateTime.Now;
        DateTime dt_next = DateTime.Now;

        transform = GetComponent<Transform>();
        agent = GetComponent<Agent>();
        raycast = GetComponent<RayPerceptionSensorComponent3D>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Adding observation
        
        //Debug.Log(exitObj.transform.localPosition);


        float normX = norm(transform.localPosition.x);
        float normZ = norm(transform.localPosition.z);
        //sensor.AddObservation(normX);
        //sensor.AddObservation(normZ);
    
        Vector3 normalized = transform.localRotation.eulerAngles / 360.0f;  // [0,1]
        sensor.AddObservation(agentRB.velocity);
        //sensor.AddObservation(normalized.y);

        sensor.AddObservation(keyObtained);
        
        //sensor.AddObservation(norm(exitObj.transform.localPosition.x));
        //sensor.AddObservation(norm(exitObj.transform.localPosition.z));

    }

    private float norm(float vector)
    {
        return (vector - -3.75f) / (3.75f - -3.75f);
    }

    // Start is called before the first frame update
    void Start()
    {
        agentRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RequestDecision();
    }

    private IEnumerator attackCoroutine;

    private IEnumerator attack(float waitTime)
    {
        swordObj.SetActive(true);
        yield return new WaitForSeconds(waitTime);
        swordObj.SetActive(false);
    }

    private IEnumerator useCoroutine;

    private IEnumerator use(float waitTime)
    {
        useObj.SetActive(true);
        yield return new WaitForSeconds(waitTime);
        useObj.SetActive(false);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        totalEpisodeSteps += 1;
        var totalReward = 0f;
        int action = actions.DiscreteActions[0];

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        AddReward(-1f / agent.MaxStep);
        totalReward +=  -1f / agent.MaxStep;

        float actionSpeed = actions.ContinuousActions[0];
        float actionTilt = actions.ContinuousActions[1];

        //var newDistance =

        if(actionSpeed > 0f && actionSpeed != 0f)
        {
            dirToGo = transform.forward * 1f;
        }
        else if(actionSpeed != 0f)
        {
            dirToGo = transform.forward * -1f;
        }

        if (actionTilt > 0f && actionTilt != 0f)
        {
            rotateDir = transform.up * 1f;
        }
        else if (actionTilt != 0f)
        {
            rotateDir = transform.up * -1f;
        }


        switch (action)
        {
            case 1:
                // Pickup Key
                useCoroutine = use(0.1f);
                if (keyObj.activeInHierarchy)
                {
                    Vector3 targetDir = keyObj.transform.position - transform.position;
                    float angle = Vector3.SignedAngle(targetDir, transform.forward, Vector3.up);
                    float distToKey = (keyObj.transform.localPosition - transform.localPosition).magnitude;
                    if ((angle < 60f && angle > -60f) && distToKey < 1f)
                    {
                        keyObtained = true;
                        setTarget(exitObj);
                        keyObj.SetActive(false);
                        seeing = false;
                        lastDistance = getDistanceToTarget();
                        startingDistance = getDistanceToTarget();

                        
                        AddReward(0.15f);
                        totalReward += 0.15f;
                    }
                }
                StartCoroutine(useCoroutine);
                break;

            case 2:
                attackCoroutine = attack(0.15f);
                if (enemyObj.activeInHierarchy)
                {
                    Vector3 targetDir = enemyObj.transform.position - transform.position;
                    float angle = Vector3.SignedAngle(targetDir, transform.forward, Vector3.up);
                    float distToKey = (enemyObj.transform.localPosition - transform.localPosition).magnitude;
                    if ((angle < 60f && angle > -60f) && distToKey < 1f)
                    {
                        keyObj.transform.localPosition = enemyObj.transform.localPosition;
                        setTarget(keyObj);
                        keyObj.SetActive(true);
                        lastDistance = getDistanceToTarget();
                        startingDistance = getDistanceToTarget();

                        enemyObj.SetActive(false);
                        seeing = false;
                        AddReward(0.15f);
                        totalReward += 0.15f;
                    }
                }
                StartCoroutine(attackCoroutine);
                break;
        }

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        agentRB.AddForce(dirToGo * agentRunSpeed,
            ForceMode.VelocityChange);


        if (enemyObj.activeInHierarchy == true)
        {
            if (Vector3.Distance(transform.position, enemyObj.transform.position) < maxRange)
            {
                if (Physics.Raycast(transform.position, (enemyObj.transform.position - transform.position), out hit, maxRange))
                {
                    if (hit.transform == enemyObj.transform)
                    {
                        // In Range and i can see you!
                        if (seeing == false)
                        {
                
                            AddReward(0.1f);
                            totalReward += 0.1f;
                            seeing = true;
                        }
                    }
                    else
                    {
                        if (seeing == true)
                        {
                            
                            AddReward(-0.1f);
                            totalReward += -0.1f;
                            seeing = false;
                        }
                    }
                }
            }
        }
        else if (keyObtained == false)
        {
            if (Vector3.Distance(transform.position, keyObj.transform.position) < maxRange)
            {
                
                if (Physics.Raycast(transform.position, (keyObj.transform.position - transform.position), out hit, maxRange))
                {
                    
                    if (hit.transform == keyObj.transform)
                    {
                        // In Range and i can see you!
                        if (seeing == false)
                        {

              
                            AddReward(0.1f);
                            totalReward += 0.1f;
                            seeing = true;
                        }
                    }
                    else
                    {
                        if (seeing == true)
                        {

                            
                            AddReward(-0.1f);
                            totalReward += -0.1f;
                            seeing = false;
                        }

                    }
                    //AddReward(0.101f - keyNotObtainedPenalty);
                    //keyNotObtainedPenalty = keyNotObtainedPenalty + 0.01f;
                }
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, exitObj.transform.position) < maxRange)
            {

                if (Physics.Raycast(transform.position, (exitObj.transform.position - transform.position), out hit, maxRange))
                {
      
                    if (hit.transform.parent == exitObj.transform)
                    {
                 
                        // In Range and i can see you!
                        if (seeing == false)
                        {

                            AddReward(0.1f);
                            totalReward += 0.1f;
                            seeing = true;
                        }
                    }
                    else
                    {
                        if (seeing == true)
                        {
                            AddReward(-0.1f);
                            totalReward += 0.1f;
                            seeing = false;
                        }

                    }
                }
            }
        }

        //Distance reward to target
        if (enemyObj.activeInHierarchy == false)
        {
            var newDistance = getDistanceToTarget();
            var deltaDistance = lastDistance - newDistance;
            var deltaReward = deltaDistance / startingDistance;
            AddReward(deltaReward);
            totalReward += deltaReward;
            lastDistance = newDistance;
            //Debug.Log(deltaReward);
        }
        
        DateTime dt_next = DateTime.Now;

        Academy.Instance.StatsRecorder.Add("Time/Step : Millis", (float)dt_next.Subtract(dt_prev).TotalMilliseconds, StatAggregationMethod.Average);
        dt_prev = dt_next;


        Debug.Log(totalReward);
        totalEpisodeReward += totalReward;

    }

    public float startingDistance;

    public float getDistanceToTarget()
    {
        return (currentTarget.transform.localPosition - transform.localPosition).magnitude;
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
        if (Input.GetKey(KeyCode.D))
        {
            continuousActionsOut[1] = 1f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            continuousActionsOut[1] = -1f;
        }
        else
        {
            continuousActionsOut[1] = 0f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            continuousActionsOut[0] = 1f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            continuousActionsOut[0] = -1f;
        }
        else
        {
            continuousActionsOut[0] = 0f;
        }


        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            discreteActionsOut[0] = 2;
        }
        else
        {
            discreteActionsOut[0] = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Door" && keyObtained == true)
        {
            AddReward(1f);
      
            EndEpisode();

   
            platform.material = winMaterial;
        }
        if (other.gameObject.tag == "Enemy" )
        {
            AddReward(-0.4f);
            
            EndEpisode();
        
            platform.material = loseMaterial;
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
        Debug.Log("Episode : " + demoEpisode);
        //// Adding Reward, Steps
        HistStats histStats = new HistStats();
        histStats.episodeNumber = demoEpisode;
        histStats.totalSteps = totalEpisodeSteps;
        histStats.totalReward = totalEpisodeReward;

        history.Add(histStats);

        if (demoEpisode > 50)
        {
            WriteCSV(history);

        }

        totalEpisodeSteps = 0;
        totalEpisodeReward = 0;
        /////////////////////////////////////////////////////////


        //Reset Env

        keyObj.SetActive(false);
        swordObj.SetActive(false);
        useObj.SetActive(false);
        enemyObj.SetActive(true);
        seeing = false;
        keyObtained = false;

        //Random rotation for agent
        float x = Random.Range(-179, 179);
        transform.rotation = Quaternion.Euler(0f, x, 0f);
        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        agentRB.velocity = Vector3.zero;
        //Random Position Enemy
        x = Random.Range(-3.5f, 3.5f);
        float z = Random.Range(-3.3f, -2.3f);
        int random = Random.Range(0, 2);
        if (random == 0) z = z * -1;
        enemyTransform.localPosition = new Vector3(x, 0.5f, z);
        setTarget(enemyObj);
        //Randomise door
        x = Random.Range(-3f, 3f);
        exitObj.transform.localPosition = new Vector3(x, 0.62f, 3.95f);
        prefabTransform.rotation = Quaternion.Euler(0f, prefabRotationArr[Random.Range(0,5)], 0f);

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

