Shader "Custom/MonsterHitShader_PixelUV"
{
    Properties
    {

        _MainTex ("Sprite Texture", 2D) = "white" {}
        _HitEffect ("Hit Effect Amount", Range(0, 1)) = 0
        
        _HitFlashColor ("Flash Color", Color) = (0.8, 0, 0, 1)
        _HitCompressScale ("Compress Scale", Range(0.2, 1)) = 0.7
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _HitEffect;
            fixed4 _HitFlashColor;
            float _HitCompressScale;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color ;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float hitAmount = _HitEffect;
                
                
                // 将 UV 原点移到中心，修正宽高比，再压缩
                float2 uvCentered = i.texcoord - 0.5;

                float compressCurve = pow(abs(uvCentered.x),1.5);
                
                float scaleX = 1 - hitAmount * compressCurve * (1.0 - _HitCompressScale) ; // X越大，扭曲越大
                uvCentered.x *= scaleX;
                float2 compressedUV = uvCentered + 0.5;
                // compressedUV.x = clamp(compressedUV.x, 0.0, 1.0);
    
                // 采样纹理
                fixed4 color = tex2D(_MainTex, compressedUV) * i.color;
                fixed4 hitColor = fixed4(_HitFlashColor.r  * compressCurve * hitAmount + 0.05,_HitFlashColor.gba);

                // 闪红
                if (color.a > 0.01)
                {
                    color.rgb = lerp(color.rgb, hitColor.rgb, hitAmount - 0.15);
                }
             
    
                return color;
            }
            
            ENDCG
        }
    }
}