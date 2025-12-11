using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    public AK.Wwise.RTPC weatherRTPC;

    public int startValue = 0;

    public int minValue = 0;
    public int maxValue = 100;

    private int targetValue;
    private float currentValue; 

    private bool playerInside = false;

    void Start()
    {
        currentValue = startValue;
        targetValue = startValue;

        // Seteamos el valor inicial en Wwise
        if (weatherRTPC != null)
            weatherRTPC.SetGlobalValue(currentValue);
    }

    void Update()
    {
        if (playerInside)
        {
            //Debug.Log("Hola");
            if (currentValue < maxValue)
            {
                currentValue += Time.deltaTime * 2;
            }
            else
            {
                currentValue = maxValue;
            }
            //Debug.Log(currentValue);
            if (weatherRTPC != null)
                    weatherRTPC.SetGlobalValue(currentValue);
            
        }
    }

   void OnTriggerEnter(Collider other)
   {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
   }
}
