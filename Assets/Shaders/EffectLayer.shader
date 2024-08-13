Shader "Custom/EffectLayer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ElementRect ("ElementRect", Vector) = (0, 0, 0, 0)
        _Overscan ("Overscan", Vector) = (0, 0, 0, 0)
        _BlurSize ("Blur Size", Range(0.0, 32.0)) = 8.0
        _BlurQuality ("Blur Quality", Range(1, 8)) = 2
        [Toggle(_BLUR_ENABLED)] _BlurEnabled ("Blur Enabled", Float) = 1
    }
    SubShader
    {
        Tags 
        { 
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
            #pragma multi_compile _ _BLUR_ENABLED
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
            float4 _ElementRect;
            float _BlurSize;
            float _BlurQuality;
            float4 _Overscan;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            int contains(float2 position){
                float x = _ElementRect.x - _Overscan.w;
                float y = _ElementRect.y - _Overscan.z;
                float xMax = _ElementRect.x + _ElementRect.z + _Overscan.y;
                float yMax = _ElementRect.y + _ElementRect.w + _Overscan.x;
                return x < position.x && position.x < xMax  && y < position.y && position.y < yMax;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pixelPosition = i.uv * _ScreenParams.xy;
                if(!contains(pixelPosition)){
                    discard;
                }

                fixed4 color;

                #ifdef _BLUR_ENABLED
                    int inputQuality = max(1, ceil(_BlurQuality));
                    int quality = inputQuality * 2 + 1;
                    int qq = quality * quality;
                    for(int x = 0; x < quality; x++)
                    {
                        for(int y = 0; y < quality; y++)
                        {
                            float2 pixelOffset = float2(x, y) - float2(inputQuality, inputQuality);
                            pixelOffset *= _BlurSize / inputQuality;
                            float2 uvOffset = pixelOffset / _ScreenParams.xy;
                            color += contains(pixelPosition + pixelOffset) * (tex2D(_MainTex, i.uv + uvOffset) / float(qq));
                        }
                    }

                    color.rgb *= 1 / color.a;
                #else
                    color = tex2D(_MainTex, i.uv);
                #endif

                return color * i.color;
            }
            
            ENDCG
        }
    }
}
