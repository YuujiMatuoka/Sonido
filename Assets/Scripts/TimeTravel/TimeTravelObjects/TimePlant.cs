using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TimePlant : PickableTimeTravelObject
{
    [Header("Plant Settings")]
    //w[SerializeField] private float regrowTime = 3f;

    private float currentRegrowTime;
    private bool isPlanted;

    protected override void Start()
    {
        base.Start();
    }

    public override void Grab(Transform handTransform, Lightning lightningEffect)
    {

        base.Grab(handTransform, lightningEffect); // Llama a la implementación base
    }

    public override void OnDrop()
    {
        base.OnDrop(); // Llama a la implementación base primero

    }


}