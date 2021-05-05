using UnityEngine;

namespace PostProcess
{
    [CreateAssetMenu()]
    public class DepthOfField : PostProcessEffect
    {
        [SerializeField] float depth;
        [SerializeField] float radious;
        [SerializeField] bool useFakeDof;

        int _MainTex;
        int _BlurTex;
        int _Depth = Shader.PropertyToID("_Depth");
        int _Radius = Shader.PropertyToID("_Radius");
        int rt1Id = Shader.PropertyToID("_rt1");
        bool enabledDepthTextureMode;
        public float Depth
        {
            get => depth;
            set => depth = value;
        }
        bool UseDepthTexture
        {
            get
            {
                if (useFakeDof) return false;
                return true;
            }
        }
        string FakeDofShaderKeyword = "FAKE_DOF";
        string DofShaderKeyword = "DOF";

        void OnEnable()
        {
            _BlurTex = Shader.PropertyToID("_BlurTex");
            _MainTex = Shader.PropertyToID("_MainTex");
        }

        public override void Render(PostProcessContext context)
        {
            if (!IsEnabled)
            {
                if (enabledDepthTextureMode)
                {
                    enabledDepthTextureMode = false;
                    context.DisableDepthTextureMode();
                }
                context.UberMaterial.DisableKeyword(DofShaderKeyword);
                context.UberMaterial.DisableKeyword(FakeDofShaderKeyword);
                return;
            }

            if (enabledDepthTextureMode && !UseDepthTexture)
            {
                enabledDepthTextureMode = false;
                context.DisableDepthTextureMode();
            }
            if (UseDepthTexture && !enabledDepthTextureMode)
            {
                enabledDepthTextureMode = true;
                context.EnableDepthTextureMode();
            }
            if (useFakeDof)
            {
                context.UberMaterial.EnableKeyword(FakeDofShaderKeyword);
                context.UberMaterial.DisableKeyword(DofShaderKeyword);
            }
            else
            {
                context.UberMaterial.EnableKeyword(DofShaderKeyword);
                context.UberMaterial.DisableKeyword(FakeDofShaderKeyword);
            }
            context.UberMaterial.SetFloat(_Depth, depth);
            context.UberMaterial.SetFloat(_Radius, radious);
            var desc = new RenderTextureDescriptor(context.Source.width, context.Source.height + 1);
            context.CommandBuffer.GetTemporaryRT(rt1Id, desc);
            var cmd = context.CommandBuffer;
            // Scale Down
            cmd.Blit(context.Source, rt1Id, context.UberMaterial, Constants.DofBlurPass);
            cmd.SetGlobalTexture(_MainTex, context.Source);
            cmd.SetGlobalTexture(_BlurTex, rt1Id);
        }

        public override bool HasDepthTextureMode() => IsEnabled && !useFakeDof;
    }
}