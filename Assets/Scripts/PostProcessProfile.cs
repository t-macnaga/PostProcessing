using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu()]
public class PostProcessProfile : ScriptableObject
{
    public Bloom bloom;
    public DepthOfField dof;

    public void Render(PostProcessContext context)
    {
        if (bloom != null)
        {
            bloom.Render(context);
        }

        if (dof != null)
        {
            dof.Render(context);
        }
    }
}