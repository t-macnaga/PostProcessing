using UnityEngine;

namespace PostProcess
{
    public abstract class PostProcessEffect : ScriptableObject, IPostProcessComponent
    {
        [SerializeField] protected bool isEnabled;
        public virtual bool IsEnabled { get => isEnabled; set => isEnabled = value; }
        public abstract void Render(PostProcessContext context);
        public virtual void FrameCleanup() { }
    }
}