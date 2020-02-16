using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class MyPostProcess : MonoBehaviour
{
    public Material uberMaterial;
    public PostProcessProfile profile;
    public bool useOnRenderImage;
    public float renderScale = 1f;
    public UnityEngine.UI.RawImage image;

    #region GaussianBlur
    public bool mobileModeGaussianBlur;
    public bool gaussianBlur;
    public Material gaussianBlurMaterial;
    #endregion

    // RenderTargetIdentifier[] ids = new RenderTargetIdentifier[30];
    // RenderTextureDescriptor[] descs = new RenderTextureDescriptor[30];
    PostProcessContext Context;
    CommandBuffer cmd;
    RenderTexture renderTexture;

    //TODO: for specific layer mask
    // Camera layerCamera;
    // RenderTexture layerCameraTarget;

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
    }

    void OnEnable()
    {
        // renderTexture = RenderTexture.GetTemporary(
        //                 new RenderTextureDescriptor((int)(Screen.width * renderScale), (int)(Screen.height * renderScale)));
        // camera.targetTexture = renderTexture;
        // image.texture = renderTexture;

        cmd = new CommandBuffer();
        Context = new PostProcessContext(cmd, uberMaterial);
        camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, cmd);

        //TODO: make specific layer camera test.
        // MakeCamera();
    }

    void OnDisable()
    {
        camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, cmd);

        //TODO:
        // RenderTexture.ReleaseTemporary(layerCameraTarget);
        // DestroyImmediate(layerCamera.gameObject);
        // camera.targetTexture = null;
        // RenderTexture.ReleaseTemporary(renderTexture);
    }

    void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        if (useOnRenderImage)
        {
            if (profile != null)
            {
                Context.useOnRenderImage = useOnRenderImage;
                if (profile.EnabledAny())
                {
                    var temp = RenderTexture.GetTemporary(
                        new RenderTextureDescriptor((int)(Screen.width * renderScale), (int)(Screen.height * renderScale)));
                    Graphics.Blit(source, temp);
                    Context.Source = source;
                    Context.Dest = temp;
                    profile.Render(Context);

                    //TODO: experiment
                    // Context.UberMaterial.SetTexture("_MainTex",);
                    // temp => temp2
                    // Source + temp2 => dest

                    Graphics.Blit(Context.Source, dest, Context.UberMaterial, 8);
                    // Graphics.Blit(temp, dest, Context.UberMaterial, 8);
                    // Graphics.Blit(source, dest);

                    RenderTexture.ReleaseTemporary(temp);

                    //TODO: experiment
                    // RenderTexture.ReleaseTemporary(Context.QuaterTex);
                }
                else
                {
                    Graphics.Blit(source, dest);
                }
            }
        }
        else
        {
            Graphics.Blit(source, dest);
        }
    }

    void OnPreRender()
    {
        BuildCommandBuffer();
    }

    void BuildCommandBuffer()
    {
        cmd.Clear();
        if (useOnRenderImage) { return; }

        if (profile != null)
        {
            profile.Render(Context);
        }

        // if (gaussianBlur)
        // {
        //     SetupGaussianBlur();
        // }
    }

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

    // void MakeCamera()
    // {
    //     var go = new GameObject("_Camera");
    //     layerCamera = go.AddComponent<Camera>();
    //     layerCamera.CopyFrom(camera);
    //     go.hideFlags = HideFlags.DontSave;
    //     layerCamera.cullingMask = 0;
    //     layerCamera.cullingMask |= (1 << LayerMask.NameToLayer("postprocess"));
    //     layerCamera.clearFlags = CameraClearFlags.Depth;
    //     layerCameraTarget = RenderTexture.GetTemporary(camera.pixelWidth / 2, camera.pixelHeight / 2);
    //     layerCamera.targetTexture = layerCameraTarget;
    //     go.AddComponent<LayerCamera>().Setup(this);
    // }

    // class LayerCamera : MonoBehaviour
    // {
    //     MyPostProcess postProcess;

    //     public void Setup(MyPostProcess postProcess)
    //     {
    //         this.postProcess = postProcess;
    //     }

    //     void OnRenderImage(RenderTexture source, RenderTexture dest)
    //     {

    //         if (postProcess.useOnRenderImage)
    //         {
    //             if (postProcess.profile != null)
    //             {
    //                 postProcess.Context.useOnRenderImage = postProcess.useOnRenderImage;
    //                 if (postProcess.profile.EnabledAny())
    //                 {
    //                     var temp = RenderTexture.GetTemporary(new RenderTextureDescriptor(Screen.width, Screen.height));
    //                     Graphics.Blit(source, temp);
    //                     postProcess.Context.Source = source;
    //                     postProcess.Context.Dest = temp;
    //                     postProcess.profile.Render(postProcess.Context);
    //                     Graphics.Blit(postProcess.Context.Source, dest, postProcess.Context.UberMaterial, 8);
    //                     RenderTexture.ReleaseTemporary(temp);
    //                 }
    //                 else
    //                 {
    //                     Graphics.Blit(source, dest);
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             //TODO: 白く表示されちゃう。
    //             Graphics.Blit(source, dest);
    //             // BuildCommandBuffer();
    //         }
    //     }
    // }
}
