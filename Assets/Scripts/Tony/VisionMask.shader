Shader "Custom/VisionMask"
{
    Properties
    {
        _Color ("Darkness Color", Color) = (0,0,0,1)
        _Center ("Center (viewport 0-1)", Vector) = (0.5,0.5,0,0)
        _Radius ("Radius", Range(0,1)) = 0.2
        _Softness ("Edge Softness", Range(0.001,1)) = 0.15
        _Aspect ("Aspect Ratio (width/height)", Float) = 1.7778
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            float4 _Center;
            float _Radius;
            float _Softness;
            float _Aspect;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Correct for screen aspect ratio so the vision area is a
                // circle, not an oval, on non-square screens.
                float2 uv = i.uv;
                uv.x = (uv.x - _Center.x) * _Aspect;
                uv.y = (uv.y - _Center.y);

                float dist = length(uv);
                float alpha = smoothstep(_Radius, _Radius + _Softness, dist);

                return fixed4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}
