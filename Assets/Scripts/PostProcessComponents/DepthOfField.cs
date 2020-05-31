using UnityEngine;

namespace PostProcess
{
    [CreateAssetMenu()]
    public class DepthOfField : PostProcessEffect
    {
        [SerializeField] float depth;
        [SerializeField] float radious;

        int _MainTex;
        int _BlurTex;
        int _Depth = Shader.PropertyToID("_Depth");
        int _Radius = Shader.PropertyToID("_Radius");
        int rt1Id = Shader.PropertyToID("_rt1");

        void OnEnable()
        {
            _BlurTex = Shader.PropertyToID("_BlurTex");
            _MainTex = Shader.PropertyToID("_MainTex");
        }

        public override void Render(PostProcessContext context)
        {
            if (!IsEnabled)
            {
                context.UberMaterial.DisableKeyword("DOF");
                return;
            }

            context.UberMaterial.EnableKeyword("DOF");
            context.UberMaterial.SetFloat(_Depth, depth);
            context.UberMaterial.SetFloat(_Radius, radious);
            var desc = new RenderTextureDescriptor(context.Source.width / 2, context.Source.height / 2);
            context.CommandBuffer.GetTemporaryRT(rt1Id, desc);
            var cmd = context.CommandBuffer;
            // Scale Down
            cmd.Blit(context.Source, rt1Id, context.UberMaterial, Constants.DofBlurPass);
            cmd.SetGlobalTexture(_MainTex, context.Source);
            cmd.SetGlobalTexture(_BlurTex, rt1Id);
        }
    }
}