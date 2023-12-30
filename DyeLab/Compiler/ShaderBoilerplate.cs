﻿namespace DyeLab.Compiler;

public static class ShaderBoilerplate
{
    public const string DefaultParameters = @"sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uSaturation;
float uOpacity;
float uTime;
float uRotation;
float uDirection;
float4 uSourceRect;
float4 uLegacyArmorSourceRect;
float2 uTargetPosition;
float2 uWorldPosition;
float2 uImageSize0;
float2 uLegacyArmorSheetSize;
float2 uImageSize1;
float3 uLightSource;
float4 uShaderSpecificData;";

    public const string DefaultPassSignature =
        @"float4 Base(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0";

    public const string DefaultTechniqueDefinition = @"technique Technique1
{	
   	pass Base
	{
		PixelShader = compile ps_2_0 Base();
	}
}";
}