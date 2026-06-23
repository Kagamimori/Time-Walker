Shader "Custom/SpriteEdgeGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _EdgeColor ("发光颜色", Color) = (1, 0.8, 0, 1)
        _EdgeWidth ("发光厚度", Range(0, 0.05)) = 0.001
        _HitEffect ("效果透明度", Range(0, 1)) = 1.0

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
            float4 _EdgeColor;
            float _EdgeWidth;
            fixed4 _RendererColor;
            float _HitEffect;

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
                fixed4 mainColor = tex2D(_MainTex, IN.texcoord);

                if (mainColor.a > 0.1)
                {
                    return mainColor * IN.color;
                }

                // 接下来只关心透明区域
                float2 texelSize = _MainTex_TexelSize.xy; // TexelSize.xy指获取纹理的1/宽或高
                // 将厚度（UV空间）转换为像素步数，并限制最大为 16 步
                int maxSteps = clamp((int)(_EdgeWidth / texelSize.x + 0.5), 1, 16);
                float2 offsets[6] = {
                    float2(0, 1),
                    float2(0, -1),
                    float2(1, 0),
                    float2(-1, 0),
                    float2(1, 1),
                    float2(-1, 1)
                }; // 左下右下也读找了 

                bool isEdge = false;
                int step = 1;

                // 加上 [loop] 阻止编译器展开
                [loop]
                for (step = 1; step <= maxSteps; step++)
                {
                    [loop]
                    for (int j = 0; j < 6; j++)
                    {
                        float2 sampleUV = IN.texcoord + offsets[j] * step * texelSize;
                        float alpha = tex2D(_MainTex, sampleUV).a;
                        if (alpha > 0.1)
                        {
                            isEdge = true;
                            break;
                        }
                    }
                    if (isEdge) break;
                }

                if (isEdge)
                {
                    float alpha = 1.0 - (step - 1.0) / maxSteps;
                    // 量化为 2 阶：大于 0.5 就是 1，否则就是 0.5
                    alpha = ceil(alpha - 0.5) * 0.5 + 0.5; 
    
                    float finalAlpha = _EdgeColor.a * alpha * _HitEffect; 
                    return fixed4(_EdgeColor.rgb, _EdgeColor.a * finalAlpha);
                }
                else
                {
                    return fixed4(0, 0, 0, 0);
                }
            }
            ENDCG
        }
    }
}