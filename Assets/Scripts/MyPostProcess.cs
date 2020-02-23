using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace PostProcess
{
    [ExecuteAlways]
    public class MyPostProcess : MonoBehaviour
    {
        public PostProcessProfile profile;
        public float renderScale = 1f;
        public UnityEngine.UI.RawImage image;
        public int targetResolutionWidth = 856;
        public int targetResolutionHeight = 480;

        #region GaussianBlur
        public bool mobileModeGaussianBlur;
        public bool gaussianBlur;
        public Material gaussianBlurMaterial;
        #endregion

        PostProcessContext Context;
        CommandBuffer cmd;
        RenderTexture temp;
        RenderTexture halfSource;
        RenderTexture dest;
        RenderTexture myRenderTexture;
        Material material;

        //TODO: for specific layer mask
        // Camera layerCamera;
        // RenderTexture layerCameraTarget;

        Camera camera;

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
            material = new Material(Shader.Find("Hidden/PostEffect/Uber"));
            material.hideFlags = HideFlags.HideAndDontSave;
            cmd = new CommandBuffer();
            Context = new PostProcessContext(cmd, material);
            camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, cmd);

            //TODO: make specific layer camera test.
            // MakeCamera();

            GetTemporaryRT();
        }

        void OnDisable()
        {
            camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, cmd);

            camera.targetTexture = null;
            ReleaseTemporaryRT();
            DestroyImmediate(material);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            RebuildTemporaryRT();
        }
#endif
        public void RebuildTemporaryRT()
        {
            ReleaseTemporaryRT();
            GetTemporaryRT();
        }

        void OnPostRender()
        {
            camera.targetTexture = myRenderTexture;
            cmd.Clear();

            var source = myRenderTexture;
            cmd.Blit(source, halfSource);
            Context.Source = source;
            Context.Dest = temp;
            profile.Render(Context);

            // Blit Final Blur Tex
            cmd.Blit(halfSource, Context.Dest, Context.UberMaterial, 8);
            // Context.Swap();

            // cmd.SetGlobalTexture("_MainTex",);
            Context.UberMaterial.SetTexture("_MainTex", source);
            Context.UberMaterial.SetTexture("_FinalBlurTex", Context.Dest);

            cmd.Blit(source, dest, Context.UberMaterial, 9);

            image.texture = dest;

            profile.GetEffect<Bloom>().Release();
            profile.GetEffect<DepthOfField>().Release();
        }

        void GetTemporaryRT()
        {
            var width = targetResolutionWidth * renderScale;
            var height = targetResolutionHeight * renderScale;
            myRenderTexture = RenderTexture.GetTemporary((int)width, (int)height, 16);
            dest = RenderTexture.GetTemporary((int)width, (int)height, 16);
            temp = RenderTexture.GetTemporary((int)width / 2, (int)height / 2, 16);
            halfSource = RenderTexture.GetTemporary((int)width / 2, (int)height / 2, 16);
        }

        void ReleaseTemporaryRT()
        {
            RenderTexture.ReleaseTemporary(halfSource);
            RenderTexture.ReleaseTemporary(temp);
            RenderTexture.ReleaseTemporary(dest);
            //TODO:
            // RenderTexture.ReleaseTemporary(layerCameraTarget);
            // DestroyImmediate(layerCamera.gameObject);
            // camera.targetTexture = null;
            // RenderTexture.ReleaseTemporary(renderTexture);
            RenderTexture.ReleaseTemporary(myRenderTexture);
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
}