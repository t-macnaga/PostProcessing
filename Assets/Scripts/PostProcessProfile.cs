using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PostProcessProfile : ScriptableObject
{
    public List<PostProcessEffect> components;

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
            if (component.IsEnabled)
            {
                component.Render(context);
            }
        }
    }
}