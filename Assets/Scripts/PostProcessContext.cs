using UnityEngine;
using UnityEngine.Rendering;

namespace PostProcess
{
    public class PostProcessContext
    {
        public RenderTexture sourceRT;
        public RenderTexture destRT;
        RenderTexture tempRT;
        RenderTexture halfSourceRT;
        public Camera Camera { get; set; }
        public PostProcessProfile Profile { get; set; }
        public CommandBuffer CommandBuffer { get; private set; }
        public Material UberMaterial { get; private set; }
        public RenderTexture Source { get; set; }
        public RenderTexture Dest { get; set; }

        public PostProcessContext(CommandBuffer commandBuffer)
        {
            CommandBuffer = commandBuffer;
            UberMaterial = new Material(Shader.Find("Hidden/PostEffect/Uber"));
            UberMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        public void Cleanup()
        {
            Object.DestroyImmediate(UberMaterial);
        }

        public void OnPostRender()
        {
            // Blit to half resolution render texture
            CommandBuffer.Blit(sourceRT, halfSourceRT);

            Source = halfSourceRT;
            Dest = tempRT;
            Profile.Render(this);

            UberMaterial.SetTexture("_MainTex", sourceRT);
            UberMaterial.SetTexture("_FinalBlurTex", Dest);

            // Blit final pass
            CommandBuffer.Blit(sourceRT, destRT, UberMaterial, Constants.BlitFinalPass);

            Profile.FrameCleanup();
        }

        public void RebuildTemporaryRT(int width, int height)
        {
            ReleaseTemporaryRT();
            GetTemporaryRT(width, height);
        }

        public void GetTemporaryRT(int width, int height)
        {
            sourceRT = RenderTexture.GetTemporary(width, height, 16);
            destRT = RenderTexture.GetTemporary(width, height, 16);
            tempRT = RenderTexture.GetTemporary(width / 2, height / 2, 16);
            var ratio = (width > height) ? (float)width / height : (float)height / width;
            // halfSourceRT = RenderTexture.GetTemporary(width / 2, height / 2, 16);
            halfSourceRT = RenderTexture.GetTemporary(512, (int)(512 / ratio), 16);
        }

        public void ReleaseTemporaryRT()
        {
            RenderTexture.ReleaseTemporary(halfSourceRT);
            RenderTexture.ReleaseTemporary(tempRT);
            RenderTexture.ReleaseTemporary(destRT);
            RenderTexture.ReleaseTemporary(sourceRT);
        }
    }
}