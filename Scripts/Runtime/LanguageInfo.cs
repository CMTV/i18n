using System;
using UnityEngine;

namespace CMTV.I18n
{
    [Serializable]
    public struct LanguageInfo
    {
        [SerializeField]
        string code, name;

        public LanguageInfo(string code, string name)
        {
            this.code = code;
            this.name = name;
        }

        public string Code => code.Trim();
        public string Name => name.Trim();
    }
}