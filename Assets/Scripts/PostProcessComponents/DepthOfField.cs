using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu()]
public class DepthOfField : ScriptableObject, IPostProcessComponent
{
    [SerializeField] float depth;
    [SerializeField] float radious;
    static readonly int GaussianBlurShaderPass = 5;
    static readonly int DofShaderPass = 6;

    int _MainTex;
    int _Direction;// = Shader.PropertyToID("_Direction");
    int _BlurTex;// = Shader.PropertyToID("_BlurTex");
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
        // int _Depth = Shader.PropertyToID("_Depth");
        // int _Radius = Shader.PropertyToID("_Radius");
    }

    public void Render(PostProcessContext context)
    {
        context.UberMaterial.SetFloat(_Depth, depth);
        context.UberMaterial.SetFloat(_Radius, radious);

        var cmd = context.CommandBuffer;
        var desc = new RenderTextureDescriptor(Screen.width / 2, Screen.height / 2);
        cmd.GetTemporaryRT(rt1Id, desc, FilterMode.Bilinear);
        cmd.GetTemporaryRT(rt2Id, desc, FilterMode.Bilinear);

        var h = new Vector2(1, 0);
        var v = new Vector2(0, 1);

        // Scale Down
        cmd.Blit(srcId, rt1Id);

        // 0: Gaussian Blur
        for (int i = 0; i < 3; i++)
        {
            cmd.SetGlobalVector(_Direction, h);
            cmd.Blit(rt1Id, rt2Id, context.UberMaterial, GaussianBlurShaderPass);
            cmd.SetGlobalVector(_Direction, v);
            cmd.Blit(rt2Id, rt1Id, context.UberMaterial, GaussianBlurShaderPass);
        }

        // 1: DOF
        cmd.SetGlobalTexture(_MainTex, srcId);
        cmd.SetGlobalTexture(_BlurTex, rt1Id);
        // context.UberMaterial.SetTexture(_BlurTex,rt1Id);
        cmd.Blit(srcId, destId, context.UberMaterial, DofShaderPass);

        cmd.ReleaseTemporaryRT(rt2Id);
        cmd.ReleaseTemporaryRT(rt1Id);
    }
}


