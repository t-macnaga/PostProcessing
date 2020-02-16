using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu()]
public class DepthOfField : PostProcessEffect, IPostProcessComponent
{
    [SerializeField] float depth;
    [SerializeField] float radious;
    static readonly int GaussianBlurShaderPass = 5;
    static readonly int DofShaderPass = 6;

    int _MainTex;
    int _Direction;
    int _BlurTex;
    int _Depth = Shader.PropertyToID("_Depth");
    int _Radius = Shader.PropertyToID("_Radius");
    int rt1Id = Shader.PropertyToID("_rt1");
    int rt2Id = Shader.PropertyToID("_rt2");
    RenderTargetIdentifier srcId = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
    RenderTargetIdentifier destId = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);

    void OnEnable()
    {

        _BlurTex = Shader.PropertyToID("_BlurTex");
        _Direction = Shader.PropertyToID("_Direction");
        _MainTex = Shader.PropertyToID("_MainTex");
    }

    void RenderForOnRenderImage(PostProcessContext context)
    {
        var desc = new RenderTextureDescriptor(context.Source.width / 2, context.Source.height / 2);
        var rt1 = RenderTexture.GetTemporary(desc);//, FilterMode.Bilinear);
        var rt2 = RenderTexture.GetTemporary(desc);//, FilterMode.Bilinear);

        var h = new Vector2(1, 0);
        var v = new Vector2(0, 1);

        // Scale Down
        // cmd.Blit(srcId, context.Rt1Id);
        Graphics.Blit(context.Source, rt1);

        // 0: Gaussian Blur
        for (int i = 0; i < 3; i++)
        {
            // .SetGlobalVector(_Direction, h);
            context.UberMaterial.SetVector(_Direction, h);
            Graphics.Blit(rt1, rt2, context.UberMaterial, GaussianBlurShaderPass);
            // cmd.SetGlobalVector(_Direction, v);
            context.UberMaterial.SetVector(_Direction, v);
            Graphics.Blit(rt2, rt1, context.UberMaterial, GaussianBlurShaderPass);
        }

        // 1: DOF
        // cmd.SetGlobalTexture(_MainTex, context.SourceId);//srcId);
        // cmd.SetGlobalTexture(_BlurTex, rt1Id);

        context.UberMaterial.SetTexture(_MainTex, context.Source);
        context.UberMaterial.SetTexture(_BlurTex, rt1);
        Graphics.Blit(context.Source, context.Dest, context.UberMaterial, DofShaderPass);
        context.Swap();

        RenderTexture.ReleaseTemporary(rt2);
        RenderTexture.ReleaseTemporary(rt1);
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
        if (context.useOnRenderImage)
        {
            RenderForOnRenderImage(context);
            return;
        }

        var cmd = context.CommandBuffer;
        var desc = new RenderTextureDescriptor(Screen.width / 2, Screen.height / 2);
        cmd.GetTemporaryRT(rt1Id, desc, FilterMode.Bilinear);
        cmd.GetTemporaryRT(rt2Id, desc, FilterMode.Bilinear);

        var h = new Vector2(1, 0);
        var v = new Vector2(0, 1);

        // Scale Down
        // cmd.Blit(srcId, context.Rt1Id);
        cmd.Blit(context.SourceId, rt1Id);

        // 0: Gaussian Blur
        for (int i = 0; i < 1; i++)
        {
            cmd.SetGlobalVector(_Direction, h);
            cmd.Blit(rt1Id, rt2Id, context.UberMaterial, GaussianBlurShaderPass);
            cmd.SetGlobalVector(_Direction, v);
            cmd.Blit(rt2Id, rt1Id, context.UberMaterial, GaussianBlurShaderPass);
        }

        // 1: DOF
        cmd.SetGlobalTexture(_MainTex, context.SourceId);//srcId);
        cmd.SetGlobalTexture(_BlurTex, rt1Id);
        // context.UberMaterial.SetTexture(_BlurTex,rt1Id);
        cmd.Blit(context.SourceId, destId, context.UberMaterial, DofShaderPass);

        cmd.ReleaseTemporaryRT(rt2Id);
        cmd.ReleaseTemporaryRT(rt1Id);
    }
}


