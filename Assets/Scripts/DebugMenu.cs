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
    // 変数
    int frameCount;
    float prevTime;
    float fps;

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(this.gameObject);
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
    }

    public void TogglePostProcess(bool on)
    {
        GameObject.FindObjectOfType<MyPostProcess>().enabled = on;
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

    public void OnValueChanged(float scale)
    {
        var postProcess = GameObject.FindObjectOfType<MyPostProcess>();
        postProcess.renderScale = scale;
        postProcess.RebuildTemporaryRT();
    }

    public void ToggleBloom(bool on)
    {
        var postProcess = GameObject.FindObjectOfType<MyPostProcess>();
        postProcess.profile.GetEffect<Bloom>().IsEnabled = on;
    }

    public void OnValueChangeBloomIteration(float step)
    {
        var postProcess = GameObject.FindObjectOfType<MyPostProcess>();
        postProcess.profile.GetEffect<Bloom>()._iteration = (int)step;
    }

    public void ToggleDof(bool on)
    {
        var postProcess = GameObject.FindObjectOfType<MyPostProcess>();
        postProcess.profile.GetEffect<DepthOfField>().IsEnabled = on;
    }

    public void ToggleGray(bool on)
    {
        var postProcess = GameObject.FindObjectOfType<MyPostProcess>();
        postProcess.profile.GetEffect<GrayScale>().IsEnabled = on;
    }


    // 表示処理
    // private void OnGUI()
    // {
    //     GUILayout.Label(fps.ToString());
    // }
}
