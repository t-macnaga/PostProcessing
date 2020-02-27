using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PostProcess
{
    public class ChromaticAberration : PostProcessEffect
    {
        static readonly int ShaderPass = 7;
        int _Size = Shader.PropertyToID("_ChromaticAberrationSize");
        [SerializeField, Range(0f, 0.1f)] float size = 0.025f;

        RenderTargetIdentifier destId = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);

        public override void Render(PostProcessContext context)
        {
            var cmd = context.CommandBuffer;
            if (IsEnabled)
            {
                context.UberMaterial.EnableKeyword("CHROMATIC_ABERRATION");
                var _MainTex = Shader.PropertyToID("_MainTex");
                cmd.SetGlobalTexture(_MainTex, context.Source);//SourceId);

                context.UberMaterial.SetFloat(_Size, size);
                // cmd.Blit(context.SourceId, destId, context.UberMaterial, ShaderPass);
            }
            else
            {
                context.UberMaterial.DisableKeyword("CHROMATIC_ABERRATION");
            }
        }
    }
}
