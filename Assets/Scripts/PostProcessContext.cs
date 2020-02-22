using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessContext
{
    public CommandBuffer CommandBuffer { get; private set; }
    public Material UberMaterial { get; private set; }
    public RenderTargetIdentifier SourceId { get; private set; } = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
    public RenderTargetIdentifier DestinationId { get; private set; } = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
    public int MainTexId { get; private set; } = Shader.PropertyToID("_MainTex");
    public RenderTextureDescriptor SourceDescriptor { get; set; }

    // OnRenderImage test.
    public RenderTexture Source { get; set; }
    public RenderTexture Dest { get; set; }
    public bool useOnRenderImage;
    public RenderTexture QuaterTex;
    //
    public PostProcessContext(RenderTexture source, RenderTexture dest, Material uberMaterial)
    {
        Source = source;
        Dest = dest;
        UberMaterial = uberMaterial;
    }

    public PostProcessContext(CommandBuffer commandBuffer, Material uberMaterial)
    {
        CommandBuffer = commandBuffer;
        UberMaterial = uberMaterial;
        // SourceDescriptor = sourceDesc;
    }

    public void Swap()
    {
        if (useOnRenderImage)
        {
            var source = Source;
            Source = Dest;
            Dest = source;
        }
        else
        {
            var source = SourceId;
            SourceId = DestinationId;
            DestinationId = SourceId;
        }
    }
}
