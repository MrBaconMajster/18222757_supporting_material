using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;
using Unity.MLAgents;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    [SerializeField] GameObject m_gameObject;
    
    [SerializeField]public Transform agentTransform;

    [SerializeField] GameObject key;

    [SerializeField] FindExitAgent m_FindExitAgent;

    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //m_gameObject.transform.position = Vector3.MoveTowards(
        //m_gameObject.transform.position, agentTransform.position, 0.2f * 2 * Time.deltaTime);
        //m_gameObject.transform.LookAt(agentTransform.position);

        //agent.SetDestination(agentTransform.transform.localPosition);

    }

    private void OnTriggerEnter(Collider other)
    {
        
     
        if (other.gameObject.tag == "Weapon" )
        {

            
            //key.transform.position = m_gameObject.transform.position;
            //m_FindExitAgent.setTarget(key);
            //m_FindExitAgent.lastDistance = m_FindExitAgent.getDistanceToTarget();
            //m_FindExitAgent.startingDistance = m_FindExitAgent.getDistanceToTarget();

            //m_gameObject.SetActive(false);
            //m_FindExitAgent.seeing = false;
            //m_FindExitAgent.AddReward(0.15f);
            //key.SetActive(true);
            Debug.Log("Enemy Died Spawn Key");
        }
   

    }
}
