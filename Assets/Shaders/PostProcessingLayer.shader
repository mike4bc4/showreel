Shader "Custom/PostProcessingLayer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CropRect ("CropRect", Vector) = (0, 0, 0, 0)
        _Overscan ("Overscan", Vector) = (0, 0, 0, 0)
        _BlurSize ("Blur Size", Range(0.0, 32.0)) = 8.0
        _BlurQuality ("Blur Quality", Range(1, 8)) = 2
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        [Toggle(_BLUR_ENABLED)] _BlurEnabled ("Blur Enabled", Float) = 1
        [Toggle(_USE_CROP_RECT)] _UseCropRect ("Use Crop Rect", Float) = 0
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
        // We are not using any blending mode as this blit shader is supposed to replace affected
        // part of active render target.

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _BLUR_ENABLED
            #pragma multi_compile _ _USE_CROP_RECT
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

            static const float OffsetSize = 0.001;

            sampler2D _MainTex;
            uniform float4 _MainTex_ST;

            float4 _CropRect;
            float _BlurSize;
            float _BlurQuality;
            float4 _Overscan;
            fixed4 _Tint;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            int contains(float2 position){
                float x = _CropRect.x - _Overscan.w;
                float y = _CropRect.y - _Overscan.z;
                float xMax = _CropRect.x + _CropRect.z + _Overscan.y;
                float yMax = _CropRect.y + _CropRect.w + _Overscan.x;
                return x < position.x && position.x < xMax  && y < position.y && position.y < yMax;
            }

            fixed4 _TextureSampleAdd;

            fixed4 frag (v2f i) : SV_Target
            {
                #ifdef _USE_CROP_RECT
                    if(!contains(i.uv))
                    {
                        discard;
                    }
                #endif

                fixed4 color;

                #ifdef _BLUR_ENABLED
                    int inputQuality = max(1, ceil(_BlurQuality));
                    int quality = inputQuality * 2 + 1;

                    float screenRatio = _ScreenParams.x / _ScreenParams.y;

                    _BlurSize = max(0, _BlurSize) / (float)inputQuality;

                    for(int x = 0; x < quality; x++)
                    {
                        for(int y = 0; y < quality; y++)
                        {
                            float2 offset = float2(x - inputQuality, y - inputQuality) * OffsetSize;
                            offset *= float2(_BlurSize, _BlurSize * screenRatio);

                            #ifdef _USE_CROP_RECT
                                float2 uv = contains(i.uv + offset) ? i.uv + offset : i.uv;
                                color += tex2D(_MainTex, uv);
                            #else
                                color += tex2D(_MainTex, i.uv + offset);
                            #endif
                        }
                    }

                    color /= float(quality * quality);
                #else
                    color = tex2D(_MainTex, i.uv);
                #endif

                return color * i.color * _Tint;
            }
            
            ENDCG
        }
    }
}
