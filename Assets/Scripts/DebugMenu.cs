using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PostProcess;

public class DebugMenu : MonoBehaviour
{
    public Text fpsText;
    public GameObject container;
    public Slider bloomIntensitySlider;
    public Slider dofDepthSlider;
    // 変数
    int frameCount;
    float prevTime;
    float fps;
    PostProcessCamera PostProcessCamera => GameObject.FindObjectOfType<PostProcessCamera>();

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        // DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        frameCount++;
        float time = Time.realtimeSinceStartup - prevTime;

        if (time >= 0.5f)
        {
            fps = frameCount / time;
            // Debug.Log(fps);
            fpsText.text = fps.ToString();

            frameCount = 0;
            prevTime = Time.realtimeSinceStartup;
        }
    }

    public void ToggleDebugMenu(bool on)
    {
        container.SetActive(on);
        bloomIntensitySlider.value = PostProcessCamera.profile.GetEffect<Bloom>().Intensity;
        dofDepthSlider.value = PostProcessCamera.profile.GetEffect<DepthOfField>().Depth;
    }

    public void TogglePostProcess(bool on)
    {
        PostProcessCamera.enabled = on;
        if (!on)
        {
            PostProcessCamera.Context.Camera.enabled = true;
        }
    }

    public void ToggleAllowHDR(bool on)
    {
        foreach (var cam in GameObject.FindObjectsOfType<Camera>())
        {
            cam.allowHDR = on;
        }
    }

    public void ToggleAllowMSAA(bool on)
    {
        foreach (var cam in GameObject.FindObjectsOfType<Camera>())
        {
            cam.allowMSAA = on;
        }
    }

    public void LoadMain()
    {
        SceneManager.LoadScene("Main");
    }

    public void LoadStage()
    {
        SceneManager.LoadScene("Stage");
    }

    public void LoadStagePPSv2()
    {
        SceneManager.LoadScene("StagePPSv2");
    }

    public void OnValueChanged(float scale)
    {
        PostProcessCamera.renderScale = scale;
        PostProcessCamera.RebuildRT();
    }

    public void ToggleBloom(bool on)
    {
        PostProcessCamera.profile.GetEffect<Bloom>().IsEnabled = on;
    }

    public void OnValueChangeBloomIteration(float step)
    {
        var postProcess = GameObject.FindObjectOfType<PostProcessCamera>();
        // postProcess.profile.GetEffect<Bloom>()._iteration = (int)step;
        //TODO:
    }

    public void OnValueChangeBloomIntensity(float value)
    {
        PostProcessCamera.profile.GetEffect<Bloom>().Intensity = value;
    }

    public void OnValueChangeDofDepth(float value)
    {
        PostProcessCamera.profile.GetEffect<DepthOfField>().Depth = value;
    }

    public void ToggleDof(bool on)
    {
        PostProcessCamera.profile.GetEffect<DepthOfField>().IsEnabled = on;
    }

    public void ToggleGray(bool on)
    {
        PostProcessCamera.profile.GetEffect<GrayScale>().IsEnabled = on;
    }
}
