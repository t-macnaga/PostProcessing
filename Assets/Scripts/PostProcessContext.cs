using UnityEngine;
using UnityEngine.Rendering;

namespace PostProcess
{
    public class PostProcessContext
    {
        public CommandBuffer CommandBuffer { get; private set; }
        public Material UberMaterial { get; private set; }
        public RenderTexture Source { get; set; }
        public RenderTexture Dest { get; set; }

        public PostProcessContext(CommandBuffer commandBuffer, Material uberMaterial)
        {
            CommandBuffer = commandBuffer;
            UberMaterial = uberMaterial;
        }

        public void Swap()
        {
            var source = Source;
            Source = Dest;
            Dest = source;
        }
    }

    public class Constants
    {
        public static readonly int BloomExtractPass = 0;
        public static readonly int BloomBlurPass = 1;
        public static readonly int DofBlurPass = 5;
        public static readonly int BloomCombinePass = 8;
        public static readonly int BlitFinalPass = 9;
    }
}