Shader "Sprites/UltimateBillboard"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [Toggle(USEFOG)] _UseFog ("Use Fog", Float) = 1
        [Toggle(CYLINDRICAL)] _Cylindrical ("Cylindrical Billboarding", Float) = 1
        [Toggle(SPHERICAL)] _Spherical ("Spherical Billboarding", Float) = 0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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
            "DisableBatching"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile_fog
            #pragma shader_feature USEFOG
            #pragma shader_feature SPHERICAL
            #pragma shader_feature CYLINDRICAL

            #include "UnitySprites.cginc"

            struct v2fBillboardFog
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2fBillboardFog vert(appdata_t IN)
            {
                v2fBillboardFog OUT;

                float4x4 mv = UNITY_MATRIX_MV;

                float3 worldScale = float3(
                length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x)), // scale x axis
                length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y)), // scale y axis
                length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z))  // scale z axis
                );

                #ifdef CYLINDRICAL
                    mv._m00 = worldScale.x; 
                    mv._m10 = 0.0f; 
                    mv._m20 = 0.0f; 
                    mv._m02 = 0.0f; 
                    mv._m12 = 0.0f; 
                    mv._m22 = worldScale.z;
                #endif

                #ifdef SPHERICAL
                    mv._m01 = 0.0f; 
                    mv._m11 = worldScale.y; 
                    mv._m21 = 0.0f; 
                        #ifndef CYLINDRICAL
                        mv._m00 = worldScale.x; 
                        mv._m10 = 0.0f; 
                        mv._m20 = 0.0f; 
                        mv._m02 = 0.0f; 
                        mv._m12 = 0.0f; 
                        mv._m22 = worldScale.z;
                        #endif
                #endif

                OUT.vertex = mul(UNITY_MATRIX_P, mul(mv, IN.vertex));
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;

                #ifdef PIXELSNAP_ON
                    OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                #ifdef USEFOG
                    UNITY_TRANSFER_FOG(OUT, OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2fBillboardFog IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                #ifdef USEFOG
                    UNITY_APPLY_FOG(IN.fogCoord, c);
                #endif
                c.rgb *= c.a;
                return c;
            }
        ENDCG

        }

    }
}