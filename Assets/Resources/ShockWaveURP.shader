Shader "Custom/URP_ShockWave"
{
    Properties
    {
        _Radius("Radius", Range(-0.2, 1)) = 0.2
        _Amplitude("Amplitude", Range(0, 1)) = 0.05
        _WaveSize("WaveSize", Range(0, 5)) = 0.2
        _Fade("Fade", Range(0, 1)) = 1
    }

        SubShader
    {
        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Shockwave"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        // Para acceder a la textura de la cámara
        TEXTURE2D(_CameraColorTexture);
        SAMPLER(sampler_CameraColorTexture);

        float _Radius;
        float _Amplitude;
        float _WaveSize;
        float _Fade;

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float4 screenPos : TEXCOORD1;
        };

        Varyings vert(Attributes IN)
        {
            Varyings OUT;
            OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
            OUT.uv = IN.uv;
            OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
            return OUT;
        }

        half4 frag(Varyings IN) : SV_Target
        {
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float2 diff = screenUV - 0.5;
            float dist = length(diff);

            float2 uvOffset = 0;
            float alpha = 0;

            // Cálculo del anillo de distorsión
            if (dist > _Radius && dist < _Radius + _WaveSize)
            {
                float angle = (dist - _Radius) * 6.2831853 / _WaveSize;
                float wave = (1 - cos(angle)) * 0.5;
                uvOffset -= wave * diff * (_Amplitude * _Fade / max(dist, 0.001));
                alpha = smoothstep(_Radius + _WaveSize, _Radius, dist);
            }

            float2 displacedUV = screenUV + uvOffset;

            // Samplea la textura de color de la cámara correctamente
            half4 sceneColor = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, displacedUV);
            sceneColor.a = alpha * _Fade;

            return sceneColor;
        }
        ENDHLSL
    }
    }
}