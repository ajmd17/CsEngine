﻿using ApexEngine.Assets;
using ApexEngine.Rendering;
using System;

namespace ApexEngine.Terrain
{
    public class TerrainShader : Shader
    {
        private static Assets.ShaderTextLoader textLoader = Assets.ShaderTextLoader.GetInstance();

        public TerrainShader(ShaderProperties properties)
            : base(properties, (string)AssetManager.Load(AppDomain.CurrentDomain.BaseDirectory + "\\shaders\\terrain.vert"),
                               (string)AssetManager.Load(AppDomain.CurrentDomain.BaseDirectory + "\\shaders\\terrain.frag"))
        {
        }

        public override void ApplyMaterial(Material material)
        {
            base.ApplyMaterial(material);

            this.SetUniform(MATERIAL_AMBIENTCOLOR, material.GetVector4f(Material.COLOR_AMBIENT));
            this.SetUniform(MATERIAL_DIFFUSECOLOR, material.GetVector4f(Material.COLOR_DIFFUSE));
            this.SetUniform(MATERIAL_SPECULARCOLOR, material.GetVector4f(Material.COLOR_SPECULAR));
            this.SetUniform(MATERIAL_SHININESS, material.GetFloat(Material.SHININESS));
            this.SetUniform(MATERIAL_ROUGHNESS, material.GetFloat(Material.ROUGHNESS));
            this.SetUniform(MATERIAL_SPECULARTECHNIQUE, material.GetInt(Material.TECHNIQUE_SPECULAR));
            this.SetUniform(MATERIAL_SPECULAREXPONENT, material.GetFloat(Material.SPECULAR_EXPONENT));
            this.SetUniform(MATERIAL_PERPIXELLIGHTING, material.GetInt(Material.TECHNIQUE_PER_PIXEL_LIGHTING));
        }

        public override void Update(Rendering.Environment environment, Camera cam, Mesh mesh)
        {
            base.Update(environment, cam, mesh);

            environment.DirectionalLight.BindLight(0, this);
            environment.AmbientLight.BindLight(0, this);
            for (int i = 0; i < environment.PointLights.Count; i++)
            {
                environment.PointLights[i].BindLight(i, this);
            }


            SetUniform(ENV_NUMPOINTLIGHTS, environment.PointLights.Count);
            SetUniform(ENV_FOGSTART, environment.FogStart);
            SetUniform(ENV_FOGEND, environment.FogEnd);
            SetUniform(ENV_FOGCOLOR, environment.FogColor);

            if (currentMaterial != null)
            {


                Texture diffuseTex0 = currentMaterial.GetTexture(TerrainMaterial.TEXTURE_DIFFUSE0);
                if (diffuseTex0 != null)
                {
                    Texture.ActiveTextureSlot(0);
                    diffuseTex0.Use();
                    SetUniform("terrainTexture0", 0);

                    float scale = 16f;
                   /* if (currentMaterial.GetFloat(TerrainMaterial.TEXTURE_SCALE_0) > 0)
                    {
                        scale = currentMaterial.GetFloat(TerrainMaterial.TEXTURE_SCALE_0);
                    }*/
                    SetUniform("terrainTexture0Scale", scale);
                }

                Texture normalTex0 = currentMaterial.GetTexture(TerrainMaterial.TEXTURE_NORMAL0);
                if (normalTex0 != null)
                {
                    Texture.ActiveTextureSlot(1);
                    normalTex0.Use();
                    SetUniform("terrainTexture0Normal", 1);
                    SetUniform("terrainTexture0HasNormal", 1);
                }
                else
                {
                    SetUniform("terrainTexture0HasNormal", 0);
                }

                Texture slopeTex = currentMaterial.GetTexture(TerrainMaterial.TEXTURE_DIFFUSE_SLOPE);
                if (slopeTex != null)
                {
                    Texture.ActiveTextureSlot(9);
                    slopeTex.Use();
                    SetUniform("slopeTexture", 9);

                    float scale = 16f;
                  /*  if (currentMaterial.GetFloat(TerrainMaterial.TEXTURE_SCALE_SLOPE) > 0)
                    {
                        scale = currentMaterial.GetFloat(TerrainMaterial.TEXTURE_SCALE_SLOPE);
                    }*/
                    SetUniform("slopeScale", scale);
                }

                Texture slopeNormalTex = currentMaterial.GetTexture(TerrainMaterial.TEXTURE_NORMAL_SLOPE);
                if (slopeNormalTex != null)
                {
                    Texture.ActiveTextureSlot(10);
                    slopeNormalTex.Use();
                    SetUniform("slopeTextureNormal", 10);
                    SetUniform("slopeTextureHasNormal", 1);
                }
                else
                {
                    SetUniform("slopeTextureHasNormal", 0);
                }

                Texture splatTex = currentMaterial.GetTexture(TerrainMaterial.TEXTURE_SPLAT);
                if (splatTex != null)
                {
                    Texture.ActiveTextureSlot(11);
                    splatTex.Use();
                    SetUniform("splatTexture", 11);
                    SetUniform("hasSplatMap", 1);
                }
                else
                {
                    SetUniform("hasSplatMap", 0);
                }
            }
            if (environment.ShadowsEnabled)
            {
                SetUniform("Env_ShadowsEnabled", 1);
                for (int i = 0; i < 4; i++)
                {
                    Texture.ActiveTextureSlot(3 + i);
                    environment.ShadowMaps[i].Use();
                    SetUniform("Env_ShadowMap" + i.ToString(), 3 + i);
                    SetUniform("Env_ShadowMatrix" + i.ToString(), environment.ShadowMatrices[i]);
                    SetUniform("Env_ShadowMapSplits[" + i.ToString() + "]", environment.ShadowMapSplits[i]);
                }
            }
        }
    }
}