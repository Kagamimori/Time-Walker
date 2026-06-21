Shader "Custom/PixelDissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("噪声纹理", 2D) = "white" {}          // 用于溶解的噪点图
        _PixelSize ("像素块大小", Range(1, 20)) = 4      // 基础像素块尺寸
        _DissolveThreshold ("溶解阈值", Range(0, 1)) = 0.5 // 溶解敏感度
        _EdgeColor ("边缘颜色", Color) = (1, 0.5, 0, 1)   // 溶解边缘发光色
        _HitAmount ("消失进度", Range(0, 1)) = 0          // 外部控制（0=正常，1=完全消失）
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
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
            #pragma vertex SpriteVert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _PixelSize;
            float _DissolveThreshold;
            float4 _EdgeColor;
            float _HitAmount;

            v2f SpriteVert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. 像素扩大（马赛克）效果
                // 根据 _HitAmount 动态缩放像素块大小：从 _PixelSize 逐渐增大到 _PixelSize * (1 + _HitAmount * 5)
                float blockScale = _PixelSize * (1.0 + _HitAmount * 5.0);
                // 计算每个像素块的大小（在 UV 空间）
                float2 blockUV = _MainTex_TexelSize.xy * blockScale;
                // 将当前 UV 对齐到最近的块中心（或左上角，这里使用中心对齐）
                float2 blockCenter = floor(IN.texcoord / blockUV) * blockUV + blockUV * 0.5;
                // 采样块中心的颜色（实现像素块放大）
                fixed4 mainColor = tex2D(_MainTex, blockCenter);

                // 如果原图该处是透明的，直接丢弃（避免边缘扩散）
                float originalAlpha = tex2D(_MainTex, IN.texcoord).a;
                if (originalAlpha < 0.1) discard;

                // 2. 溶解效果
                // 采样噪声纹理（使用原 UV 或世界坐标，这里用原 UV 加上时间可产生动态，但这里我们用静态噪声）
                float2 noiseUV = IN.texcoord * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
                float noise = tex2D(_NoiseTex, noiseUV).r;
                // 溶解阈值：随 _HitAmount 增大，阈值降低，使更多像素被裁掉
                float threshold = 1.0 - _HitAmount * (1.0 + _DissolveThreshold);
                // 如果噪声值大于阈值，则丢弃该像素（产生溶解空洞）
                clip(noise - threshold);

                // 3. 溶解边缘发光
                // 在即将被裁掉的边缘（噪声值接近阈值）增加发光色
                float edgeGlow = smoothstep(threshold - 0.05, threshold, noise);
                // 混合边缘发光
                float3 finalColor = mainColor.rgb;
                float finalAlpha = mainColor.a;
                if (edgeGlow > 0.01)
                {
                    // 发光颜色叠加（加色混合）
                    finalColor = lerp(finalColor, _EdgeColor.rgb, edgeGlow * 0.8);
                    finalAlpha = max(finalAlpha, _EdgeColor.a * edgeGlow);
                }

                // 应用顶点颜色（保留 Sprite 的 tint）
                finalColor *= IN.color.rgb;
                finalAlpha *= IN.color.a;

                return fixed4(finalColor, finalAlpha);
            }
            ENDCG
        }
    }
}