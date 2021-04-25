using UnityEngine;

namespace PostProcess
{
    [CreateAssetMenu()]
    public class Bloom : PostProcessEffect
    {
        // [SerializeField, Range(1, 6)] public int _iteration = 1;
        [SerializeField, Range(0.0f, 1.0f)] float _threshold = 0.0f;
        [SerializeField, Range(0.0f, 1.0f)] float _softThreshold = 0.0f;
        [SerializeField, Range(0.0f, 50.0f)] float _intensity = 1.0f;
        [SerializeField] bool bloomHQ = true;
        [SerializeField] bool _debug;

        RenderTexture[] textures = new RenderTexture[6];
        RenderTextureDescriptor[] descs = new RenderTextureDescriptor[6];

        public override void Render(PostProcessContext context)
        {
            if (!IsEnabled)
            {
                context.UberMaterial.DisableKeyword("BLOOM");
                return;
            }
            context.UberMaterial.EnableKeyword("BLOOM");

            var sourceDesc = new RenderTextureDescriptor(context.Source.width, context.Source.height);
            var currentSource = context.Source;
            var filterParams = Vector4.zero;
            var knee = _threshold * _softThreshold;
            filterParams.x = _threshold;
            filterParams.y = _threshold - knee;
            filterParams.z = knee * 2.0f;
            filterParams.w = 0.25f / (knee + 0.00001f);
            context.UberMaterial.SetVector("_FilterParams", filterParams);
            context.UberMaterial.SetFloat("_Intensity", _intensity);
            context.UberMaterial.SetTexture("_SourceTex", context.Source);

            var width = sourceDesc.width;
            var height = sourceDesc.height;

            var pathIndex = 0;
            var i = 0;
            RenderTextureDescriptor currentDesc;

            context.CommandBuffer.BeginSample("Bloom");

            var iterations = bloomHQ ? 6 : 3;
            // Down Sample
            for (; i < iterations; i++)
            {
                if (width < 2 || height < 2) break;
                currentDesc = descs[i] = new RenderTextureDescriptor(width, height);
                pathIndex = i == 0 ? Constants.BloomExtractPass : Constants.BloomBlurPass;
                textures[i] = RenderTexture.GetTemporary(currentDesc);
                context.CommandBuffer.SetGlobalTexture("_MainTex", currentSource);
                context.CommandBuffer.Blit(currentSource, textures[i], context.UberMaterial, pathIndex);
                currentSource = textures[i];
                width /= 2;
                height /= 2;
            }

            context.UberMaterial.SetTexture("_BloomTex1", textures[0]);
            context.UberMaterial.SetTexture("_BloomTex2", textures[1]);
            context.UberMaterial.SetTexture("_BloomTex3", textures[2]);
            if (bloomHQ)
            {
                context.UberMaterial.EnableKeyword("BLOOM_HQ");
                context.UberMaterial.DisableKeyword("BLOOM_LQ");
                context.UberMaterial.SetTexture("_BloomTex4", textures[3]);
                context.UberMaterial.SetTexture("_BloomTex5", textures[4]);
                context.UberMaterial.SetTexture("_BloomTex6", textures[5]);
            }
            else
            {
                context.UberMaterial.EnableKeyword("BLOOM_LQ");
                context.UberMaterial.DisableKeyword("BLOOM_HQ");
            }
            context.CommandBuffer.Blit(context.Source, context.Dest, context.UberMaterial, Constants.BloomCombinePass);
            context.CommandBuffer.EndSample("Bloom");
        }

        public override void FrameCleanup()
        {
            for (var idx = 0; idx < textures.Length; idx++)
            {
                RenderTexture.ReleaseTemporary(textures[idx]);
            }
        }
    }
}