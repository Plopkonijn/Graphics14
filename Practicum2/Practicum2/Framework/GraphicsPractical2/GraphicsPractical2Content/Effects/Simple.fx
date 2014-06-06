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

//------------------------------------------ Functions ------------------------------------------

// Implement the Coloring using normals assignment here
float4 NormalColor(float3 Normal)
{
	Normal = (Normal+1)*0.5;
	return float4(Normal, 1);
}

// Implement the Procedural texturing assignment here
float4 ProceduralColor(float3 Normal, float4 Pos3D)
{
	//int factor = ( abs(floor(Pos3D.x*0.5)) + abs(floor(Pos3D.y*0.5)) ) % 2;
	//Normal = Normal*(factor*2-1);
	return float4(Normal, 1);
}

//---------------------------------------- Technique: Simple ----------------------------------------

float4 PhongColor( float4 pos3D, float3 normal )
{
	normal = normalize(normal);
	float3 LightDir = normalize(LightSource - pos3D);
	float3 ViewDir =  normalize(EyePos - pos3D);
	float3 H = normalize(LightDir + ViewDir);
	float distance = pow(length(LightSource - pos3D),0.1);
	float NdotH = dot(normal, H);
	float intensity = pow(saturate(NdotH), SpecularPower);

	return (intensity * SpecularColor * SpecularIntensity / distance); 
}

float4 LambertColor(float4 color, float4 pos3D, float3 normal)   //Lambert + Ambient
{
		
    //float intensity = 6000/(pow(length(LightSource-pos3D),2));
	float intensity = 1;

	//color = PhongColor(pos3D, normal);
	color=color*intensity*max(0,dot(normal,normalize(LightSource-pos3D))) + AmbientIntensity*AmbientColor + PhongColor(pos3D, normal);
	
	return color;
}




VertexShaderOutput SimpleVertexShader(VertexShaderInput input)
{
	// Allocate an empty output struct
	VertexShaderOutput output = (VertexShaderOutput)0;

	// Do the matrix multiplications for perspective projection and the world transform
	float4 worldPosition = mul(input.Position3D, World);
    float4 viewPosition  = mul(worldPosition, View);
	output.Position2D    = mul(viewPosition, Projection);
	output.Pos3D = worldPosition;
	
	//float3x3 rotationAndScale = (float3x3) World;
	float3x3 rotationAndScale = (float3x3) InvTrWorld;
	
	output.Normal = mul(rotationAndScale, input.Normal);
	normalize(output.Normal);

	//output.Normal = input.Normal;

	return output;
}

float4 SimplePixelShader(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(TextureSampler,);
	color = color * LambertColor(DiffuseColor, input.Pos3D,normalize(input.Normal));
	return color
}

technique Simple
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 SimpleVertexShader();
		PixelShader  = compile ps_2_0 SimplePixelShader();
	}
}