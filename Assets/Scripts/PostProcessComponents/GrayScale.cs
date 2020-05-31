using UnityEngine;

namespace PostProcess
{
    public class GrayScale : PostProcessEffect
    {
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