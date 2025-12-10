using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//[System.Serializable]
//public class CustomPostProcessRenderFeature : ScriptableRendererFeature
//{
//    [SerializeField]
//    private Shader m_bloomShader;
//    [SerializeField]
//    private Shader m_compositeShader;

//    private CustomPostProcessPass m_customPass;
//    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//    {
//        renderer.EnqueuePass(m_customPass);
//    }

//    public override void Create()
//    {
//        m_bloomShader = CoreUtils.CreateEngineMaterial(m_bloomShader);
//        m_compositeShader = CoreUtils.CreateEngineMaterial(m_compositeShader);


//        m_customPass = new CustomPostProcessPass();
//    }

//    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
//    {
//        if (renderingData.cameraData.cameraType == CameraType.Game)
//        {
//            m_pass.ConfigureInput(ScriptableRenderPassInput.Depth);
//            m_pass.ConfigureInput(ScriptableRenderPassInput.Color);
//            m_pass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
//        }
//    }

//    protected override void Dispose(bool disposing)
//    {
//        CoreUtils.Destroy(m_bloomMaterial);
//        CoreUtils.Destroy(m_compositematerial);

//    }
//}
