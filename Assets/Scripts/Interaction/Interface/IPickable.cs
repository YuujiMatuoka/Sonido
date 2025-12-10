using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickable
{
    void OnDrop();
    void Grab(Transform handTransform, Lightning lightningEffect);
    bool IsHolding();

}
