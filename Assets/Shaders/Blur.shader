Shader "Custom/BlurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AlphaTex ("Alpha Mask", 2D) = "white" {}
        _Size ("Size", Range(0.0, 32.0)) = 8.0
        _Quality ("Quality", Range(0.001, 1.0)) = 0.5
        [Toggle(BLUR_ON)] _BlurEnabled ("Blur Enabled", Float) = 1
        // [Toggle(USE_ALPHA_MASK)] _UseAlphaMask ("Use Alpha Mask", Float) = 1
    }
    SubShader
    {
        Tags { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Cull Off 
        ZWrite Off 
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ BLUR_ON
            // #pragma multi_compile _ USE_ALPHA_MASK
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float _Size;
            float _Quality;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // #ifdef USE_ALPHA_MASK
                fixed4 alphaSample = tex2D(_AlphaTex, i.uv);
                i.color.a *= alphaSample.r;
                // #endif

                #ifdef BLUR_ON
                    float4 pixelSize = 1 / _ScreenParams;                
                    int quality = ceil(16 * _Quality) + 1;
                    float step = 1 / float(quality);

                    fixed4 color;
                    for(int x = 0; x < quality; x++)
                    {
                        for(int y = 0; y < quality; y++)
                        {
                            float a = float(x) / float(quality - 1);
                            float b = float(y) / float(quality - 1);
                            float2 offset = pixelSize.xy * _Size * (float2(a, b) * 2 - 1);
                            color += tex2D(_MainTex, i.uv + offset) / float(quality * quality);
                        }
                    }

                    return color * i.color;
                #else
                    return tex2D(_MainTex, i.uv) * i.color;
                #endif
            }
            
            ENDCG
        }
    }
}
