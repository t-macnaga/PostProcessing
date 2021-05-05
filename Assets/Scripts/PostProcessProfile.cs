using System.Collections.Generic;
using UnityEngine;

namespace PostProcess
{
    [CreateAssetMenu()]
    public class PostProcessProfile : ScriptableObject
    {
        public List<PostProcessEffect> components;
#if UNITY_EDITOR
        void OnValidate()
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }
#endif
        public void Add(PostProcessEffect component)
        {
            components.Add(component);
        }

        public void Remove(PostProcessEffect component)
        {
            components.Remove(component);
        }

        public void Render(PostProcessContext context)
        {
            foreach (var component in components)
            {
                component.Render(context);
            }
        }

        public void FrameCleanup()
        {
            foreach (var component in components)
            {
                component.FrameCleanup();
            }
        }

        public T GetEffect<T>() where T : PostProcessEffect => components.Find(x => x is T) as T;
    }
}