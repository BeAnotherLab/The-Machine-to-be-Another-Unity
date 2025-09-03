Shader "Custom/WorldGridShaderAllSides"
{
    Properties
    {
        _Color("Grid Color", Color) = (0.2, 1, 0.2, 1)
        _BackgroundColor("Background Color", Color) = (0, 0, 0, 0)
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

                // Calculate line proximity for each plane
                float2 xz = float2(frac(world.x), frac(world.z));
                float2 xy = float2(frac(world.x), frac(world.y));
                float2 yz = float2(frac(world.y), frac(world.z));

                float lineXZ = step(abs(xz.x - 0.5), _LineWidth) + step(abs(xz.y - 0.5), _LineWidth);
                float lineXY = step(abs(xy.x - 0.5), _LineWidth) + step(abs(xy.y - 0.5), _LineWidth);
                float lineYZ = step(abs(yz.x - 0.5), _LineWidth) + step(abs(yz.y - 0.5), _LineWidth);

                float grid = saturate((lineXZ + lineXY + lineYZ) / 3.0); // Average across 3 planes

                float3 color = lerp(_BackgroundColor.rgb, _Color.rgb, grid);
                float alpha = lerp(_BackgroundColor.a, _Color.a, grid);

                return float4(color, alpha);
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
