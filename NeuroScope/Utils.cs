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
            if (characterDialogueTree._characterName == "SIGN")
            {
                text = $"[SIGN] {text}";
            }
            else if (characterDialogueTree._characterName != "")
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

        public static string sectorToString(Sector sector)
        {
            switch (sector.GetName())
            {

                case Sector.Name.Sun:
                    return null;    // Sun Sector is too big
                case Sector.Name.HourglassTwin_A:
                    return null;    // Already handled by Sector.Name.HourglassTwins
                case Sector.Name.HourglassTwin_B:
                    return null;    // Already handled by Sector.Name.HourglassTwins
                case Sector.Name.TimberHearth:
                    return UITextLibrary.GetString(UITextType.LocationTH);
                case Sector.Name.BrittleHollow:
                    return UITextLibrary.GetString(UITextType.LocationBH);
                case Sector.Name.GiantsDeep:
                    return UITextLibrary.GetString(UITextType.LocationGD);
                case Sector.Name.DarkBramble:
                    return UITextLibrary.GetString(UITextType.LocationDB);
                case Sector.Name.Comet:
                    return UITextLibrary.GetString(UITextType.LocationCo);
                case Sector.Name.QuantumMoon:
                    return UITextLibrary.GetString(UITextType.LocationQM);
                case Sector.Name.TimberMoon:
                    return UITextLibrary.GetString(UITextType.LocationTHMoon);
                case Sector.Name.BrambleDimension:
                    return UITextLibrary.GetString(UITextType.LocationDB) + " Interior";
                case Sector.Name.VolcanicMoon:
                    return UITextLibrary.GetString(UITextType.LocationBHMoon);
                case Sector.Name.OrbitalProbeCannon:
                    return UITextLibrary.GetString(UITextType.LocationOPC);
                case Sector.Name.EyeOfTheUniverse:
                    return UITextLibrary.GetString(UITextType.LocationEye);
                case Sector.Name.Ship:
                    return "Ship";
                case Sector.Name.SunStation:
                    return UITextLibrary.GetString(UITextType.LocationSS);
                case Sector.Name.WhiteHole:
                    return UITextLibrary.GetString(UITextType.LocationWH);
                case Sector.Name.TimeLoopDevice:
                    return null;
                case Sector.Name.Vessel:
                    return "The Vessel";
                case Sector.Name.VesselDimension:
                    return "The Vessel";
                case Sector.Name.HourglassTwins:
                    return UITextLibrary.GetString(UITextType.LocationHGT);
                case Sector.Name.InvisiblePlanet:
                    return UITextLibrary.GetString(UITextType.LocationIP);
                case Sector.Name.DreamWorld:
                    return null;
                case Sector.Name.Unnamed:
                    return null;
                default:
                    return null;
            }
        }

        public static void updateSurveyProbeLights()
        {
            foreach (SurveyorProbe probe in SurveyProbePatches.surveyorProbes)
            {
                if (probe == null) continue;
                foreach (OWLight2 light in probe.GetLights())
                {
                    light._light.color = SurveyProbePatches.surveyorProbeColor;
                    light.SetIntensity(SurveyProbePatches.surveyorProbeIntensity);
                }
            }
        }
    }
}