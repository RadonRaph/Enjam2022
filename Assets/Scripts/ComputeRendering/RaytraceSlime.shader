Shader "FullScreen/RaytraceSlime"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    // The PositionInputs struct allow you to retrieve a lot of useful information for your fullScreenShader:
    // struct PositionInputs
    // {
    //     float3 positionWS;  // World space position (could be camera-relative)
    //     float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
    //     uint2  positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
    //     uint2  tileCoord;   // Screen tile coordinates                              : [0, NumTiles)
    //     float  deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
    //     float  linearDepth; // View space Z coordinate                              : [Near, Far]
    // };

    // To sample custom buffers, you have access to these functions:
    // But be careful, on most platforms you can't sample to the bound color buffer. It means that you
    // can't use the SampleCustomColor when the pass color buffer is set to custom (and same for camera the buffer).
    // float4 CustomPassSampleCustomColor(float2 uv);
    // float4 CustomPassLoadCustomColor(uint2 pixelCoords);
    // float LoadCustomDepth(uint2 pixelCoords);
    // float SampleCustomDepth(float2 uv);

    // There are also a lot of utility function you can use inside Common.hlsl and Color.hlsl,
    // you can check them out in the source code of the core SRP package.

    float3 camPos;

    float3 WorldToScreenPosAndZ(float3 position)
    {
            float3 camWPos = position-camPos;

            float4 positionCS = ComputeClipSpacePosition(camWPos, UNITY_MATRIX_VP);

            #if UNITY_UV_STARTS_AT_TOP
                    positionCS.y = -positionCS.y;
            #endif

            float clip = positionCS.w;
            positionCS *= rcp(positionCS.w);
            positionCS.xy = positionCS.xy * 0.5 + 0.5;

        return float3(positionCS.xy, clamp(sign(clip),0,1));
    }
    

    Buffer<float3> slimes;
    
    int slimesCount;
    

    float4 FullScreenPass(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
        float depth = LoadCameraDepth(varyings.positionCS.xy);
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        float3 viewDirection = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
        float4 color = float4(0.0, 0.0, 0.0, 0.0);

        // Load the camera color buffer at the mip 0 if we're not at the before rendering injection point
        if (_CustomPassInjectionPoint != CUSTOMPASSINJECTIONPOINT_BEFORE_RENDERING)
            color = float4(CustomPassLoadCameraColor(varyings.positionCS.xy, 0), 1);

        // Add your custom pass code here

        float intensity = 0;
        float4 slime = float4(0.3, 1, 0.3, 1);



        for (int i = 0; i < slimesCount; i++)
        {



            
            //float4 clipPos =  ComputeClipSpacePosition(camWPos, UNITY_MATRIX_VP);
            

            //float2 pos = ComputeNormalizedDeviceCoordinates(clipPos);
            //float2 pos = clipPos.xy/clipPos.w;
            float3 pos = WorldToScreenPosAndZ(slimes[i]);
            float3 scalePos = WorldToScreenPosAndZ(slimes[i]+float3(1,1,1));

            float2 scale = abs(scalePos.xy - pos.xy);
            
            

            //float2 pos = ComputeNormalizedDeviceCoordinates(, UNITY_MATRIX_VP);

            float2 pPos = posInput.positionNDC;
            float deltaX = scale.x- abs(pos.x - pPos.x);
            float deltaY = scale.y-  abs (pos.y-pPos.y);

            //float m = sqrt(pow(deltaX,2)+pow(deltaY,2));

            float d = distance(pos.xy, pPos);
            float ds = distance(pos.xy, scalePos.xy);
            

            float p = clamp(ds-d,0 ,1)*pos.z;

            intensity += p;
            
        }

        intensity = clamp(intensity, 0, 1);

        color = lerp(color, slime, intensity);
        

        // Fade value allow you to increase the strength of the effect while the camera gets closer to the custom pass volume
        float f = 1 - abs(_FadeValue * 2 - 1);
        return float4(color.rgb + f, color.a);
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Custom Pass 0"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
