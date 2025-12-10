Shader "Custom/DepthMaskShader"
{
    // Este shader no necesita propiedades
    Properties { }

    SubShader
    {
        // Etiqueta: Renderizar como geometría opaca,
        // pero en una cola ligeramente anterior (para asegurarse de que se dibuje primero)
        Tags { "RenderType"="Opaque" "Queue"="Geometry-10" }

        Pass
        {
            // --- LA MAGIA ---
            
            // 1. SÍ escribe en el Z-Buffer (el buffer de profundidad)
            ZWrite On
            
            // 2. NO escribe en el Color Buffer (hace que el objeto sea invisible)
            ColorMask 0
        }
    }
}
