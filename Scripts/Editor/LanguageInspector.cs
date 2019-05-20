using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace I18n.EditorUtilities
{
    [CustomEditor(typeof(Language))]
    public class LanguageInspector : Editor
    {
        VisualElement root;

        SerializedProperty Code => serializedObject.FindProperty("info.code");
        SerializedProperty Name => serializedObject.FindProperty("info.name");
        SerializedProperty IsDefault => serializedObject.FindProperty("isDefault");

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            #region Loading UXML

            var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.UXML("LanguageInspector"));
            uiAsset.CloneTree(root);

            #endregion

            #region Loading stylesheet

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.USS("LanguageInspector"));
            root.styleSheets.Add(styleSheet);

            #endregion

            #region Language code and name

            TextField code, name;

            code = root.Query<TextField>("code").First();
            name = root.Query<TextField>("name").First();

            code.value = Code.stringValue;
            name.value = Name.stringValue;

            code.RegisterValueChangedCallback((e) => 
            {
                Code.stringValue = e.newValue;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            });

            name.RegisterValueChangedCallback((e) =>
            {
                Name.stringValue = e.newValue;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            });

            #endregion

            #region "Is default" toggle

            Toggle isDefault = root.Query<Toggle>("is-default").First();

            isDefault.value = IsDefault.boolValue;

            isDefault.RegisterValueChangedCallback(e =>
            {
                IsDefault.boolValue = e.newValue;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            });

            #endregion

            #region "Open in Translator" button

            Button openButton = root.Query<Button>("open").First();
            
            openButton.RegisterCallback<MouseUpEvent>(e => 
            {
                Translator.ShowWindow().Lang = (Language)target;
            });

            #endregion

            return root;
        }

        protected override bool ShouldHideOpenButton()
        {
            return true;
        }
    }
}