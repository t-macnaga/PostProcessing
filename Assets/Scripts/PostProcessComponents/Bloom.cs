using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu()]
public class Bloom : ScriptableObject, IPostProcessComponent
{
    // [SerializeField] bool bloom;
    [SerializeField, Range(1, 30)] int _iteration = 1;
    [SerializeField, Range(0.0f, 1.0f)] float _threshold = 0.0f;
    [SerializeField, Range(0.0f, 1.0f)] float _softThreshold = 0.0f;
    [SerializeField, Range(0.0f, 10.0f)] float _intensity = 1.0f;
    [SerializeField] bool _debug;
    // 4点をサンプリングして色を作るマテリアル

    RenderTargetIdentifier[] ids = new RenderTargetIdentifier[30];
    RenderTextureDescriptor[] descs = new RenderTextureDescriptor[30];

    // CommandBuffer cmd;
    RenderTextureDescriptor sourceDesc = new RenderTextureDescriptor();
    int sourceId = Shader.PropertyToID("Source");

    // RenderTargetIdentifier srcId = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
    RenderTargetIdentifier destId = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
    // Camera camera;
    // RenderTargetIdentifier srcId = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
    // RenderTargetIdentifier destId = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
    // int rt1Id = Shader.PropertyToID("_rt1");
    // int rt2Id = Shader.PropertyToID("_rt2");

    // void Awake()
    // {
    //     camera = GetComponent<Camera>();
    //     camera.depthTextureMode |= DepthTextureMode.Depth;
    //     _Direction = Shader.PropertyToID("_Direction");
    //     _BlurTex = Shader.PropertyToID("_BlurTex");
    // }

    public void Render(PostProcessContext context)
    {
        var cmd = context.CommandBuffer;
        sourceDesc.width = Screen.width;
        sourceDesc.height = Screen.height;
        sourceDesc.msaaSamples = 1;
        sourceDesc.colorFormat = RenderTextureFormat.Default;
        sourceDesc.useMipMap = false;
        sourceDesc.dimension = TextureDimension.Tex2D;
        sourceDesc.enableRandomWrite = false;
        sourceDesc.depthBufferBits = 0;
        sourceDesc.volumeDepth = 16;
        sourceDesc.autoGenerateMips = false;
        sourceDesc.msaaSamples = 1;
        sourceDesc.sRGB = false;

        cmd.GetTemporaryRT(sourceId, sourceDesc, FilterMode.Bilinear);
        var currentSourceId = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);

        var filterParams = Vector4.zero;
        var knee = _threshold * _softThreshold;
        filterParams.x = _threshold;
        filterParams.y = _threshold - knee;
        filterParams.z = knee * 2.0f;
        filterParams.w = 0.25f / (knee + 0.00001f);
        context.UberMaterial.SetVector("_FilterParams", filterParams);
        context.UberMaterial.SetFloat("_Intensity", _intensity);
        cmd.SetGlobalTexture("_SourceTex", new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));//currentSourceId);

        var width = sourceDesc.width;
        var height = sourceDesc.height;

        var pathIndex = 0;
        var i = 0;
        RenderTextureDescriptor currentDesc;
        // ダウンサンプリング
        for (; i < _iteration; i++)
        {
            width /= 2;
            height /= 2;
            if (width < 2 || height < 2)
            {
                break;
            }
            currentDesc = descs[i] = new RenderTextureDescriptor(width, height);

            // 最初の一回は明度抽出用のパスを使ってダウンサンプリングする
            pathIndex = i == 0 ? 0 : 1;
            var descId = $"_{width}_{height}";
            ids[i] = descId;

            cmd.GetTemporaryRT(Shader.PropertyToID(descId), currentDesc, FilterMode.Bilinear);
            cmd.Blit(currentSourceId, ids[i], context.UberMaterial, pathIndex);
            currentSourceId = descId;
        }

        // アップサンプリング
        for (i -= 2; i >= 0; i--)
        {
            currentDesc = descs[i];

            // Blit時にマテリアルとパスを指定する
            cmd.Blit(currentSourceId, ids[i], context.UberMaterial, 2);
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(currentSourceId.ToString()));
            currentSourceId = ids[i];
        }
        // 最後にdestにBlit
        pathIndex = _debug ? 4 : 3;
        cmd.Blit(currentSourceId, destId, context.UberMaterial, pathIndex);
        cmd.ReleaseTemporaryRT(Shader.PropertyToID(currentSourceId.ToString()));
    }
}