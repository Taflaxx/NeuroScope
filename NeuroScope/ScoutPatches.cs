using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NeuroSdk.Actions;
using UnityEngine;

namespace NeuroScope
{

    [HarmonyPatch]
    public class ScoutPatches
    {
        public static HashSet<SurveyorProbe> surveyorProbes = new();
        public static Color surveyorProbeColor = Color.white;
        public static int surveyorProbeIntensity = 1;
        public static ProbeLauncher probeLauncher = null;   // Store the launcher to be able to take Snapshots

        [HarmonyPostfix, HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.LaunchProbe))]
        public static void ProbeLauncher_LaunchProbe_Postfix(ProbeLauncher __instance)
        {
            // Notify Neuro of the probe launch if manual control is enabled
            if (NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Scout Launcher")) return;
            probeLauncher = __instance;
            NeuroSdk.Messages.Outgoing.Context.Send(Utils.stripHtml("Scout launcher launched a surveyor probe"));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.RetrieveProbe))]
        public static void ProbeLauncher_RetrieveProbe_Postfix()
        {
            NeuroActionHandler.UnregisterActions("take_scout_photo", "retrieve_scout", "spin_scout", "turn_scout_camera");

            // Notify Neuro of the probe retrieval if manual control is enabled
            if (NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Scout Launcher")) return;
            probeLauncher = null;
            NeuroSdk.Messages.Outgoing.Context.Send(Utils.stripHtml("Scout retrieved"));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SurveyorProbe), nameof(SurveyorProbe.Awake))]
        public static void SurveyorProbe_Awake_Postfix(SurveyorProbe __instance)
        {
            if (!NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Scout Launcher")) return;
            if (surveyorProbes.Count == 0) NeuroActionHandler.RegisterActions(new SetScoutColorAction());
            surveyorProbes.Add(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.AllowLaunchMode))]
        [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.InPhotoMode))]
        public static bool ProbeLauncher_AllowLaunchMode_Prefix(ref bool __result)
        {
            // Prevent manual probe control
            if (NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Scout Launcher"))
            {
                __result = false;
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.UpdatePostLaunch))]
        [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.InPhotoMode))]
        public static bool ProbeLauncher_UpdatePostLaunch_Prefix()
        {
            // Prevent manual probe control
            if (NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Scout Launcher"))
            {
                return false;
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.EquipTool))]
        public static void SurveyorProbe_EquipTool_Postfix(ProbeLauncher __instance)
        {
            if (!NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Scout Launcher")) return;
            probeLauncher = __instance;
            NeuroActionHandler.RegisterActions(new LauchScoutAction());
            NeuroSdk.Messages.Outgoing.Context.Send(Utils.stripHtml("Scout launcher equipped."));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.UnequipTool))]
        public static void SurveyorProbe_UnequipTool_Postfix(ProbeLauncher __instance)
        {
            if (!NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Scout Launcher")) return;
            probeLauncher = __instance;
            NeuroActionHandler.UnregisterActions("launch_scout");
            NeuroSdk.Messages.Outgoing.Context.Send(Utils.stripHtml("Scout launcher unequipped."));
        }
    }
}