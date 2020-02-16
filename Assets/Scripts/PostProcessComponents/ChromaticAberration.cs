using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ChromaticAberration : PostProcessEffect, IPostProcessComponent
{
    static readonly int ShaderPass = 7;
    RenderTargetIdentifier destId = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);

    public override void Render(PostProcessContext context)
    {
        var cmd = context.CommandBuffer;
        if (IsEnabled)
        {
            context.UberMaterial.EnableKeyword("CHROMATIC_ABERRATION");
            var _MainTex = Shader.PropertyToID("_MainTex");
            cmd.SetGlobalTexture(_MainTex, context.SourceId);
            // cmd.Blit(context.SourceId, destId, context.UberMaterial, ShaderPass);
        }
        else
        {
            context.UberMaterial.DisableKeyword("CHROMATIC_ABERRATION");
        }

    }
}


