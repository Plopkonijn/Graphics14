//------------------------------------------- Defines -------------------------------------------

#define Pi 3.14159265

//------------------------------------- Top Level Variables -------------------------------------

// Top level variables can and have to be set at runtime

// Matrices for 3D perspective projection 
float4x4 View, Projection, World, InvTrWorld;
float3 LightSource;
float3 EyePos;
float4 DiffuseColor, AmbientColor, SpecularColor;
float AmbientIntensity, SpecularIntensity, SpecularPower;
Texture2D Brick;

sampler TextureSampler = sampler_state { texture = <Brick> ; 
magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror; };



//---------------------------------- Input / Output structures ----------------------------------

// Each member of the struct has to be given a "semantic", to indicate what kind of data should go in
// here and how it should be treated. Read more about the POSITION0 and the many other semantics in 
// the MSDN library
struct VertexShaderInput
{
	float4 Position3D : POSITION0;
	float3 Normal: NORMAL0;
};
struct VertexShaderInputTexture
{
	float4 Position3D : POSITION0;
	float3 Normal: NORMAL0;
	float2 TextureCord: TEXCOORD0;
};

// The output of the vertex shader. After being passed through the interpolator/rasterizer it is also 
// the input of the pixel shader. 
// Note 1: The values that you pass into this struct in the vertex shader are not the same as what 
// you get as input for the pixel shader. A vertex shader has a single vertex as input, the pixel 
// shader has 3 vertices as input, and lets you determine the color of each pixel in the triangle 
// defined by these three vertices. Therefor, all the values in the struct that you get as input for 
// the pixel shaders have been linearly interpolated between there three vertices!
// Note 2: You cannot use the data with the POSITION0 semantic in the pixel shader.
struct VertexShaderOutput
{
	float4 Position2D : POSITION0;
	float3 Normal: TEXCOORD0;
	float4 Pos3D: TEXCOORD1;
};

struct VertexShaderOutputTexture
{
	float4 Position2D : POSITION0;
	float3 Normal: TEXCOORD0;
	float4 Pos3D: TEXCOORD1;
	float2 TextureCord: TEXCOORD2;
};

//------------------------------------------ Functions ------------------------------------------


//---------------------------------------- Technique: Simple ----------------------------------------



VertexShaderOutputTexture TextureVertexShader(VertexShaderInputTexture input)   //Vertexshader for the texture
{
	// Allocate an empty output struct
	VertexShaderOutputTexture output = (VertexShaderOutputTexture)0;

	// Do the matrix multiplications for perspective projection and the world transform
	float4 worldPosition = mul(input.Position3D, World);
    float4 viewPosition  = mul(worldPosition, View);
	output.Position2D    = mul(viewPosition, Projection);
	output.Pos3D = worldPosition;
	
	//float3x3 rotationAndScale = (float3x3) World;
	float3x3 rotationAndScale = (float3x3) InvTrWorld;
	
	output.Normal = mul(rotationAndScale, input.Normal);
	normalize(output.Normal);
	output.TextureCord = input.TextureCord;
	//output.Normal = input.Normal;

	return output;
}


float4 TexturePixelShader(VertexShaderOutputTexture input) : COLOR0  //pixelshader for the texture
{
	float4 c = tex2D(TextureSampler, input.TextureCord);
	float gr = 0.3*c.r + 0.59*c.g + 0.11*c.b;
	return (float4)(gr, gr, gr);
	
}

technique TextureQuad   //technique for the texture
{
	pass Pass0
	{
		cullmode = none;
		VertexShader = compile vs_2_0 TextureVertexShader();
		PixelShader  = compile ps_2_0 TexturePixelShader();
	}
}