using System;
using UnityEngine;

namespace I18n
{
    [Serializable]
    public struct Phrase
    {
        [SerializeField]
        string id, text;

        public Phrase(string id, string text)
        {
            this.id = id;
            this.text = text;
        }

        public string Id => id;
        public string Text => text;
    }
}