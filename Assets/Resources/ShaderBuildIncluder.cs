using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderBuildIncluder : MonoBehaviour
{
    [SerializeField] private Shader[] requiredShaders;

    void Start()
    {    
        // Verificar específicamente tu shader
        var shockwaveShader = Shader.Find("Custom/URP_ShockWave");
    }
}