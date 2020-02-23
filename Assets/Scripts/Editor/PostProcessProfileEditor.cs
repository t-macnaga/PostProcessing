using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace PostProcess
{
    [CustomEditor(typeof(PostProcessProfile))]
    public class PostProcessProfileEditor : Editor
    {
        static PostProcessProfileEditor profileEditorInstance;
        PostProcessProfile profile;
        List<Editor> postProcessEditors;
        string[] avairableEffectNames;
        int selectedIndex;

        [MenuItem("CONTEXT/PostProcessEffect/Remove")]
        static void RemoveEffect(MenuCommand command)
        {
            profileEditorInstance.RemoveEffect(command.context.name);
        }

        void OnEnable()
        {
            profileEditorInstance = this;
            profile = target as PostProcessProfile;
            RefreshEditors();
            avairableEffectNames = TypeCache.GetTypesDerivedFrom<PostProcessEffect>()
            .Select(x => x.Name).ToArray();
        }

        void OnDisable()
        {
            profileEditorInstance = null;
        }

        public override void OnInspectorGUI()
        {
            foreach (var editor in postProcessEditors)
            {
                editor.DrawHeader();
                EditorGUILayout.BeginVertical("box");
                editor.OnInspectorGUI();
                EditorGUILayout.EndVertical();
            }

            selectedIndex = EditorGUILayout.Popup(selectedIndex, avairableEffectNames);

            if (GUILayout.Button("Add Effect"))
            {
                AddEffect(avairableEffectNames[selectedIndex]);
            }
        }

        void AddEffect(string name)
        {
            var effect = ScriptableObject.CreateInstance(name) as PostProcessEffect;
            effect.name = name;
            AssetDatabase.AddObjectToAsset(effect, profile);
            profile.Add(effect);
            Reimport();
            RefreshEditors();
        }

        void RemoveEffect(string name)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(profile));
            for (var i = 0; i < assets.Length; i++)
            {
                if (AssetDatabase.IsSubAsset(assets[i]) && assets[i].name == name)
                {
                    profile.Remove(assets[i] as PostProcessEffect);
                    DestroyImmediate(assets[i], true);
                    break;
                }
            }
            Reimport();
            RefreshEditors();
        }

        void Reimport()
        {
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(profile));
        }

        void RefreshEditors()
        {
            postProcessEditors = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(profile))
            .Where(x => AssetDatabase.IsSubAsset(x))
            .Select(x => Editor.CreateEditor(x))
            .ToList();
        }
    }
}