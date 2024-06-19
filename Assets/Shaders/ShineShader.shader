Shader "Custom/ShineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShineTexture("Shine Texture", 2D) = "white" {}
        _ShineOffset("Shine Offset", Range(0.0, 1.0)) = 0.5
        _ShineWidth("Shine Width", Range(0.0, 1.0)) = 0.5
        _ShineColor("Shine Color", Color) = (1.0, 1.0, 1.0, 1.0)

        // [Toggle(C1)] _C1 ("C1", Float) = 0
        // [KeywordEnum(A, B, C)] _C1("C1", Float) = 0
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
            // #pragma multi_compile _ _C1_A
            // #pragma multi_compile _C1_A _C1_B _C1_C

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
            float4 _MainTex_ST;
            sampler2D _ShineTexture;
            float _ShineOffset;
            float _ShineWidth;
            fixed4 _ShineColor;
            // float _C1;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Support texture tiling and offset.
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // #ifdef _C1_A

                fixed4 color = tex2D(_MainTex, i.uv);
                
                _ShineWidth = 1.0 / clamp(_ShineWidth, 0.001, 1.0);
                _ShineOffset = clamp(_ShineOffset, 0.0, 1.0);
                fixed4 shine = tex2D(_ShineTexture, (i.uv + float2(1.0 - _ShineOffset, 0.0)) * float2(_ShineWidth, 1.0) - float2(_ShineWidth - 1.0 / 2.0, 0.0));
                
                color = fixed4(lerp(color.rgb, _ShineColor, shine.r), color.a);
                return color;
            }
            
            ENDCG
        }
    }
}
