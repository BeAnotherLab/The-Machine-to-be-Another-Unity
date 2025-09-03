Shader "Custom/WorldGridShaderTransparent"
{
    Properties
    {
        _Color("Grid Color", Color) = (0.2, 1, 0.2, 1)
        _BackgroundColor("Background Color", Color) = (0, 0, 0, 1)
        _GridSpacing("Grid Spacing", Float) = 1.0
        _LineWidth("Line Width", Float) = 0.05
        [HideInInspector]_MainTex("Main Tex", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Lighting Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _Color;
            float4 _BackgroundColor;
            float _GridSpacing;
            float _LineWidth;

            v2f vert(appdata v)
            {
                v2f o;
                float4 world = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = world.xyz;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 world = i.worldPos / _GridSpacing;

                float lineX = abs(frac(world.x) - 0.5);
                float lineZ = abs(frac(world.z) - 0.5);

                float grid = step(lineX, _LineWidth) + step(lineZ, _LineWidth);
                grid = saturate(grid);

                // Blend grid lines into background based on grid value
                float4 baseColor = lerp(_BackgroundColor, _Color, grid);

                // Respect alpha from _Color for fade control
                return float4(baseColor.rgb, _Color.a);
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
