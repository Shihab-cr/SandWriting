

     
            struct TessellationFactors{
                float edgeFactors[3] : SV_TessFactor;
                float insideFactor : SV_InsideTessFactor;
            };

            [domain("tri")]
            [patchconstantfunc("patchConstantFunction")]
            [partitioning("fractional_odd")]
            [outputtopology("triangle_cw")]
            [outputcontrolpoints(3)]
            
            PackedVaryings Hull(InputPatch<PackedVaryings,3> patch,uint id : SV_OutputControlPointID)
            {
                return patch[id];
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


            float CalcTextureChangeTessFactor(float2 uv){
                const float pi = 3.14159;
                float maxFactor = _MinTess;
                float4 sampleCenter = SAMPLE_TEXTURE2D_LOD(_BaseMap,sampler_BaseMap,uv,0);
                float degreeIncrement = 2*pi / _RotationalIteration;
                for(int i=0;i<_RotationalIteration;i++){
                    float rad = degreeIncrement * i;
                    half2 offset = half2(cos(rad), sin(rad))*_CheckDist;
                    float4 sm2 = SAMPLE_TEXTURE2D_LOD(_BaseMap, sampler_BaseMap, uv+offset,0);
                    float delta = length(sampleCenter - sm2);
                    float tessFactor = 0.5 * delta * _ProxmityTessFactor;

                    maxFactor = max(maxFactor,tessFactor);
                }
                return maxFactor;  
            }
            //main controller, gets the world space of the verts, then gets the distTessFactors to decide whether to draw them or not
            // then passes the calculation to the edges, so that the edges can be divided according the verts that won't show up
            TessellationFactors DistanceBasedTess(PackedVaryings vert0,PackedVaryings vert1, PackedVaryings vert2){
                float3 vertexTessFactor;
                

                vertexTessFactor.x = CalcDistTessFactors(vert0.positionWS) * CalcTextureChangeTessFactor(vert0.texCoord0.xy);
                vertexTessFactor.y = CalcDistTessFactors(vert1.positionWS) * CalcTextureChangeTessFactor(vert1.texCoord0.xy);
                vertexTessFactor.z = CalcDistTessFactors(vert2.positionWS) * CalcTextureChangeTessFactor(vert2.texCoord0.xy);

                return CalcTriEdgeTessDistFactor(vertexTessFactor); 
            } 

            //what the hardware calls
            TessellationFactors patchConstantFunction(InputPatch<PackedVaryings, 3> patch){
                
                return DistanceBasedTess(patch[0],patch[1],patch[2]);
            }

			
            void vert(inout PackedVaryings IN)
            {
                
                float4 color = SAMPLE_TEXTURE2D_LOD(_BaseMap, sampler_BaseMap, IN.texCoord0.xy,0);
                color = 1-color;
                
				float stepSize = 0.01;
				float hCenter = (1-SAMPLE_TEXTURE2D_LOD(_BaseMap,sampler_BaseMap,IN.texCoord0.xy,0).g)*_HightDeformation;
				float hRight = (1 - SAMPLE_TEXTURE2D_LOD(_BaseMap,sampler_BaseMap,IN.texCoord0.xy + float2(stepSize,0),0).g)*_HightDeformation;
				float hForward = (1 - SAMPLE_TEXTURE2D_LOD(_BaseMap,sampler_BaseMap,IN.texCoord0.xy+float2(0,stepSize),0).g)*_HightDeformation;

                IN.positionWS += IN.normalWS * hCenter;
			#if defined(VARYINGS_NEED_TANGENT_WS)
				float3 bitangent = cross(IN.normalWS, IN.tangentWS.xyz)* IN.tangentWS.w;
				float3 modifiedTangent = IN.tangentWS.xyz + IN.normalWS*(hRight-hCenter);
				float3 modifiedBiTangent = bitangent + IN.normalWS * (hForward - hCenter);
				IN.normalWS = normalize(cross(modifiedTangent,modifiedBiTangent));
			#endif
				float3 objectPos = TransformWorldToObject(IN.positionWS);
				IN.positionCS = TransformObjectToHClip(objectPos);
                
            }

            
            #define Interpolate(fieldName) data.fieldName = patch[0].fieldName*barycentricCoordinates.x + \
                    patch[1].fieldName*barycentricCoordinates.y +\
                    patch[2].fieldName*barycentricCoordinates.z;
            
            [domain("tri")]
            PackedVaryings domain(TessellationFactors factors, OutputPatch<PackedVaryings,3> patch,float3 barycentricCoordinates : SV_DomainLocation){
                PackedVaryings data;
				ZERO_INITIALIZE(PackedVaryings,data);

                Interpolate(positionWS);
				Interpolate(positionCS);
				Interpolate(normalWS);
				#if defined(VARYINGS_NEED_TANGENT_WS)
                Interpolate(tangentWS);
				#endif
				Interpolate(texCoord0);
                
                
				 vert(data);
				 return data;
            }
