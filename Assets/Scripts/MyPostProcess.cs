using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessContext
{
    public CommandBuffer CommandBuffer { get; private set; }
    public Material UberMaterial { get; private set; }

    public PostProcessContext(CommandBuffer commandBuffer, Material uberMaterial)
    {
        CommandBuffer = commandBuffer;
        UberMaterial = uberMaterial;
    }
}

public interface IPostProcessComponent
{
    void Render(PostProcessContext context);
}

[ExecuteAlways]
public class MyPostProcess : MonoBehaviour
{
    public Material uberMaterial;
    public PostProcessProfile profile;

    // #region DOF
    // public bool dof;
    // public Material material;

    // private int _Direction;
    // private int _BlurTex;
    // #endregion

    #region GaussianBlur
    public bool mobileModeGaussianBlur;
    public bool gaussianBlur;
    public Material gaussianBlurMaterial;
    #endregion

    // RenderTargetIdentifier[] ids = new RenderTargetIdentifier[30];
    // RenderTextureDescriptor[] descs = new RenderTextureDescriptor[30];
    PostProcessContext Context;
    CommandBuffer cmd;
    // RenderTextureDescriptor sourceDesc = new RenderTextureDescriptor();
    // int sourceId = Shader.PropertyToID("Source");

    Camera camera;
    // RenderTargetIdentifier srcId = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
    // RenderTargetIdentifier destId = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
    // int rt1Id = Shader.PropertyToID("_rt1");
    // int rt2Id = Shader.PropertyToID("_rt2");

    void Awake()
    {
        camera = GetComponent<Camera>();
        camera.depthTextureMode |= DepthTextureMode.Depth;
        // _Direction = Shader.PropertyToID("_Direction");
        // _BlurTex = Shader.PropertyToID("_BlurTex");
    }

    void OnEnable()
    {
        cmd = new CommandBuffer();
        Context = new PostProcessContext(cmd, uberMaterial);
        camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, cmd);
    }

    void OnDisable()
    {
        camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, cmd);
    }

    void OnPreRender()
    {
        BuildCommandBuffer();
    }

    void BuildCommandBuffer()
    {
        cmd.Clear();

        if (profile != null)
        {
            profile.Render(Context);
        }

        // if (gaussianBlur)
        // {
        //     SetupGaussianBlur();
        // }
    }

    // void SetupDOF()
    // {
    //     var desc = new RenderTextureDescriptor(Screen.width / 2, Screen.height / 2);
    //     cmd.GetTemporaryRT(rt1Id, desc, FilterMode.Bilinear);
    //     cmd.GetTemporaryRT(rt2Id, desc, FilterMode.Bilinear);

    //     var h = new Vector2(1, 0);
    //     var v = new Vector2(0, 1);

    //     // Scale Down
    //     cmd.Blit(srcId, rt1Id);

    //     // 0: Gaussian Blur
    //     for (int i = 0; i < 3; i++)
    //     {
    //         // material.SetVector(_Direction, h);
    //         cmd.SetGlobalVector(_Direction, h);
    //         cmd.Blit(rt1Id, rt2Id, material, 0);
    //         cmd.SetGlobalVector(_Direction, v);
    //         // material.SetVector(_Direction, v);
    //         cmd.Blit(rt2Id, rt1Id, material, 0);
    //     }

    //     // 1: DOF
    //     cmd.SetGlobalTexture(_BlurTex, rt1Id);
    //     cmd.Blit(srcId, destId, material, 1);

    //     cmd.ReleaseTemporaryRT(rt2Id);
    //     cmd.ReleaseTemporaryRT(rt1Id);
    // }

    // void SetupGaussianBlur()
    // {
    //     var desc = new RenderTextureDescriptor(Screen.width / 2, Screen.height / 2);
    //     cmd.GetTemporaryRT(rt1Id, desc);//, FilterMode.Bilinear);
    //     cmd.GetTemporaryRT(rt2Id, desc);//, FilterMode.Bilinear);

    //     var h = new Vector2(1, 0);
    //     var v = new Vector2(0, 1);

    //     // Scale Down
    //     cmd.Blit(srcId, rt1Id);

    //     // Gaussian Blur
    //     if (mobileModeGaussianBlur)
    //     {
    //         gaussianBlurMaterial.EnableKeyword("MOBILE");
    //         // cmd.SetGlobalVector(_Direction, h);
    //         cmd.Blit(rt1Id, rt2Id, gaussianBlurMaterial, 0);

    //         // Scale up
    //         cmd.Blit(rt2Id, destId);
    //     }
    //     else
    //     {
    //         gaussianBlurMaterial.DisableKeyword("MOBILE");
    //         for (int i = 0; i < 1; i++)
    //         {
    //             // gaussianBlurMaterial.SetVector(_Direction, h);
    //             cmd.SetGlobalVector(_Direction, v);
    //             cmd.Blit(rt1Id, rt2Id, gaussianBlurMaterial, 0);
    //             // gaussianBlurMaterial.SetVector(_Direction, v);
    //             cmd.SetGlobalVector(_Direction, h);
    //             cmd.Blit(rt2Id, rt1Id, gaussianBlurMaterial, 0);
    //         }
    //         // Scale up
    //         cmd.Blit(rt1Id, destId);
    //     }


    //     cmd.ReleaseTemporaryRT(rt2Id);
    //     cmd.ReleaseTemporaryRT(rt1Id);
    // }
}
