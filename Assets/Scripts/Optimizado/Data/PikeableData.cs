using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PikeableData
{
    public float grabDistance = 2.5f;
    public float holdSpeed = 15f;
    public float minSpeed = 2f;
    public float angularSpeed = 15f;
    public float grabTolerance = 0.02f;
    public float maxGrabDistance = 4f;
    public float positionThreshold = 0.1f;
    public float minImpactVelocity = 1.5f;
    public float minTimeBetweenSounds = 0.2f;
}