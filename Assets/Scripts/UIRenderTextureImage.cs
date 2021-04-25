using UnityEngine;

[ExecuteAlways]
public class UIRenderTextureImage : MonoBehaviour
{
    public UnityEngine.UI.RawImage image;
    public PostProcess.PostProcessCamera postProcessing;

    public void PostRender()
    {
        image.texture = postProcessing.Context.destRT;
    }
}
