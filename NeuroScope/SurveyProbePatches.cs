using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NeuroSdk.Actions;
using UnityEngine;

namespace NeuroScope
{

    [HarmonyPatch]
    public class SurveyProbePatches
    {
        public static HashSet<SurveyorProbe> surveyorProbes = new();
        public static Color surveyorProbeColor = Color.white;
        public static int surveyorProbeIntensity = 1;
        public static ProbeLauncher probeLauncher = null;   // Store the launcher to be able to take Snapshots
        
        [HarmonyPostfix, HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.LaunchProbe))]
        public static void ProbeLauncher_LaunchProbe_Postfix(ProbeLauncher __instance)
        {
            if (!NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Survey Probe")) return;
            probeLauncher = __instance;
            NeuroSdk.Messages.Outgoing.Context.Send(Utils.stripHtml("Player launched a surveyor probe"));
            NeuroActionHandler.RegisterActions(new SpinSurveyProbe());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.RetrieveProbe))]
        public static void ProbeLauncher_RetrieveProbe_Postfix()
        {
            if (!NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Survey Probe")) return;
            probeLauncher = null;
            NeuroActionHandler.UnregisterActions("spin_survey_probe");
             NeuroSdk.Messages.Outgoing.Context.Send(Utils.stripHtml("Surveyor probe retrieved"));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SurveyorProbe), nameof(SurveyorProbe.Awake))]
        public static void SurveyorProbe_Awake_Postfix(SurveyorProbe __instance)
        {
            if (!NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Survey Probe")) return;
            if (surveyorProbes.Count == 0) NeuroActionHandler.RegisterActions(new SetSurveyProbeColor());
            surveyorProbes.Add(__instance);
            NeuroScope.Instance.ModHelper.Console.WriteLine($"SurveyorProbe Awake called. Current probes: {string.Join(", ", surveyorProbes.Select(p => p.name))}");
            Utils.updateSurveyProbeLights();
        }
    }
}