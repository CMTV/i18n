using System; 
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using I18n.EditorUtilities.UIElements;

namespace I18n.EditorUtilities
{
    public class Translator : EditorWindow 
    {
        SearchFilter sFilter = SearchFilter.All;
        bool sMatchCase = false;

        public string SQuery
        {
            get
            {
                var query = Root.Query<ToolbarPopupSearchField>("search").First().value.TrimEnd();

                if (!sMatchCase)
                {
                    query = query.ToLower();
                }

                return query;
            }
        }

        Language lang;

        public Language Lang
        {
            get => lang;

            set
            {
                lang = value;

                if (lang != null)
                {
                    EditorUtility.SetDirty(lang);
                }

                SetupLayout();
            }
        }

        VisualElement Root => rootVisualElement;

        [MenuItem("Window/Translator")]
        public static Translator ShowWindow()
        {
            var window = GetWindow<Translator>();
            window.titleContent = new GUIContent("Translator");
            window.minSize = new Vector2(300, 300);
            window.Show();

            return window;
        }

        void OnEnable()
        {
            #region Loading stylesheet

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.USS("Translator"));
            Root.styleSheets.Add(styleSheet);

            #endregion
        
            SetupLayout();
        }
        
        #region Layouts

        void SetupLayout()
        {
            ClearLayout();

            if (Lang != null)
            {
                SetupLangLayout();
            }
            else
            {
                SetupNoLangLayout();
            }
        }

        void SetupLangLayout()
        {
            Root.AddToClassList("has-lang");

            #region Loading UXML

            var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.UXML("Translator"));
            uiAsset.CloneTree(Root);

            #endregion
        
            #region Toolbar actions

            var langBtn = Root.Query<Button>("lang-switcher").First();

            langBtn.text = Lang.Info.Code;
            langBtn.RegisterCallback<MouseUpEvent>((e) => OpenLangPicker(Lang));

            var addBtn = Root.Query<Button>("add-phrase").First();

            addBtn.RegisterCallback<MouseUpEvent>((e) =>
            {
                var phrase = new UIElements.Phrase(
                    Lang,
                    "id", "text",
                    UIElements.Phrase.PhraseState.Add
                );

                Root.Query("phrase-container").First().Add(phrase);                
            });

            #endregion
        
            #region Search

            var searchBar = Root.Query<ToolbarPopupSearchField>("search").First();

            SetupSearchMenu(searchBar);

            searchBar.RegisterValueChangedCallback((e) =>
            {
                FillPhraseList();
            });

            #endregion
        
            FillPhraseList();
        }

        void SetupSearchMenu(ToolbarPopupSearchField searchBar)
        {
            foreach (var fName in Enum.GetNames(typeof(SearchFilter)))
            {
                searchBar.menu.AppendAction(
                    fName,
                    (e) => 
                    { 
                        sFilter = (SearchFilter)Enum.Parse(typeof(SearchFilter), fName);
                        FillPhraseList();
                    },
                    (e) => 
                    { 
                        return sFilter.ToString() == fName ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal; 
                    }
                );
            }

            searchBar.menu.AppendSeparator();

            searchBar.menu.AppendAction(
                "Match case",
                (e) => 
                {
                    sMatchCase = !sMatchCase;
                    FillPhraseList();
                },
                (e) =>
                {
                    return sMatchCase ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
                }
            );
        }

        void SetupNoLangLayout()
        {
            Root.AddToClassList("no-lang");

            var message = new Label("Select a language to work with Translator.");
            
            var selectBtn = new Button(() =>
            {
                OpenLangPicker();
            });

            selectBtn.text = "Select";

            Root.Add(message);
            Root.Add(selectBtn);
        }

        void ClearLayout()
        {
            Root.Clear();
            Root.RemoveFromClassList("no-lang");
            Root.RemoveFromClassList("has-lang");
        }

        #endregion

        #region Language picker

        void OnGUI()
        {
            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                Lang = (Language)EditorGUIUtility.GetObjectPickerObject();
            }
        }

        void OpenLangPicker(Language langToShow = null)
        {
            EditorGUIUtility.ShowObjectPicker<Language>(langToShow, false, "", 0);
        }

        #endregion
    
        void FillPhraseList()
        {
            var phraseContainer = Root.Query("phrase-container").First();
            
            phraseContainer.Clear();

            foreach (var phrase in Lang.Phrases)
            {
                if (SQuery != "")
                {
                    var pId = (sMatchCase) ? phrase.Id : phrase.Id.ToLower();
                    var pText = (sMatchCase) ? phrase.Text : phrase.Text.ToLower();

                    switch (sFilter)
                    {
                        case SearchFilter.All:
                            if (!pId.Contains(SQuery) && !pText.Contains(SQuery))
                                continue;
                            break;
                        case SearchFilter.ID:
                            if (!pId.Contains(SQuery))
                                continue;
                            break;
                        case SearchFilter.Text:
                            if (!pText.Contains(SQuery))
                                continue;
                            break;
                    }
                }

                var phraseElem = new UIElements.Phrase(
                    Lang,
                    phrase.Id, phrase.Text,
                    UIElements.Phrase.PhraseState.Normal
                );

                phraseContainer.Add(phraseElem);
            }
        }

        enum SearchFilter
        {
            All, ID, Text
        }
    }
}