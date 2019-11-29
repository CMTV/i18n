using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace CMTV.I18n.EditorUtilities.UIElements
{
    public class Phrase : VisualElement
    {
        LanguageAccess lang;

        string _id, _text;

        public string Id
        {
            get
            {
                return this.Query<TextField>(className: "i18n-phrase--id").First().value.Trim();
            }

            set
            {
                this.Query<TextField>(className: "i18n-phrase--id").First().value = value;
            }
        }

        public string Text
        {
            get
            {
                return this.Query<TextField>(className: "i18n-phrase--text").First().value.Trim();
            }

            set
            {
                this.Query<TextField>(className: "i18n-phrase--text").First().value = value;
            }
        }

        #region State

        PhraseState state;

        public PhraseState State
        {
            get => state;

            set
            {
                state = value;

                foreach (var stateStr in Enum.GetNames(typeof(PhraseState)))
                {
                    RemoveFromClassList("i18n-phraseState--" + stateStr.ToLower());
                }

                if (state != PhraseState.Normal)
                {
                    AddToClassList("i18n-phraseState--edit");
                }

                AddToClassList("i18n-phraseState--" + state.ToString().ToLower());
            }
        }

        #endregion

        public Phrase(Language lang, string id, string text, PhraseState state = PhraseState.Normal)
        {
            AddToClassList("i18n-phrase");

            this.lang = new LanguageAccess(lang);

            _id = (state == PhraseState.Add) ? "" : id;
            _text = (state == PhraseState.Add) ? "" : text;

            State = state;

            #region Loading UXML

            var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.UXML("Phrase"));
            uiAsset.CloneTree(this);

            #endregion

            #region Loading stylesheet

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.USS("Phrase"));
            styleSheets.Add(styleSheet);

            #endregion
        
            #region Initializing Id and Text fields

            var idElem = this.Query<TextField>(className: "i18n-phrase--id").First();
            var textElem = this.Query<TextField>(className: "i18n-phrase--text").First();

            idElem.value = id;
            textElem.value = text;

            Action<TextField, string> valueChangeFunc = (textField, oldValue) =>
            {
                if (State == PhraseState.Normal)
                {
                    textField.value = oldValue;
                }
            };

            idElem.RegisterValueChangedCallback(e => valueChangeFunc(idElem, _id));
            textElem.RegisterValueChangedCallback(e => valueChangeFunc(textElem, _text));

            #endregion
        
            SetupMenu();
            SetupActions();
        }

        void SetupMenu()
        {
            var menuElem = this.Query<ToolbarMenu>(className: "i18n-phrase--menu").First();

            menuElem.menu.AppendAction("Edit", (e) =>
            {
                State = PhraseState.Edit;
            });

            menuElem.menu.AppendAction("Remove", (e) =>
            {
                if (EditorUtility.DisplayDialog("Remove phrase?", _id, "Remove", "Cancel"))
                {
                    RemovePhrase(_id);
                    RemoveFromHierarchy();
                } 
            });
        }

        void SetupActions()
        {
            Button cancel, apply;

            cancel = this.Query<Button>(className: "i18n-phrase--cancel").First();
            apply = this.Query<Button>(className: "i18n-phrase--apply").First();

            cancel.RegisterCallback<MouseUpEvent>(e =>
            {
                if (_id == "")
                {
                    RemoveFromHierarchy();
                }
                else
                {
                    RevertValues();
                    State = PhraseState.Normal;
                }
            });

            apply.RegisterCallback<MouseUpEvent>(e =>
            {
                var canAddEdit = CanAddEditPhrase(_id, Id);

                if (canAddEdit != null)
                {
                    this.Query<Label>(className: "i18n-phrase--message").First().text = canAddEdit;
                    State = PhraseState.Error;
                    return;
                }

                AddEditPhrase(_id, Id, Text);
                UpdateDefaultValues();

                State = PhraseState.Normal;
            });
        }

        void RevertValues()
        {
            Id = _id;
            Text = _text;
        }

        void UpdateDefaultValues()
        {
            _id = Id;
            _text = Text;
        }

        #region Language phrases

        void AddEditPhrase(string oldId, string id, string text)
        {
            RemovePhrase(oldId);

            lang.phrases.Add(new I18n.Phrase(lang.Lang.GetId(id), text));
            lang.Save();
        }

        void RemovePhrase(string id)
        {
            foreach (var phrase in lang.phrases)
            {
                if (phrase.Id == lang.Lang.GetId(id))
                {
                    lang.phrases.Remove(phrase);
                    break;
                }
            }

            lang.Save();
        }

        string CanAddEditPhrase(string oldId, string id)
        {
            if (id == "")
            {
                return "Phrase ID can't be empty!";
            }

            var phrase = lang.Lang.GetPhrase(id, strict: true);

            if (phrase != null && oldId == "")
            {
                return "Phrase ID must be unique!";
            }

            return null;
        }

        #endregion

        public enum PhraseState
        {
            Normal, Add, Edit, Error
        }
    }
}