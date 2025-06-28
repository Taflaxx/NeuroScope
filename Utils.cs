using System;
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
            if (characterDialogueTree._characterName != "")
            {
                text = $"{TextTranslation.Translate(characterDialogueTree._characterName)}: {text}";
            }
            return stripHtml(text);
        }

        public static string stripHtml(string text)
        {
            return Regex.Replace(text, "<.*?>", System.String.Empty);
        }
    }
}