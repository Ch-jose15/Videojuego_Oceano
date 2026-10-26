// Deep Treasure - Cookie animado de cáusticos para la luz direccional (HDRP)
// Lo usa AnimatedLightCookie.cs: renderiza este shader en una RenderTexture que se asigna
// como Cookie del sol. El patrón es tileable (se repite sin costuras).
// Blanco = luz completa, oscuro = luz atenuada -> haces en la niebla volumétrica.

Shader "DeepTreasure/CausticCookie"
{
    Properties
    {
        _Cells ("Celdas por tile (entero)", Float) = 6
        _Speed ("Velocidad", Float) = 0.6
        _LineWidth ("Grosor de líneas", Range(0.02, 0.6)) = 0.18
        _Warp ("Ondulación", Range(0, 0.6)) = 0.2
        _MinLight ("Luz mínima entre líneas", Range(0, 1)) = 0.35
        _Contrast ("Contraste", Range(0.3, 4)) = 1.4
        _CookieTime ("Tiempo (lo pone el script)", Float) = 0
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Cells, _Speed, _LineWidth, _Warp, _MinLight, _Contrast, _CookieTime;

            float2 Hash22(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453);
            }

            // Voronoi tileable: los índices de celda se repiten cada 'cells'
            float CausticLayer(float2 uv, float cells, float t, float seed)
            {
                // Ondulación periódica en [0,1] para que no rompa el tileado
                float2 warp = sin(uv.yx * 6.2831853 * 2.0 + t) * _Warp;
                float2 p = uv * cells + warp;

                float2 i = floor(p);
                float2 f = frac(p);
                float f1 = 8.0, f2 = 8.0;

                [unroll] for (int y = -1; y <= 1; y++)
                [unroll] for (int x = -1; x <= 1; x++)
                {
                    float2 g = float2(x, y);
                    float2 id = fmod(i + g + cells, cells);       // wrap -> tileable
                    float2 o = Hash22(id + seed);
                    o = 0.5 + 0.5 * sin(t + 6.2831853 * o);
                    float d = length(g + o - f);
                    if (d < f1) { f2 = f1; f1 = d; }
                    else if (d < f2) { f2 = d; }
                }

                return 1.0 - smoothstep(0.0, _LineWidth, f2 - f1);
            }

            float4 frag(v2f_img i) : SV_Target
            {
                float cells = max(1.0, round(_Cells));
                float t = fmod(_CookieTime * _Speed, 628.3185);

                float a = CausticLayer(i.uv, cells, t, 0.0);
                float b = CausticLayer(i.uv, cells, t * 1.23 + 1.7, 17.3);
                float c = saturate(a * 0.65 + b * 0.65 + a * b);

                float v = lerp(_MinLight, 1.0, pow(c, _Contrast));
                return float4(v, v, v, 1.0);
            }
            ENDCG
        }
    }
}
