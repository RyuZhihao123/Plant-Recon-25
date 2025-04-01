Shader "AlphaTest"{
    Properties{
        _MainTex("Main Tex", 2D) = "white" {}
        _Color("Color", Color) = (0, 0, 0, 1)
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5 //控制剔除的阈值
    }
        SubShader
        {
            Tags { "Queue" = "AlphaTest" "IgnoreProjection" = "True" "RanderType" = "TransparentCutout"}

            Pass{
                Tags { "LightMode" = "ForwardBase" }

                Cull Off	//用于双面渲染，不剔除背对摄像机的图元


                CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag

                #include "Lighting.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                };

                struct v2f {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    float3 worldPos : TEXCOORD1;
                    float3 worldNormal : TEXCOORD2;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed _Cutoff;
                fixed4 _Color;

                v2f vert(appdata v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.worldNormal = UnityObjectToWorldNormal(v.normal);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target{
                    

                    return fixed4(_Color.rgb, 1.0);
                }
                ENDCG
            }
        }
            FallBack "Transparent/Cutout/VertexLit"
}