using UnityEngine;
using UnityEngine.UI;

namespace CMTV.I18n
{
    [AddComponentMenu("UI/Language Text")]
    [RequireComponent(typeof(Text))]
    public class LangText : MonoBehaviour 
    {
        public bool updateOnLangChange = true;

        string phraseId;

        public Text Text
        {
            get
            {
                return GetComponent<Text>();
            }
        }

        void Start()
        {
            phraseId = Text.text;

            UpdateText();

            if (updateOnLangChange)
            {
                Lang.Instance.onLanguageSwitch.AddListener(UpdateText);
            }
        }

        public void UpdateText()
        {
            try
            {
                Text.text = Lang.Phrase(phraseId);
            }
            catch
            {
                Debug.LogWarning("Can't update Text!");
            }
        }
    }   
}