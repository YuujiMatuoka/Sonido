using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ShockwaveRendererFeature : ScriptableRendererFeature
{
    class ShockwaveRenderPass : ScriptableRenderPass
    {
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Pass vacío solo para forzar la inclusión del shader
        }
    }

    ShockwaveRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new ShockwaveRenderPass();
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}