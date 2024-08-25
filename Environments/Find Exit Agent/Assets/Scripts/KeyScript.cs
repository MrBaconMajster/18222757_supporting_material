using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyScript : MonoBehaviour
{

    [SerializeField] GameObject m_gameObject;
    [SerializeField] FindExitAgent m_FindExitAgent;
    [SerializeField] GameObject exitObj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
       

        if (other.gameObject.tag == "Use")
        {
            //m_FindExitAgent.setTarget(exitObj);
            //m_FindExitAgent.lastDistance = m_FindExitAgent.getDistanceToTarget();
            //m_FindExitAgent.startingDistance = m_FindExitAgent.getDistanceToTarget();
            //m_FindExitAgent.keyObtained = true;
            //m_gameObject.SetActive(false);
            //m_FindExitAgent.seeing = false;
            
           
            //m_FindExitAgent.AddReward(0.15f);
            
            
            
            Debug.Log("Key Picked up");
        }


    }
}
