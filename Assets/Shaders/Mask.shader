Shader "Custom/Mask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AlphaTex ("Alpha Mask", 2D) = "white" {}
        // [Toggle(USE_ALPHA_MASK)] _UseAlphaMask ("Use Alpha Mask", Float) = 1
        // [Toggle] _InvertAlphaMask ("Invert Alpha Mask", Float) = 0
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
            // float _InvertAlphaMask;

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
                // if(_InvertAlphaMask)
                // {
                //     alphaSample = fixed4(1, 1, 1, 1) - alphaSample;
                // }

                i.color.a *= alphaSample.r;
                // #endif
                
                return tex2D(_MainTex, i.uv) * i.color;
            }
            
            ENDCG
        }
    }
}
