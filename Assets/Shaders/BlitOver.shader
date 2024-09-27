Shader "Hidden/BlitOver" 
{
    Properties 
    {
        _MainTex ("Texture", any) = "" {} 
    }
    SubShader 
    {
        Pass 
        {
            ZTest Always Cull Off ZWrite Off
            
            // Simple blit that respect source alpha.
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            uniform float4 _MainTex_ST;

            sampler2D _DestTex;
            uniform float4 _DestTex_ST;

            struct appdata_t 
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f 
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.texcoord);
            }
    
            ENDCG
        }
    }

    Fallback Off
}
