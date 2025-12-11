using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float fanDistanceSound;
    //FirstPersonController player; = FindObjectOfType<FirstPersonController>();
    //Transform playerTransform = player.transform;
    //Transform playerTransform;
    //GameObject player = GameObject.FindGameObjectWithTag("Player");

    public AK.Wwise.RTPC weatherRTPC;

    public int startValue = 0;

    public int minValue = 0;
    public int maxValue = 100;

    private int targetValue;
    private float currentValue; 

    Vector3 direction;

void Start()
    {
        //FirstPersonController player = FindObjectOfType<FirstPersonController>();
        //playerTransform = player.transform;
        
        currentValue = startValue;
        targetValue = startValue;

        if (weatherRTPC != null)
            weatherRTPC.SetGlobalValue(currentValue);
    }

    void Update()
    {
        direction = player.position - transform.position;

        if (direction.magnitude < fanDistanceSound)
        {
            //currentValue = direction.magnitude * 10;
            Debug.Log(direction.magnitude);
            currentValue = (5 - direction.magnitude) * 20;
        }
        else if (direction.magnitude >= fanDistanceSound)
        {
            currentValue = 0;
        }
        Debug.Log(currentValue);
        if (weatherRTPC != null)
                    weatherRTPC.SetGlobalValue(currentValue);


    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fanDistanceSound);
    }
}
