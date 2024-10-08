Shader "Custom/Vignette"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Dither ("Dither", Range(0, 1)) = 0.1
        _PowerFactor("Power Factor", Range(1, 8)) = 1
    }
    SubShader
    {
        // No culling or depth
        Cull Off 
        ZWrite Off 
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            static const float2 NoiseTextureSize = float2(512, 512);

            sampler2D _NoiseTex;
            float _Dither;
            fixed4 _Color;
            float _PowerFactor;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 textureScale = _ScreenParams.xy / NoiseTextureSize;
                fixed4 random = tex2D(_NoiseTex, i.uv * textureScale);

                _Dither = clamp(_Dither, 0, 1);
                float2 offset = (random - float2(0.5, 0.5)) * 2 * _Dither;

                float dist = distance(float2(0.5, 0.5), i.uv + offset);
                dist /= 0.707;

                _PowerFactor = max(1, _PowerFactor);
                dist = pow(dist, _PowerFactor);

                return _Color * dist;
            }
            ENDCG
        }
    }
}
