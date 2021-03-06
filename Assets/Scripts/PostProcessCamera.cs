﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Events;

namespace PostProcess
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class PostProcessCamera : MonoBehaviour
    {
        public PostProcessProfile profile;
        public float renderScale = 1f;
        public UnityEvent onPostRender = new UnityEvent();
        public int targetResolutionWidth = 856;
        public int targetResolutionHeight = 480;
        public bool isRenderManual;
        public bool isHalfRender;
        int counter;
        public PostProcessContext Context { get; private set; }
        CommandBuffer cmd;
        Camera camera;
        public int Width => (int)(targetResolutionWidth * renderScale);
        public int Height => (int)(targetResolutionHeight * renderScale);

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Context == null) return;
            RebuildRT();
        }
#endif
        void Update()
        {
            if (isRenderManual)
            {
                if (isHalfRender)
                {
                    counter++;
                    if (counter % 2 == 0) return;
                }
                camera.enabled = false;
                camera.Render();
            }
        }

        void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.update += Update;
#endif
            camera = GetComponent<Camera>();
            cmd = new CommandBuffer();
            Context = new PostProcessContext(cmd, profile, camera);
            camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, cmd);
            Context.GetTemporaryRT(Width, Height);
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.update -= Update;
#endif
            Context.Cleanup();
            camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, cmd);
            camera.targetTexture = null;
            Context.ReleaseTemporaryRT();
        }

        void OnPreRender()
        {
            camera.targetTexture = Context.sourceRT;
            cmd.Clear();
        }

        void OnPostRender()
        {
            Context.OnPostRender();
            onPostRender?.Invoke();
        }

        public void RebuildRT()
        {
            Context.RebuildTemporaryRT(Width, Height);
        }
    }
}