Shader "Custom/BlurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AlphaTex ("Alpha Mask", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0.0, 32.0)) = 8.0
        _BlurQuality ("Blur Quality", Range(0.001, 1.0)) = 0.5
        [Toggle(BLUR_ON)] _BlurEnabled ("Blur Enabled", Float) = 1

        [HideInInspector] _MaskCoords0 ("Mask Coords 0", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords1 ("Mask Coords 1", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords2 ("Mask Coords 2", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords3 ("Mask Coords 3", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords4 ("Mask Coords 4", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords5 ("Mask Coords 5", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords6 ("Mask Coords 6", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords7 ("Mask Coords 7", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords8 ("Mask Coords 8", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords9 ("Mask Coords 9", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords10 ("Mask Coords 10", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords11 ("Mask Coords 11", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords11 ("Mask Coords 12", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords11 ("Mask Coords 13", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords11 ("Mask Coords 14", Vector) = (-1, -1, -1, -1)
        [HideInInspector] _MaskCoords11 ("Mask Coords 15", Vector) = (-1, -1, -1, -1)
        
        [KeywordEnum(None, X8, X16)] _MASK_CHANNELS("Mask Channels", Float) = 0
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
            #pragma multi_compile _MASK_CHANNELS_NONE _MASK_CHANNELS_X8 _MASK_CHANNELS_X16
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
            float _BlurSize;
            float _BlurQuality;
            float4 _MaskCoords0;
            float4 _MaskCoords1;
            float4 _MaskCoords2;
            float4 _MaskCoords3;
            float4 _MaskCoords4;
            float4 _MaskCoords5;
            float4 _MaskCoords6;
            float4 _MaskCoords7;
            float4 _MaskCoords8;
            float4 _MaskCoords9;
            float4 _MaskCoords10;
            float4 _MaskCoords11;
            float4 _MaskCoords12;
            float4 _MaskCoords13;
            float4 _MaskCoords14;
            float4 _MaskCoords15;

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
                fixed4 alphaSample = tex2D(_AlphaTex, i.uv);
                i.color.a *= alphaSample.r;

                fixed4 color;
                #ifdef BLUR_ON
                    float4 pixelSize = 1 / _ScreenParams;                
                    int quality = ceil(16 * _BlurQuality) + 1;
                    float step = 1 / float(quality);

                    for(int x = 0; x < quality; x++)
                    {
                        for(int y = 0; y < quality; y++)
                        {
                            float a = float(x) / float(quality - 1);
                            float b = float(y) / float(quality - 1);
                            float2 offset = pixelSize.xy * _BlurSize * (float2(a, b) * 2 - 1);
                            color += tex2D(_MainTex, i.uv + offset) / float(quality * quality);
                        }
                    }
                #else
                    color = tex2D(_MainTex, i.uv);
                #endif

                #ifdef _MASK_CHANNELS_X8
                    float4 coords[8] = {
                        _MaskCoords0, _MaskCoords1, _MaskCoords2, _MaskCoords3,
                        _MaskCoords4, _MaskCoords5, _MaskCoords6, _MaskCoords7
                    };

                    for(int idx = 0; idx < 8; idx++){
                        if(i.uv.x > coords[idx].x && i.uv.x < coords[idx].z && i.uv.y > coords[idx].y && i.uv.y < coords[idx].w)
                        {
                            discard;
                        }
                    }
                #endif

                #ifdef _MASK_CHANNELS_X16
                    float4 coords[16] = {
                        _MaskCoords0, _MaskCoords1, _MaskCoords2, _MaskCoords3,
                        _MaskCoords4, _MaskCoords5, _MaskCoords6, _MaskCoords7,
                        _MaskCoords8, _MaskCoords9, _MaskCoords10, _MaskCoords11,
                        _MaskCoords12, _MaskCoords13, _MaskCoords14, _MaskCoords15
                    };

                    for(int idx = 0; idx < 16; idx++){
                        if(i.uv.x > coords[idx].x && i.uv.x < coords[idx].z && i.uv.y > coords[idx].y && i.uv.y < coords[idx].w)
                        {
                            discard;
                        }
                    }
                #endif

                return color * i.color;
            }
            
            ENDCG
        }
    }
}
