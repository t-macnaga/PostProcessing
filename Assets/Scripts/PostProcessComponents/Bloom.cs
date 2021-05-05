using UnityEngine;

namespace PostProcess
{
    [CreateAssetMenu()]
    public class Bloom : PostProcessEffect
    {
        static readonly int FilterParamId = Shader.PropertyToID("_FilterParams");
        static readonly int ThresholdId = Shader.PropertyToID("_Threshold");
        static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int BloomTex1Id = Shader.PropertyToID("_BloomTex1");
        static readonly int BloomTex2Id = Shader.PropertyToID("_BloomTex2");
        static readonly int BloomTex3Id = Shader.PropertyToID("_BloomTex3");
        static readonly int BloomTex4Id = Shader.PropertyToID("_BloomTex4");
        static readonly int BloomTex5Id = Shader.PropertyToID("_BloomTex5");
        static readonly int BloomTex6Id = Shader.PropertyToID("_BloomTex6");
        static readonly int[] bloomTexIds = new int[]
            {
                BloomTex1Id,
                BloomTex2Id,
                BloomTex3Id,
                BloomTex4Id,
                BloomTex5Id,
                BloomTex6Id,
            };

        [SerializeField, Range(2, 6)] public int _iteration = 6;
        [SerializeField, Range(0.0f, 1.0f)] float _threshold = 0.0f;
        [SerializeField, Range(0.0f, 50.0f)] float _intensity = 1.0f;
        [SerializeField, Range(2, 4)] int _downSampleScale = 2;

        RenderTexture[] textures = new RenderTexture[6];
        Material bloomMaterial;

        public float Intensity
        {
            get => _intensity;
            set => _intensity = value;
        }

        void OnEnable()
        {
            bloomMaterial = new Material(Shader.Find("Hidden/PostEffect/Bloom"));
            bloomMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        void OnDisable()
        {
            DestroyImmediate(bloomMaterial);
        }

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
            bloomMaterial.SetFloat(IntensityId, _intensity);
            bloomMaterial.SetFloat(ThresholdId, _threshold);

            var width = sourceDesc.width / 2;
            var height = sourceDesc.height / 2;

            var pathIndex = 0;
            RenderTextureDescriptor currentDesc;

            context.CommandBuffer.BeginSample("Bloom");

            // Down Sample
            var iteration = 0;
            for (var i = 0; i < _iteration; i++)
            {
                if (width < 2 || height < 2) break;
                currentDesc = new RenderTextureDescriptor(width, height);
                pathIndex = i == 0 ? Constants.BloomExtractPass : Constants.BloomBlurPass;
                textures[i] = RenderTexture.GetTemporary(currentDesc);
                context.CommandBuffer.SetGlobalTexture(MainTexId, currentSource);
                context.CommandBuffer.Blit(currentSource, textures[i], bloomMaterial, pathIndex);
                bloomMaterial.SetTexture(bloomTexIds[i], textures[i]);
                currentSource = textures[i];
                width /= _downSampleScale;
                height /= _downSampleScale;
                iteration++;
            }
            context.CommandBuffer.Blit(context.Source, context.Dest, bloomMaterial, iteration);
            context.UberMaterial.SetTexture("_FinalBlurTex", context.Dest);
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