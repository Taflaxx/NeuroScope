using System.Text.RegularExpressions;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
            foreach (SurveyorProbe probe in ScoutPatches.surveyorProbes)
            {
                if (probe == null) continue;
                foreach (OWLight2 light in probe.GetLights())
                {
                    light._light.color = ScoutPatches.surveyorProbeColor;
                    light.SetIntensity(ScoutPatches.surveyorProbeIntensity);
                }
            }
        }
        public static async UniTask turnSurveyorProbe(SurveyorProbe surveyorProbe, string direction, int steps)
        {
            float rotationStep = (direction == "left" || direction == "up") ? -30f : 30f;
            NeuroScope.Instance.ModHelper.Console.WriteLine($"Turning surveyor probe {direction} by {steps} steps ({rotationStep * steps} degrees).");
            float duration = 2f * ((float)steps / 12f); // Full rotation should take 2 seconds
            var rotatingCamera = surveyorProbe.GetRotatingCamera();

            float stepDuration = duration / steps;

            for (int i = 1; i <= steps; i++)
            {
                if (!surveyorProbe.IsAnchored()) return; // Handles retrieval during rotation
                if (direction == "left" || direction == "right") {
                    rotatingCamera.RotateHorizontal(rotationStep);
                } else {
                    rotatingCamera.RotateVertical(rotationStep);
                }

                ScoutPatches.probeLauncher.TakeSnapshotWithCamera(rotatingCamera);
                await UniTask.Delay((int)(stepDuration * 1000));
            }
        } 
    }
}