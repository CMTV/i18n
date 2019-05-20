using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;

namespace I18n
{
    public class Lang : MonoBehaviour
    {
        public UnityEvent onLanguageSwitch = new UnityEvent();

        Language[] langs;

        Language defaultLang;

        Language currentLang;
        string currentLangCode;

        #region Singleton

        static Lang _instance;
        public static Lang Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject lngManager = new GameObject("Language Manager");
                    _instance = lngManager.AddComponent<Lang>();
                    _instance.Init();
                    DontDestroyOnLoad(lngManager);
                }

                return _instance;
            }
        }

        #endregion

        void Init()
        {
            LoadLangs();
            LoadCurrent();
        }

        void LoadLangs()
        {
            Language[] _langs = Resources.LoadAll<Language>("Languages");
            Dictionary<string, Language> langMap = new Dictionary<string, Language>();

            int skipCounter = 0;

            foreach (var lang in _langs)
            {
                if (lang.Info.Code == "")
                {
                    Debug.LogWarning("Language \"" + lang.Info.Name + "\" has empty code! Skipping!");
                    skipCounter++;
                    continue;
                }

                if (lang.Info.Name == "")
                {
                    Debug.LogWarning("Language with code \"" + lang.Info.Code + "\" has empty name! Skipping!");
                    skipCounter++;
                    continue;
                }

                if (langMap.ContainsKey(lang.Info.Code))
                {
                    Debug.LogWarning("Code (" + lang.Info.Code + ") duplicate for language \"" + lang.Info.Name + "\". Skipping!");
                    skipCounter++;
                    continue;
                }

                if (lang.IsDefault)
                {
                    defaultLang = lang;
                }

                langMap.Add(lang.Info.Code, lang);
            }

            langs = langMap.Values.ToArray();

            var defaultName = defaultLang == null ? "none" : defaultLang.Info.Name;

            Debug.Log(langs.Length + " languages were loaded. " + skipCounter + " skipped. Default language: \"" + defaultName + "\"");
        }
    
        Language GetLang(string code)
        {
            foreach (var lang in langs)
            {
                if (lang.Info.Code == code)
                {
                    return lang;
                }
            }

            Debug.LogWarning("No language with code \"" + code + "\" found!");
            
            return null;
        }

        void Switch(string code)
        {
            currentLang = GetLang(code);
            onLanguageSwitch.Invoke();
            SaveCurrent();
        }

        #region Save and Load current language

        void SaveCurrent()
        {
            if (currentLangCode == null)
            {
                return;
            }

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream file = File.Create(Application.persistentDataPath + "/lang.data"))
            {
                bf.Serialize(file, currentLangCode);
            }
        }

        void LoadCurrent()
        {
            if (!File.Exists(Application.persistentDataPath + "/lang.data"))
            {
                return;
            }

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream file = File.Open(Application.persistentDataPath + "/lang.data", FileMode.Open))
            {
                while (file.Position < file.Length)
                {
                    try
                    {
                        currentLangCode = (string)bf.Deserialize(file);
                    }
                    catch { }
                }
            }

            currentLang = (currentLangCode == null) ? defaultLang : GetLang(currentLangCode);
        }

        #endregion
    
        public static string Phrase(string id)
        {
            if (Instance.currentLang == null)
            {
                return id;
            }

            return Instance.currentLang.GetPhrase(id);
        }

        public static string Phrase(string id, Dictionary<string, string> phraseParams)
        {
            if (Instance.currentLang == null)
            {
                return id;
            }

            return Instance.currentLang.GetPhrase(id, phraseParams);
        }

        public static LanguageInfo[] GetLangs()
        {
            return Instance.langs.Select(language => language.Info).ToArray();
        }
    }
}