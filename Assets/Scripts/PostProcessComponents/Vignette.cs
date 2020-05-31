using UnityEngine;

namespace PostProcess
{
    public class Vignette : PostProcessEffect
    {
        [SerializeField, Range(0, 100)] int intensity = 25;
        [SerializeField, Range(0f, 1f)] float smoothness = 0.25f;

        int intensityId = Shader.PropertyToID("_VignetteIntensity");
        int smoothnessId = Shader.PropertyToID("_VignetteSmoothness");

        public override void Render(PostProcessContext context)
        {
            var cmd = context.CommandBuffer;
            if (IsEnabled)
            {
                context.UberMaterial.EnableKeyword("VIGNETTE");
                var _MainTex = Shader.PropertyToID("_MainTex");
                cmd.SetGlobalTexture(_MainTex, context.Source);
                context.UberMaterial.SetInt(intensityId, intensity);
                context.UberMaterial.SetFloat(smoothnessId, smoothness);
            }
            else
            {
                context.UberMaterial.DisableKeyword("VIGNETTE");
            }
        }
    }
}