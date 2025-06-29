using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NeuroScope
{
    public class Utils
    {
        public static string getText(CharacterDialogueTree characterDialogueTree)
        {
            string text = characterDialogueTree._currentNode._name + characterDialogueTree._currentNode._listPagesToDisplay[characterDialogueTree._currentNode._currentPage];
            text = TextTranslation.Translate(text).Trim();
            if (characterDialogueTree._characterName == "SIGN") {
                text = $"[SIGN] {text}";
            } else if (characterDialogueTree._characterName != "")
            {
                text = $"[DIALOGUE] {TextTranslation.Translate(characterDialogueTree._characterName)}: {text}";
            }
            return stripHtml(text);
        }

        public static string stripHtml(string text)
        {
            return Regex.Replace(text, "<.*?>", System.String.Empty);
        }

        public static void sendContext(string settingsKey, string text)
        {
            if (NeuroScope.Instance.ModHelper.Config.GetSettingsValue<string>(settingsKey).Equals("Disabled")) return;
            NeuroSdk.Messages.Outgoing.Context.Send(stripHtml(text), !NeuroScope.Instance.ModHelper.Config.GetSettingsValue<string>(settingsKey).Equals("Enabled"));
        }
    }
}