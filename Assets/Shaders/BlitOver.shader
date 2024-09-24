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
                fixed4 c1 = tex2D(_DestTex, i.texcoord);
                fixed4 c2 = tex2D(_MainTex, i.texcoord);
                
                float g = 2.2;
                c1.a = pow(c1.a, 1 / g);
                c2.a = pow(c2.a, 1 / g);

                float a0 = c2.a + c1.a * (1 - c2.a);
                fixed3 c0 = c2.rgb + c1.rgb * (1 - c2.a);
                
                a0 = pow(a0, g);
                return fixed4(c0, a0);
            }
    
            ENDCG
        }
    }

    Fallback Off
}
