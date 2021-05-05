using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace PostProcess
{
    public class EffectEditor
    {
        bool foldout = true;
        PostProcessProfileEditor profileEditor;
        Editor editor;
        public string Name => editor.target.name;
        public EffectEditor(Editor editor, PostProcessProfileEditor profileEditor)
        {
            this.editor = editor;
            this.profileEditor = profileEditor;
        }

        public void GUI()
        {
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, Name);
            var rect = GUILayoutUtility.GetLastRect();
            if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.ContextClick)
            {
                // Stuff I wanna do
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Remove", "Removes the Effect from Profile."), false,
                () =>
                {
                    profileEditor.RemoveEffect(Name);
                });
                menu.DropDown(rect);
            }
            if (foldout)
            {
                EditorGUILayout.BeginVertical("box");
                editor.OnInspectorGUI();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }

    [CustomEditor(typeof(PostProcessProfile))]
    public class PostProcessProfileEditor : Editor
    {
        static PostProcessProfileEditor profileEditorInstance;
        PostProcessProfile profile;
        List<EffectEditor> postProcessEditors;
        string[] avairableEffectNames;
        string[] avairableEffectFullNames;
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

            avairableEffectFullNames = TypeCache.GetTypesDerivedFrom<PostProcessEffect>()
            .Select(x => x.FullName).ToArray();
        }

        void OnDisable()
        {
            profileEditorInstance = null;
        }

        public override void OnInspectorGUI()
        {
            foreach (var editor in postProcessEditors)
            {
                // editor.DrawHeader();
                // EditorGUILayout.BeginFoldoutHeaderGroup(true, editor.name);
                editor.GUI();
            }

            selectedIndex = EditorGUILayout.Popup(selectedIndex, avairableEffectNames);

            if (GUILayout.Button("Add Effect"))
            {
                AddEffect(avairableEffectFullNames[selectedIndex], avairableEffectNames[selectedIndex]);
            }
        }

        void AddEffect(string fullName, string name)
        {
            var effect = ScriptableObject.CreateInstance(fullName) as PostProcessEffect;
            effect.name = name;
            AssetDatabase.AddObjectToAsset(effect, profile);
            profile.Add(effect);
            Reimport();
            RefreshEditors();
        }

        public void RemoveEffect(string name)
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
            .Select(x => new EffectEditor(Editor.CreateEditor(x), this))
            .OrderBy(x => x.Name)
            .ToList();
        }
    }
}