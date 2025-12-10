Shader "Custom/SimpleOutlineShader"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness("Outline Thickness", Float) = 0.05
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            Name "OUTLINE"
            ZWrite On
            ColorMask RGB
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float _OutlineThickness;
            float4 _OutlineColor;

            v2f vert(appdata v)
            {
                v2f o;

                // Transform the normal to world space
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                // Inflate along the normal
                worldPos += worldNormal * _OutlineThickness;

                // Now project the new position
                o.pos = UnityWorldToClipPos(float4(worldPos, 1.0));

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }
    }
        Fallback "Diffuse"
}