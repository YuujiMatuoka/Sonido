Shader "Custom/SubstanceStyle_CellShading"
{
    Properties
    {
        [Header(Base Color Textures)]
        [MainTexture] _BaseMap("Base Color Map", 2D) = "white" {}
        _BaseColorIntensity("Base Color Intensity", Range(0, 3)) = 1.2
        [Toggle]_PureBaseColor("Pure Base Color", Float) = 1

        [Header(Emission)]
        _EmissionMap("Emission Map", 2D) = "black" {}
        _EmissionStrength("Emission Strength", Range(0,20)) = 1
        _EmissionColor("Emission Color", Color) = (1,1,1,1)

        [Header(Cell Shading)]
        _RampLevels("Ramp Levels", Range(1, 8)) = 3
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.4
        _ShadowSmoothness("Shadow Smoothness", Range(0, 0.2)) = 0.05
        _SpecularSize("Specular Size", Range(0, 1)) = 0.1
        _SpecularIntensity("Specular Intensity", Range(0, 2)) = 1
        _RimPower("Rim Power", Range(0, 10)) = 3
        _RimIntensity("Rim Intensity", Range(0, 2)) = 0.5

        [Header(Dirt Generation)]
        _DirtColor("Dirt Color", Color) = (0.3, 0.25, 0.2, 1)
        _DirtAmount("Dirt Amount", Range(0,1)) = 0.6
        _DirtSize("Dirt Size", Range(1,50)) = 15
        _DirtSharpness("Dirt Sharpness", Range(0,10)) = 4
        _DirtEdgeVariation("Edge Variation", Range(0,5)) = 1.5
        _DirtAffectAlbedo("Affect Base Color", Range(0,1)) = 0.8

        [Header(Lighting Control)]
        _LightInfluence("Light Influence", Range(0,2)) = 1
        _AmbientStrength("Ambient Strength", Range(0,2)) = 0.8
        _DirectLightMultiplier("Direct Light Multiplier", Range(0,3)) = 1.2

        [Header(Depth for Fresnel Effects)]
        [Toggle]_EnableDepthOutput("Enable Depth for Fresnel", Float) = 1
    }

        SubShader
        {
            Tags
            {
                "RenderPipeline" = "UniversalPipeline"
                "RenderType" = "Opaque"
                "Queue" = "Geometry"
                "IgnoreProjector" = "True"
            }

            // Pass 1: Renderizado principal
            Pass
            {
                Name "ForwardLit"
                Tags { "LightMode" = "UniversalForward" }

                ZWrite On
                ZTest LEqual
                ColorMask RGBA
                Cull Back
                Blend One Zero

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
                #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
                #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
                #pragma multi_compile_fragment _ _SHADOWS_SOFT
                #pragma multi_compile_fog
                #pragma multi_compile _ LIGHTMAP_ON

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

                struct Attributes
                {
                    float4 positionOS : POSITION;
                    float3 normalOS   : NORMAL;
                    float2 uv         : TEXCOORD0;
                };

                struct Varyings
                {
                    float4 positionHCS : SV_POSITION;
                    float3 posWS : TEXCOORD0;
                    float3 normalWS : TEXCOORD1;
                    float3 viewDirWS : TEXCOORD2;
                    float2 uv : TEXCOORD3;
                    half3 vertexSH : TEXCOORD4;
                    float clipDepth : TEXCOORD5; // Para efectos de profundidad
                };

                TEXTURE2D(_BaseMap);
                SAMPLER(sampler_BaseMap);
                TEXTURE2D(_EmissionMap);
                SAMPLER(sampler_EmissionMap);

                CBUFFER_START(UnityPerMaterial)
                    float _BaseColorIntensity;
                    float _PureBaseColor;
                    float4 _EmissionColor;
                    float _EmissionStrength;
                    float _RampLevels;
                    float _ShadowThreshold;
                    float _ShadowSmoothness;
                    float _SpecularSize;
                    float _SpecularIntensity;
                    float _RimPower;
                    float _RimIntensity;
                    float4 _DirtColor;
                    float _DirtAmount;
                    float _DirtSize;
                    float _DirtSharpness;
                    float _DirtEdgeVariation;
                    float _DirtAffectAlbedo;
                    float _LightInfluence;
                    float _AmbientStrength;
                    float _DirectLightMultiplier;
                    float _EnableDepthOutput;
                CBUFFER_END

                    // [Tus funciones hash, noise, blotchNoise, celRamp aquí...]
                    float hash(float2 p) {
                        return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
                    }

                    float noise(float2 p)
                    {
                        float2 i = floor(p);
                        float2 f = frac(p);
                        float a = hash(i);
                        float b = hash(i + float2(1.0, 0.0));
                        float c = hash(i + float2(0.0, 1.0));
                        float d = hash(i + float2(1.0, 1.0));
                        float2 u = f * f * (3.0 - 2.0 * f);
                        return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
                    }

                    float blotchNoise(float2 uv)
                    {
                        float n = noise(uv);
                        n += 0.5 * noise(uv * 0.5);
                        n += 0.25 * noise(uv * 0.25);
                        n /= 1.75;
                        n = smoothstep(0.4, 0.6, n);
                        n = pow(n, _DirtSharpness);
                        n += (noise(uv * 3.0) - 0.5) * 0.25 * _DirtEdgeVariation;
                        return saturate(n);
                    }

                    float celRamp(float value, float levels)
                    {
                        float stepped = floor(value * levels) / levels;
                        float smoothStep = smoothstep(stepped - _ShadowSmoothness, stepped + _ShadowSmoothness, value);
                        return lerp(stepped, smoothStep, _ShadowSmoothness * 10.0);
                    }

                    Varyings vert(Attributes IN)
                    {
                        Varyings OUT;

                        VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                        VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                        OUT.positionHCS = positionInputs.positionCS;
                        OUT.posWS = positionInputs.positionWS;
                        OUT.normalWS = normalInputs.normalWS;
                        OUT.viewDirWS = GetWorldSpaceNormalizeViewDir(OUT.posWS);
                        OUT.uv = IN.uv;
                        OUT.vertexSH = SampleSH(OUT.normalWS);
                        OUT.clipDepth = OUT.positionHCS.z / OUT.positionHCS.w; // Profundidad para Fresnel

                        return OUT;
                    }

                    half3 ComputeCellShading(float3 posWS, float3 normalWS, float3 viewDirWS)
                    {
                        half3 totalLight = 0;
                        float3 reflected = reflect(-viewDirWS, normalWS);

                        Light mainLight = GetMainLight(TransformWorldToShadowCoord(posWS));
                        float3 lightDir = mainLight.direction;

                        float NdotL = dot(normalWS, -lightDir);
                        float celDiffuse = celRamp(saturate(NdotL), _RampLevels);
                        celDiffuse = celDiffuse < _ShadowThreshold ? celDiffuse * 0.5 : celDiffuse;

                        float specular = pow(saturate(dot(reflected, -lightDir)), _SpecularSize * 100.0);
                        specular = step(0.9 - _SpecularSize, specular) * _SpecularIntensity;

                        float rim = 1.0 - saturate(dot(normalWS, viewDirWS));
                        rim = pow(rim, _RimPower) * _RimIntensity;

                        totalLight += mainLight.color * (celDiffuse + specular + rim) *
                                     mainLight.distanceAttenuation * mainLight.shadowAttenuation * _DirectLightMultiplier;

                        #if defined(_ADDITIONAL_LIGHTS)
                        uint lightCount = GetAdditionalLightsCount();
                        for (uint i = 0; i < lightCount; i++)
                        {
                            Light light = GetAdditionalLight(i, posWS);
                            float3 addLightDir = light.direction;
                            float addNdotL = dot(normalWS, -addLightDir);
                            float addCelDiffuse = celRamp(saturate(addNdotL), _RampLevels);

                            float addSpecular = pow(saturate(dot(reflected, -addLightDir)), _SpecularSize * 80.0);
                            addSpecular = step(0.8 - _SpecularSize, addSpecular) * _SpecularIntensity * 0.5;

                            totalLight += light.color * (addCelDiffuse + addSpecular) *
                                         light.distanceAttenuation * light.shadowAttenuation;
                        }
                        #endif

                        return totalLight;
                    }

                    half4 frag(Varyings IN) : SV_Target
                    {
                        half4 baseColorTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                        half4 emissionTex = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv);

                        half3 baseColor = baseColorTex.rgb * _BaseColorIntensity;
                        if (_PureBaseColor > 0.5)
                        {
                            baseColor = baseColorTex.rgb;
                        }

                        float dirtMask = blotchNoise(IN.uv * _DirtSize);
                        dirtMask = 1.0 - dirtMask;
                        float3 finalBaseColor = lerp(baseColor, baseColor * lerp(1.0, _DirtColor.rgb, dirtMask), _DirtAmount * _DirtAffectAlbedo);

                        float3 normalWS = normalize(IN.normalWS);
                        float3 viewDirWS = normalize(IN.viewDirWS);
                        float3 lighting = ComputeCellShading(IN.posWS, normalWS, viewDirWS);

                        half3 ambient = IN.vertexSH * _AmbientStrength;

                        float3 litColor = finalBaseColor * (lighting + ambient) * _LightInfluence +
                                         finalBaseColor * (1.0 - _LightInfluence);

                        float3 emission = emissionTex.rgb * _EmissionColor.rgb * _EmissionStrength;

                        float3 finalColor = litColor + emission;

                        return half4(finalColor, baseColorTex.a);
                    }
                    ENDHLSL
                }

            // Pass 2: Shadow Caster (IMPORTANTE para sombras)
            Pass
            {
                Name "ShadowCaster"
                Tags{"LightMode" = "ShadowCaster"}

                ZWrite On
                ZTest LEqual
                ColorMask 0

                HLSLPROGRAM
                #pragma vertex ShadowPassVertex
                #pragma fragment ShadowPassFragment

                #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
                ENDHLSL
            }

                        // Pass 3: Depth Only (CRÍTICO para efectos Fresnel)
                        Pass
                        {
                            Name "DepthOnly"
                            Tags{"LightMode" = "DepthOnly"}

                            ZWrite On
                            ColorMask R
                            Cull Back

                            HLSLPROGRAM
                            #pragma vertex DepthOnlyVertex
                            #pragma fragment DepthOnlyFragment

                            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
                            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
                            ENDHLSL
                        }

                        // Pass 4: Depth Normals (OPCIONAL pero recomendado)
                        Pass
                        {
                            Name "DepthNormals"
                            Tags{"LightMode" = "DepthNormals"}

                            ZWrite On
                            Cull Back

                            HLSLPROGRAM
                            #pragma vertex DepthNormalsVertex
                            #pragma fragment DepthNormalsFragment

                            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
                            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
                            ENDHLSL
                        }
        }

            Fallback "Universal Render Pipeline/Lit"
}