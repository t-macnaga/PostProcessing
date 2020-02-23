using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu()]
public class Bloom : PostProcessEffect, IPostProcessComponent
{
    [SerializeField, Range(1, 30)] public int _iteration = 1;
    [SerializeField, Range(0.0f, 1.0f)] float _threshold = 0.0f;
    [SerializeField, Range(0.0f, 1.0f)] float _softThreshold = 0.0f;
    [SerializeField, Range(0.0f, 10.0f)] float _intensity = 1.0f;
    [SerializeField] bool _debug;
    // 4点をサンプリングして色を作るマテリアル

    RenderTargetIdentifier[] ids = new RenderTargetIdentifier[30];
    RenderTexture[] textures = new RenderTexture[30];
    RenderTextureDescriptor[] descs = new RenderTextureDescriptor[30];

    RenderTextureDescriptor sourceDesc = new RenderTextureDescriptor();
    int sourceId = Shader.PropertyToID("Source");
    int halfId = Shader.PropertyToID("_HalfTex");

    void RenderForOnRenderImage(PostProcessContext context)
    {
        var sourceDesc = new RenderTextureDescriptor(context.Source.width / 4, context.Source.height / 4);
        var currentSource = context.Source;
        var filterParams = Vector4.zero;
        var knee = _threshold * _softThreshold;
        filterParams.x = _threshold;
        filterParams.y = _threshold - knee;
        filterParams.z = knee * 2.0f;
        filterParams.w = 0.25f / (knee + 0.00001f);
        context.UberMaterial.SetVector("_FilterParams", filterParams);
        context.UberMaterial.SetFloat("_Intensity", _intensity);
        context.UberMaterial.SetTexture("_SourceTex", context.Source);

        var width = sourceDesc.width;
        var height = sourceDesc.height;

        var pathIndex = 0;
        var i = 0;
        RenderTextureDescriptor currentDesc;

        // ダウンサンプリング
        for (; i < _iteration; i++)
        {
            // width /= 2;
            // height /= 2;
            if (width < 2 || height < 2)
            {
                break;
            }
            currentDesc = descs[i] = new RenderTextureDescriptor(width, height);

            // 最初の一回は明度抽出用のパスを使ってダウンサンプリングする
            pathIndex = i == 0 ? 0 : 1;
            // var descId = $"_{width}_{height}";
            // ids[i] = descId;

            textures[i] = RenderTexture.GetTemporary(currentDesc);//, FilterMode.Bilinear);

            // context.UberMaterial.SetTexture("_MainTex", currentSource);
            context.CommandBuffer.SetGlobalTexture("_MainTex", currentSource);
            // Graphics.Blit(currentSource, textures[i], context.UberMaterial, pathIndex);
            context.CommandBuffer.Blit(currentSource, textures[i], context.UberMaterial, pathIndex);
            currentSource = textures[i];
            width /= 2;
            height /= 2;
        }

        context.UberMaterial.SetTexture("_BloomTex1", textures[0]);
        context.UberMaterial.SetTexture("_BloomTex2", textures[1]);
        context.UberMaterial.SetTexture("_BloomTex3", textures[2]);
        context.UberMaterial.SetTexture("_BloomTex4", textures[3]);
        context.UberMaterial.SetTexture("_BloomTex5", textures[4]);
        context.UberMaterial.SetTexture("_BloomTex6", textures[5]);

        // アップサンプリング
        // for (i -= 2; i >= 0; i--)
        // {
        //     currentDesc = descs[i];
        //     // cmd.SetGlobalTexture("_MainTex", currentSourceId);
        //     context.UberMaterial.SetTexture("_MainTex", currentSource);
        //     // Blit時にマテリアルとパスを指定する
        //     Graphics.Blit(currentSource, textures[i], context.UberMaterial, 2);
        //     RenderTexture.ReleaseTemporary(currentSource);
        //     currentSource = textures[i];
        // }
        // 最後にdestにBlit
        // pathIndex = _debug ? 4 : 3;
        // // Graphics.Blit(currentSource, context.Dest, context.UberMaterial, pathIndex);
        // Graphics.Blit(context.Source, context.Dest, context.UberMaterial, pathIndex);

        // for (var idx = 0; idx < textures.Length; idx++)
        // {
        //     RenderTexture.ReleaseTemporary(textures[idx]);
        // }

        // context.Swap();
        // RenderTexture.ReleaseTemporary(currentSource);
    }

    public void Release()
    {
        for (var idx = 0; idx < textures.Length; idx++)
        {
            RenderTexture.ReleaseTemporary(textures[idx]);
        }
    }

    public override void Render(PostProcessContext context)
    {
        if (!IsEnabled)
        {
            context.UberMaterial.DisableKeyword("BLOOM");
            return;
        }
        context.UberMaterial.EnableKeyword("BLOOM");

        if (context.useOnRenderImage)
        {
            RenderForOnRenderImage(context);
            return;
        }


        var cmd = context.CommandBuffer;

        // cmd.SetGlobalTexture("_MainTex", context.SourceId);
        // var tempId = Shader.PropertyToID("_TEMP2");
        // context.CommandBuffer.GetTemporaryRT(tempId, Screen.width / 2, Screen.height / 2, 0, FilterMode.Bilinear, RenderTextureFormat.DefaultHDR);
        // context.CommandBuffer.Blit(context.SourceId, context.DestinationId, context.UberMaterial, 8);
        //TODO: 実機だと、このSourceIdが映ってなくて真っ黒になっているっぽい。
        // if (blitCopy == null)
        // {
        //     blitCopy = new Material(Shader.Find("Hidden/BlitCopy"));
        // }
        // context.CommandBuffer.Blit(context.SourceId, tempId, blitCopy, 0);
        // context.CommandBuffer.Blit(tempId, context.DestinationId, context.UberMaterial, 0);

        // int texID = Shader.PropertyToID("_TEMP");
        // cmd.GetTemporaryRT(texID, Screen.width, Screen.height);// camera.pixelWidth, camera.pixelHeight);
        // cmd.Blit(context.SourceId, texID);
        // cmd.Blit(texID, BuiltinRenderTextureType.CameraTarget, new Material(Shader.Find("Hidden/BlitCopy")));
        // var tempId = Shader.PropertyToID("_TEMP");
        // var tempId = new RenderTargetIdentifier("_TEMP");
        // cmd.SetRenderTarget(new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));// context.SourceId);
        // cmd.SetRenderTarget(new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));// context.SourceId);
        // cmd.Blit(new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget), context.SourceId);//,context.DestinationId);
        // cmd.SetGlobalTexture("_MainTex",);
        // cmd.SetGlobalTexture("_MainTex", new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));// currentSourceId);
        // cmd.GetTemporaryRT(Shader.PropertyToID("_TEMP"), Screen.width, Screen.height);
        // cmd.Blit(context.SourceId, tempId);
        var sourceDesc = new RenderTextureDescriptor(Screen.width / 2, Screen.height / 2);
        var currentSourceId = context.SourceId;// new RenderTargetIdentifier(tempId);//"_TEMP");// context.SourceId;
        // var currentSourceId = context.SourceId;

        var filterParams = Vector4.zero;
        var knee = _threshold * _softThreshold;
        filterParams.x = _threshold;
        filterParams.y = _threshold - knee;
        filterParams.z = knee * 2.0f;
        filterParams.w = 0.25f / (knee + 0.00001f);
        context.UberMaterial.SetVector("_FilterParams", filterParams);
        context.UberMaterial.SetFloat("_Intensity", _intensity);
        cmd.SetGlobalTexture("_SourceTex", context.SourceId);

        var width = sourceDesc.width;
        var height = sourceDesc.height;

        var pathIndex = 0;
        var i = 0;
        RenderTextureDescriptor currentDesc;
        // ダウンサンプリング
        for (; i < _iteration; i++)
        {
            // width /= 2;
            // height /= 2;
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

            cmd.SetGlobalTexture("_MainTex", currentSourceId);
            cmd.Blit(currentSourceId, ids[i], context.UberMaterial, pathIndex);
            // BlitFullscreenTriangle(cmd,currentSourceId,ids[i]);
            currentSourceId = descId;
            width /= 2;
            height /= 2;
        }

        // アップサンプリング
        for (i -= 2; i >= 0; i--)
        {
            currentDesc = descs[i];

            cmd.SetGlobalTexture("_MainTex", currentSourceId);
            // Blit時にマテリアルとパスを指定する
            cmd.Blit(currentSourceId, ids[i], context.UberMaterial, 2);
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(currentSourceId.ToString()));
            currentSourceId = ids[i];
        }
        // 最後にdestにBlit
        pathIndex = _debug ? 4 : 3;
        cmd.Blit(currentSourceId, context.DestinationId, context.UberMaterial, pathIndex);
        cmd.ReleaseTemporaryRT(Shader.PropertyToID(currentSourceId.ToString()));
    }
}