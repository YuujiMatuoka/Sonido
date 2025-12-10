using UnityEngine;

public class ShaderBuildFix : MonoBehaviour
{
    [SerializeField] private Shader shockwaveShader;
    [SerializeField] private Material shockwaveMaterial;

    void Awake()
    {
        // También fuerza la inclusión del shader URP necesario
        var urpShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpShader != null)
        {
            var tempMat = new Material(urpShader);
            tempMat.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}