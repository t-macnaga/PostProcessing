using UnityEngine;
using UnityEngine.Rendering;

namespace PostProcess
{
    [ExecuteAlways]
    public class PostProcessCamera : MonoBehaviour
    {
        public PostProcessProfile profile;
        public float renderScale = 1f;
        public UnityEngine.UI.RawImage image;
        public int targetResolutionWidth = 856;
        public int targetResolutionHeight = 480;
        PostProcessContext Context;
        CommandBuffer cmd;
        RenderTexture sourceRT;
        RenderTexture tempRT;
        RenderTexture halfSourceRT;
        RenderTexture destRT;
        Material material;
        Camera camera;

        void Awake()
        {
            camera = GetComponent<Camera>();
            camera.depthTextureMode |= DepthTextureMode.Depth;
        }

        void OnEnable()
        {
            material = new Material(Shader.Find("Hidden/PostEffect/Uber"));
            material.hideFlags = HideFlags.HideAndDontSave;
            cmd = new CommandBuffer();
            Context = new PostProcessContext(cmd, material);
            camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, cmd);
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

        void OnPreRender()
        {
            camera.targetTexture = sourceRT;
            cmd.Clear();
        }

        void OnPostRender()
        {
            // Blit to half resolution render texture
            cmd.Blit(sourceRT, halfSourceRT);
            Context.Source = sourceRT;
            Context.Dest = tempRT;
            profile.Render(Context);

            // Blit Final Blur Tex
            cmd.Blit(halfSourceRT, Context.Dest, Context.UberMaterial, Constants.BloomCombinePass);

            Context.UberMaterial.SetTexture("_MainTex", sourceRT);
            Context.UberMaterial.SetTexture("_FinalBlurTex", Context.Dest);

            // Blit final pass
            cmd.Blit(sourceRT, destRT, Context.UberMaterial, Constants.BlitFinalPass);

            image.texture = destRT;

            profile.Cleanup();
        }

        public void RebuildTemporaryRT()
        {
            ReleaseTemporaryRT();
            GetTemporaryRT();
        }

        void GetTemporaryRT()
        {
            var width = targetResolutionWidth * renderScale;
            var height = targetResolutionHeight * renderScale;
            sourceRT = RenderTexture.GetTemporary((int)width, (int)height, 16);
            destRT = RenderTexture.GetTemporary((int)width, (int)height, 16);
            tempRT = RenderTexture.GetTemporary((int)width / 2, (int)height / 2, 16);
            halfSourceRT = RenderTexture.GetTemporary((int)width / 2, (int)height / 2, 16);
        }

        void ReleaseTemporaryRT()
        {
            RenderTexture.ReleaseTemporary(halfSourceRT);
            RenderTexture.ReleaseTemporary(tempRT);
            RenderTexture.ReleaseTemporary(destRT);
            RenderTexture.ReleaseTemporary(sourceRT);
        }
    }
}