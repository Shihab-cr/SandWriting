Shader "Custom/Tessellation"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _TessellationFactor("TessellationFactor",float) = 1
        _MaxTessellationFactor("Tessellation distance",float) = 20
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex DummyVert
            #pragma fragment frag
            #pragma hull Hull
            #pragma domain domain


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float _TessellationFactor;
                float _MaxTessellationFactor;
            CBUFFER_END
            
            struct TessellationFactors{
                float edgeFactors[3] : SV_TessFactor;
                float insideFactor : SV_InsideTessFactor;
            };

            [domain("tri")]
            [patchconstantfunc("patchConstantFunction")]
            [partitioning("fractional_odd")]
            [outputtopology("triangle_cw")]
            [outputcontrolpoints(3)]
            
            Attributes Hull(InputPatch<Attributes,3> patch,uint id : SV_OutputControlPointID)
            {
                return patch[id];
            }

             Attributes DummyVert(Attributes IN){
                return IN;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            
            #define Interpolate(fieldName) data.fieldName = patch[0].fieldName*barycentricCoordinates.x + \
                    patch[1].fieldName*barycentricCoordinates.y +\
                    patch[2].fieldName*barycentricCoordinates.z;
            
            [domain("tri")]
            Varyings domain(TessellationFactors factors, OutputPatch<Attributes,3> patch,float3 barycentricCoordinates : SV_DomainLocation){
                Attributes data;
                Interpolate(positionOS);
                Interpolate(uv);
                return vert(data);
            }

            TessellationFactors CalcTriEdgeTessDistFactor(float3 vertexTessFactor){
                TessellationFactors factors;
                factors.edgeFactors[0] = 0.5 * (vertexTessFactor.y + vertexTessFactor.z);
                factors.edgeFactors[1] = 0.5 * (vertexTessFactor.x + vertexTessFactor.z );
                factors.edgeFactors[2] = 0.5 * (vertexTessFactor.x + vertexTessFactor.y);
                factors.insideFactor = (vertexTessFactor.x + vertexTessFactor.y + vertexTessFactor.z)/3;

                return factors;
            }

            float CalcDistTessFactors(float3 worldPos){
                const float minDist = 2;
                float dist = distance(worldPos,_WorldSpaceCameraPos);
                float factor = clamp(1 - (dist-minDist)/(_MaxTessellationFactor - minDist),0.01,1);
                return clamp(factor * _TessellationFactor, 0, _TessellationFactor);
            }

            //main controller, gets the world space of the verts, then gets the distTessFactors to decide whether to draw them or not
            // then passes the calculation to the edges, so that the edges can be divided according the verts that won't show up
            TessellationFactors DistanceBasedTess(Attributes vert0,Attributes vert1, Attributes vert2){
                float3 vertexTessFactor;
                float3 pos0 = TransformObjectToWorld(vert0.positionOS);
                float3 pos1 = TransformObjectToWorld(vert1.positionOS);
                float3 pos2 = TransformObjectToWorld(vert2.positionOS);

                vertexTessFactor.x = CalcDistTessFactors(pos0);
                vertexTessFactor.y = CalcDistTessFactors(pos1);
                vertexTessFactor.z = CalcDistTessFactors(pos2);

                return CalcTriEdgeTessDistFactor(vertexTessFactor); 
            } 

            //what the hardware calls
            TessellationFactors patchConstantFunction(InputPatch<Attributes, 3> patch){
                
                return DistanceBasedTess(patch[0],patch[1],patch[2]);
            }


          
            float4 frag(Varyings IN) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                return color;
            }
            ENDHLSL
        }
    }
}
