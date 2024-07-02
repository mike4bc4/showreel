Shader "Custom/GrabPassBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Size ("Size", Range(0.0, 32.0)) = 8.0
        _Quality ("Quality", Range(0.001, 1.0)) = 0.5
        [Toggle(BLUR_ON)] _BlurEnabled ("Blur Enabled", Float) = 1
    }
    SubShader
    {
        // Draw after all opaque geometry
        Tags { 
            "Queue" = "Transparent"
            "RenderType"="Transparent"
        }

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }

        Cull Off 
        ZWrite Off 
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        // Render the object with the texture generated above
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ BLUR_ON
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 grabPos : TEXCOORD0;
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _BackgroundTexture;
            float _Size;
            float _Quality;

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                #ifdef BLUR_ON
                    _Size = max(0, _Size);
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
                            color += tex2Dproj(_BackgroundTexture, i.grabPos + float4(offset, 0, 0)) / float(quality * quality);
                        }
                    }

                    return color * i.color;
                #else
                    return tex2Dproj(_BackgroundTexture, i.grabPos) * i.color;
                #endif
            }
            ENDCG
        }

    }
}
