using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class MyPostProcess : MonoBehaviour
{
    #region Bloom
    [SerializeField] bool bloom;
    [SerializeField, Range(1, 30)] int _iteration = 1;
    [SerializeField, Range(0.0f, 1.0f)] float _threshold = 0.0f;
    [SerializeField, Range(0.0f, 1.0f)] float _softThreshold = 0.0f;
    [SerializeField, Range(0.0f, 10.0f)] float _intensity = 1.0f;
    [SerializeField] bool _debug;
    // 4点をサンプリングして色を作るマテリアル
    [SerializeField] Material bloomMaterial;
    #endregion
    #region DOF
    public bool dof;
    public Material material;

    private int _Direction;
    private int _BlurTex;
    #endregion

    RenderTargetIdentifier[] ids = new RenderTargetIdentifier[30];
    RenderTextureDescriptor[] descs = new RenderTextureDescriptor[30];

    CommandBuffer cmd;
    RenderTextureDescriptor sourceDesc = new RenderTextureDescriptor();
    int sourceId = Shader.PropertyToID("Source");

    Camera camera;
    RenderTargetIdentifier srcId = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
    RenderTargetIdentifier destId = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
    int rt1Id = Shader.PropertyToID("_rt1");
    int rt2Id = Shader.PropertyToID("_rt2");

    void Awake()
    {
        camera = GetComponent<Camera>();
        camera.depthTextureMode |= DepthTextureMode.Depth;
        _Direction = Shader.PropertyToID("_Direction");
        _BlurTex = Shader.PropertyToID("_BlurTex");
    }

    void OnEnable()
    {
        cmd = new CommandBuffer();
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

        if (bloom)
        {
            SetupBloom();
        }

        if (dof)
        {
            SetupDOF();
        }
    }

    void SetupBloom()
    {
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
        bloomMaterial.SetVector("_FilterParams", filterParams);
        bloomMaterial.SetFloat("_Intensity", _intensity);
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
            cmd.Blit(currentSourceId, ids[i], bloomMaterial, pathIndex);
            currentSourceId = descId;
        }

        // アップサンプリング
        for (i -= 2; i >= 0; i--)
        {
            currentDesc = descs[i];

            // Blit時にマテリアルとパスを指定する
            cmd.Blit(currentSourceId, ids[i], bloomMaterial, 2);
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(currentSourceId.ToString()));
            currentSourceId = ids[i];
        }
        // 最後にdestにBlit
        pathIndex = _debug ? 4 : 3;
        cmd.Blit(currentSourceId, destId, bloomMaterial, pathIndex);
        cmd.ReleaseTemporaryRT(Shader.PropertyToID(currentSourceId.ToString()));
    }

    void SetupDOF()
    {
        var desc = new RenderTextureDescriptor(Screen.width / 2, Screen.height / 2);
        cmd.GetTemporaryRT(rt1Id, desc, FilterMode.Bilinear);
        cmd.GetTemporaryRT(rt2Id, desc, FilterMode.Bilinear);

        var h = new Vector2(1, 0);
        var v = new Vector2(0, 1);

        // Scale Down
        cmd.Blit(srcId, rt1Id);

        // 0: Gaussian Blur
        for (int i = 0; i < 3; i++)
        {
            material.SetVector(_Direction, h);
            cmd.Blit(rt1Id, rt2Id, material, 0);
            material.SetVector(_Direction, v);
            cmd.Blit(rt2Id, rt1Id, material, 0);
        }

        // 1: DOF
        cmd.SetGlobalTexture(_BlurTex, rt1Id);
        cmd.Blit(srcId, destId, material, 1);

        cmd.ReleaseTemporaryRT(rt2Id);
        cmd.ReleaseTemporaryRT(rt1Id);
    }
}
