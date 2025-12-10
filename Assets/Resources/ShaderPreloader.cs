using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderPreloader : MonoBehaviour
{
    [SerializeField] private Shader shockwaveShader;

    void Start()
    {
        if (shockwaveShader != null)
        {
            // Fuerza la inclusión del shader creando un material temporal
            Material tempMaterial = new Material(shockwaveShader);
            tempMaterial.hideFlags = HideFlags.HideAndDontSave;

        }

        // También fuerza shaders de URP
        Shader.Find("Universal Render Pipeline/Lit");
        Shader.Find("Sprites/Default");
    }
}