using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace I18n
{
    [CreateAssetMenu(fileName = "New Language", menuName = "I18n/Language", order = 0)]
    public class Language : ScriptableObject 
    {
        #pragma warning disable 649

        [SerializeField]
        bool isDefault;

        [SerializeField]
        LanguageInfo info;

        [SerializeField]
        List<Phrase> phrases;

        #pragma warning restore 649

        public bool IsDefault => isDefault;

        public LanguageInfo Info => info;
        
        public IReadOnlyCollection<Phrase> Phrases => phrases.AsReadOnly();

        #region GetPhrase

        public string GetPhrase(string id, bool strict = false)
        {
            var splitId = SplitId(id);

            string clearId = splitId[0];
            string ending = splitId[1];

            var matches = phrases.Where(phrase => phrase.Id == clearId);

            if (matches.Count() == 0)
            {
                return strict ? null : id;
            }

            return matches.First().Text + ending;
        }

        public string GetPhrase(string id, Dictionary<string, string> phraseParams)
        {
            string text = GetPhrase(id, strict: true);

            if (text == null)
            {
                return id;
            }

            foreach (var param in phraseParams)
            {
                text = text.Replace("{" + param.Key + "}", param.Value);
            }

            return text;
        }

        #endregion

        public string[] SplitId(string id)
        {
            var punctuation = new string[]
            {
                "...", ".", ":", ";",
                "!?", "?!", "!", "?"
            };

            string ending = "";

            foreach (var item in punctuation)
            {
                if (id.EndsWith(item))
                {
                    ending = item;
                    break;
                }
            }

            var clearId = ending != "" ? id.Substring(0, id.LastIndexOf(ending)) : id;

            return new string[] { clearId, ending };
        }

        public string GetId(string id)
        {
            return SplitId(id)[0];
        }

        public string GetEnding(string id)
        {
            return SplitId(id)[1];
        }
    }
}