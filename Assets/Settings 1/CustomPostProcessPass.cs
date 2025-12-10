using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


//[System.Serializable]
//public class CustomPostProcessPass : ScriptableRendererFeature
//{
//    private Material m_bloomMaterial;
//    private Material m_comompositeMaterial;

//    public CustomPostProcessPass(Material bloomMaterial, Material compositeMaterial)
//    {
//        m_bloomMaterial = bloomMaterial;
//        m_comompositeMaterial = compositeMaterial;
//    }

//    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
//    {
//        m_Descriptor = renderingData.cameraData.cameraTargetDescriptor;
//    }

//    public void SetTarget(RTHandle cameraColorTargetHandle, RTHandle cameraDepthTargetHandle)
//    {
//        m_CameraColorTarget = cameraColorTargetHandle;
//        m_CameraDepthTarget = cameraDepthTargetHandle;
//    }

//    public override void Execute(SciptableRenderContext context, ref RenderingData renderingData)
//    {
        
//    }

//}
