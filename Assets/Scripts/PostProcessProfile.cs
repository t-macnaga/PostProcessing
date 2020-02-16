using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu()]
public class PostProcessProfile : ScriptableObject
{
    public List<PostProcessEffect> components;

    public bool EnabledAny()
    {
        for (var i = 0; i < components.Count; i++)
        {
            if (components[i].IsEnabled)
            {
                return true;
            }
        }
        return false;
    }

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

        // Graphics.Blit(context.Source, context.Dest, context.UberMaterial, 8);
        // // //TODO: 8 is Uber Pass.
        // var tempId = Shader.PropertyToID("_TEMP");
        // context.CommandBuffer.GetTemporaryRT(tempId, Screen.width, Screen.height);

        // context.CommandBuffer.Blit(context.SourceId, tempId);
        // context.CommandBuffer.Blit(tempId, context.DestinationId, context.UberMaterial, 8);
    }

    public T GetEffect<T>() where T : PostProcessEffect => components.Find(x => x is T) as T;
}