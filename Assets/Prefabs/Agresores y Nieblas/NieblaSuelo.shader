Shader "Echos/NieblaSuelo"
{
    Properties
    {
        _TexturaRuido ("Textura de ruido", 2D) = "white" {}
        _ColorNiebla ("Color de niebla", Color) = (0.55, 0.58, 0.62, 1)
        _VelocidadScrollA ("Velocidad scroll capa A", Vector) = (0.02, 0.01, 0, 0)
        _VelocidadScrollB ("Velocidad scroll capa B", Vector) = (-0.015, 0.02, 0, 0)
        _Densidad ("Densidad (alpha maximo)", Range(0,1)) = 0.6
        _Escala ("Escala de textura (tiling)", Float) = 3
        _RadioFade ("Radio del fade hacia el borde", Range(0.5, 2)) = 1.3
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_TexturaRuido);
            SAMPLER(sampler_TexturaRuido);

            CBUFFER_START(UnityPerMaterial)
                float4 _ColorNiebla;
                float4 _VelocidadScrollA;
                float4 _VelocidadScrollB;
                float _Densidad;
                float _Escala;
                float _RadioFade;
            CBUFFER_END

            struct Atributos
            {
                float4 posicion : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpoladores
            {
                float4 posicionClip : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Interpoladores vert(Atributos entrada)
            {
                Interpoladores salida;
                salida.posicionClip = TransformObjectToHClip(entrada.posicion.xyz);
                // Se conserva el UV original (0..1) para calcular el fade radial
                // y por separado se escala para el tiling del ruido.
                salida.uv = entrada.uv;
                return salida;
            }

            half4 frag(Interpoladores entrada) : SV_Target
            {
                float2 uvRuido = entrada.uv * _Escala;
                float2 uvA = uvRuido + _VelocidadScrollA.xy * _Time.y;
                float2 uvB = uvRuido * 1.4 + _VelocidadScrollB.xy * _Time.y;

                half ruidoA = SAMPLE_TEXTURE2D(_TexturaRuido, sampler_TexturaRuido, uvA).r;
                half ruidoB = SAMPLE_TEXTURE2D(_TexturaRuido, sampler_TexturaRuido, uvB).r;
                half ruidoCombinado = saturate(ruidoA * 0.6 + ruidoB * 0.6);

                // Fade radial: 1 en el centro del plano, 0 hacia los bordes.
                // Pensado para un Quad con UV 0..1 sin necesidad de pintar vertex colors.
                float distanciaCentro = distance(entrada.uv, float2(0.5, 0.5));
                float fadeBorde = saturate(1.0 - distanciaCentro * _RadioFade * 2.0);

                half alphaFinal = ruidoCombinado * _Densidad * fadeBorde;

                return half4(_ColorNiebla.rgb, alphaFinal);
            }
            ENDHLSL
        }
    }
}
