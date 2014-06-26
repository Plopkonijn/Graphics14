using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GraphicsPractical2
{
    /// <summary>
    /// This struct can be used to make interaction with the effects easier.
    /// To use this, create a new material and set all the variables you want to share with the effect.
    /// Then call the SetEffectParameters to set the globals of the effect given using the parameter.
    /// Make sure to comment all the lines that set effect parameters that are currently not existing in your effect file.
    /// </summary>
    public struct Material
    {
        // Color of the ambient light
        public Color AmbientColor;
        // Intensity of the ambient light
        public float AmbientIntensity;
        // The color of the surface (can be ignored if texture is used, or not if you want to blend)
        public Color DiffuseColor;
        // The texture of the surface
        public Texture DiffuseTexture;
        // The normal displacement texture
        public Texture NormalMap;
        // The normal displacement factor, to apply the effect in a subtle manner
        public float DisplacementFactor;
        // Color of the specular highlight (mostly equal to the color of the light source)
        public Color SpecularColor;
        // The intensity factor of the specular highlight, controls it's size
        public float SpecularIntensity;
        // The power term of the specular highlight, controls it's smoothness
        public float SpecularPower;
        // Special surface color, use normals as color
        public bool NormalColoring;
        // Special surface color, procedural colors
        public bool ProceduralColoring;

        // Using this function requires all these elements to be present as top-level variables in the shader code. Comment out the ones that you don't use
        public void SetEffectParameters(Effect effect)
        {

            effect.Parameters["DiffuseColor"].SetValue(this.DiffuseColor.ToVector4());
            effect.Parameters["AmbientColor"].SetValue(this.AmbientColor.ToVector4());
            effect.Parameters["AmbientIntensity"].SetValue(this.AmbientIntensity);
            //effect.Parameters["SpecularColor"].SetValue(this.SpecularColor.ToVector4());
            effect.Parameters["SpecularIntensity"].SetValue(this.SpecularIntensity);
            effect.Parameters["SpecularPower"].SetValue(this.SpecularPower);
            /*effect.Parameters["DiffuseTexture"].SetValue(this.DiffuseTexture);
            effect.Parameters["NormalMap"].SetValue(this.NormalMap);
            effect.Parameters["DisplacementFactor"].SetValue(this.DisplacementFactor);
            
            effect.Parameters["NormalColoring"].SetValue(this.NormalColoring);
            effect.Parameters["ProceduralColoring"].SetValue(this.ProceduralColoring);

            effect.Parameters["HasTexture"].SetValue(this.DiffuseTexture != null);
            effect.Parameters["HasNormalMap"].SetValue(this.NormalMap != null);*/
        }
    }
}