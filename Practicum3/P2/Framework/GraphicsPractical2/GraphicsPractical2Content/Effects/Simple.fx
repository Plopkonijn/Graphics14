//------------------------------------------- Defines -------------------------------------------

#define Pi 3.14159265

//------------------------------------- Top Level Variables -------------------------------------

// Top level variables can and have to be set at runtime

// Matrices for 3D perspective projection 
float4x4 View, Projection, World, InvTrWorld;
#define MAXLIGHTS 5
float3 LightSource[MAXLIGHTS];
float3 EyePos;
float4 DiffuseColor, AmbientColor;
float4 SpecularColor[MAXLIGHTS];
float AmbientIntensity, SpecularIntensity, SpecularPower;
Texture2D Brick;
float cosine_alpha, cosine_beta;

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

// Implement the Coloring using normals assignment here
float4 NormalColor(float3 Normal)
{
	Normal = (Normal+1)*0.5;
	return float4(Normal, 1);
}

// Implement the Procedural texturing assignment here
float4 ProceduralColor(float3 Normal, float4 Pos3D)
{
	//checkerboard pattern:
	//int factor = ( abs(floor(Pos3D.x*0.5)) + abs(floor(Pos3D.y*0.5)) ) % 2; 
	//Normal = Normal*(factor*2-1);
	return float4(Normal, 1);
}

float4 CelColor(float4 color,float cells)
{
	//nog iets toevoegen met ddx en ddy
	//misschien in de bovenliggende methode
	color = color*cells;
	color = color - frac(color);
	color = color /cells;
	return color;
}

//---------------------------------------- Technique: Simple ----------------------------------------

float4 PhongColor( float4 pos3D, float3 normal, int teller )   //returns Blinn-Phong shading
{
	//normalize vectors
	normal = normalize(normal);
	float3 LightDir = normalize(LightSource[teller] - pos3D);
	float3 ViewDir =  normalize(EyePos - pos3D);
	float3 H = normalize(LightDir + ViewDir);   //halfvector between the lightdirection and the viewdirection.

	float distance = pow(length(LightSource[teller] - pos3D),0.1);
	float NdotH = dot(normal, H);   //use the dotproduct to calculate the intensity
	float intensity = pow(saturate(NdotH), SpecularPower);

	return (intensity * SpecularColor[teller] * SpecularIntensity / distance); 
}

float4 LambertColor(float4 color, float4 pos3D, float3 normal, int teller)   //Lambert + Ambient
{
		//Using the dotproduct between the lightdirection and the normal we can calculate the intensity
		//for Lambert shading
		color=color*max(0,dot(normal,normalize(LightSource[teller]-pos3D)));
		float distance = pow(length(LightSource[teller] - pos3D), 0.2);
		color = color/distance;

		//Blinn-Phong shading is added up
		color = color + PhongColor(pos3D, normal, teller);

	return color;
}

VertexShaderOutput SimpleVertexShader(VertexShaderInput input)      //Vertexshader for the teapot
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
	
	//rotates and scales the normal vectors in the right way
	output.Normal = mul(rotationAndScale, input.Normal);
	normalize(output.Normal);

	//output.Normal = input.Normal;

	return output;
}

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

float4 SimplePixelShader(VertexShaderOutput input) : COLOR0    //pixelshader for the teapot
{
	float4 color = 0;

	[loop]
	for( uint i = 0; i < 3; i++ )
	{
		color = saturate(color + LambertColor(DiffuseColor, input.Pos3D,normalize(input.Normal), i));
	}
			
		//Ambient is added to the Lambert shading
		color=color + AmbientIntensity*AmbientColor;
		
		
		color = CelColor(color,2);


	return color;
}



float4 TexturePixelShader(VertexShaderOutputTexture input) : COLOR0  //pixelshader for the texture
{
	return tex2D(TextureSampler, input.TextureCord);
}

float4 SpotlightPixelShader(VertexShaderOutput input) : COLOR0    //pixelshader for the teapot
{
	float4 color = 0;

	[loop]
	for( uint i = 0; i < 1; i++ )
	{
		
		float factor = dot(normalize(LightSource[i]), normalize(LightSource[i]- input.Pos3D ));
		factor = saturate(max(factor-cosine_beta,0)/(cosine_alpha-cosine_beta));
	    color = saturate(color + factor*LambertColor(DiffuseColor, input.Pos3D,normalize(input.Normal), i));


		
		
	}
	//Ambient is added to the Lambert shading
	color = color + AmbientIntensity*AmbientColor;
	//color = CelColor(color,2);
	return color;
}

technique Simple  //technique for the teapot
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 SimpleVertexShader();
		PixelShader  = compile ps_3_0 SimplePixelShader();
	}
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



technique Spotlight  //technique for the spotlight
{
	pass Pass0
	{
		cullmode = none;
		VertexShader = compile vs_2_0 SimpleVertexShader();
		PixelShader  = compile ps_2_0 SpotlightPixelShader();
	}
}