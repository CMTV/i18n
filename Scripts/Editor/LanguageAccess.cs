using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace CMTV.I18n.EditorUtilities
{
    public class LanguageAccess
    {
        Language lang;

        public Language Lang => lang;

        public List<Phrase> phrases;

        FieldInfo Field
        {
            get
            {
                return typeof(Language).GetField("phrases", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        public LanguageAccess(Language lang)
        {
            this.lang = lang;
            EditorUtility.SetDirty(lang);

            phrases = (List<Phrase>)Field.GetValue(lang);
        }

        public void Save()
        {
            Field.SetValue(lang, phrases);
        }
    }   
}