#pragma kernel RenderGrass

#include "UnityCG.cginc"

// Define the structure for each grass blade
struct GrassBlade
{
    float3 position;
    float3 color;
};

// Define the buffer that holds the grass data
StructuredBuffer<GrassBlade> grassBuffer;

// Define the output texture to render the grass
RWTexture2D<float4> result;

// Define the dimensions of the grass texture
uint2 grassTextureDimensions;

// Define the size of each grass blade
float grassBladeSize = 0.05;

// Define the color of the grass
float3 grassColor = float3(0.2, 0.8, 0.2);

[numthreads(16, 16, 1)]
void RenderGrass(uint3 id : SV_DispatchThreadID)
{
    // Calculate the UV coordinates of the pixel
    float2 uv = id.xy / float2(grassTextureDimensions);
    
    // Get the position of the pixel in world space
    float3 position = float3(uv.x, 0.0, uv.y);
    
    // Get the length of the grass buffer
    uint grassCount = grassBuffer.Length();
    
    // Iterate through all grass blades in the buffer
    for (uint i = 0; i < grassCount; i++)
    {
        // Get the current grass blade from the buffer
        GrassBlade blade = grassBuffer[i];
        
        // Calculate the distance from the current pixel to the grass blade
        float distance = distance(position, blade.position);
        
        // If the distance is within the grass blade size, color the pixel as grass
        if (distance <= grassBladeSize)
        {
            result[id.xy] = float4(blade.color * grassColor, 1.0);
            return;
        }
    }
    
    // If no grass blade was found, color the pixel as transparent
    result[id.xy] = float4(0.0, 0.0, 0.0, 0.0);
}
