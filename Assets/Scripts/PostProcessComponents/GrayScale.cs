using UnityEngine;
using UnityEngine.Rendering;

namespace PostProcess
{
    public class GrayScale : PostProcessEffect
    {
        static readonly int ShaderPass = 7;
        RenderTargetIdentifier destId = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);

        public override void Render(PostProcessContext context)
        {
            var cmd = context.CommandBuffer;
            if (IsEnabled)
            {
                context.UberMaterial.EnableKeyword("GRAYSCALE");
                var _MainTex = Shader.PropertyToID("_MainTex");
                cmd.SetGlobalTexture(_MainTex, context.Source);
            }
            else
            {
                context.UberMaterial.DisableKeyword("GRAYSCALE");
            }
        }
    }
}