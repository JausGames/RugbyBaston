Shader "Custom/GrassShader" {
    Properties{
        _MainTex("Main Texture", 2D) = "white" {}
        _GrassColor("Grass Color", Color) = (0.2, 0.8, 0.2, 1.0)
        _ParallaxStrength("Parallax Strength", Range(0.0, 1.0)) = 0.1
    }

        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Lambert

            sampler2D _MainTex;
            fixed4 _GrassColor;
            float _ParallaxStrength;

            struct Input {
                float2 uv_MainTex;
            };

            void surf(Input IN, inout SurfaceOutput o) {
                // Sample the main texture
                fixed4 mainTextureColor = tex2D(_MainTex, IN.uv_MainTex);

                // Calculate parallax offset
                float2 parallaxOffset = normalize(IN.uv_MainTex - 0.5) * _ParallaxStrength;
                float2 parallaxUV = IN.uv_MainTex + parallaxOffset;

                // Sample the parallax offset texture
                fixed4 parallaxTextureColor = tex2D(_MainTex, parallaxUV);

                // Set the grass color
                o.Albedo = mainTextureColor.rgb * _GrassColor.rgb;
                o.Alpha = mainTextureColor.a;

                // Apply parallax offset to the surface position
                o.Normal = UnpackNormal(float4(parallaxTextureColor.rgb, 0.0));
                o.Normal.xy *= _ParallaxStrength;
            }
            ENDCG
        }

            FallBack "Diffuse"
}
