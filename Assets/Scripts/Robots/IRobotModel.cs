using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRobotModel 
{
    void Move(Vector3 dir);
    void LookDir(Vector3 dir);
    Vector3 GetDir();
    void SetPosition(Vector3 pos);
}
