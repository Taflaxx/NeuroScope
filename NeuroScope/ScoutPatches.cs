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
            if (NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Scout Launcher (Neuro)"))
            {
                NeuroActionHandler.RegisterActions(new TakeScoutPhotoAction(), new RetrieveScoutAction(), new SpinScoutAction(), new TurnScoutCameraAction());
            }
            probeLauncher = __instance;
            NeuroSdk.Messages.Outgoing.Context.Send(Utils.stripHtml("Scout launcher launched a scout"));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.RetrieveProbe))]
        public static void ProbeLauncher_RetrieveProbe_Postfix()
        {
            NeuroActionHandler.UnregisterActions("take_scout_photo", "retrieve_scout", "spin_scout", "turn_scout_camera");
            NeuroSdk.Messages.Outgoing.Context.Send(Utils.stripHtml("Scout retrieved"));
            if (probeLauncher.IsEquipped()) NeuroActionHandler.RegisterActions(new LauchScoutAction());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SurveyorProbe), nameof(SurveyorProbe.Awake))]
        public static void SurveyorProbe_Awake_Postfix(SurveyorProbe __instance)
        {
            if (!NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Scout Launcher (Neuro)")) return;
            if (surveyorProbes.Count == 0) NeuroActionHandler.RegisterActions(new SetScoutColorAction());
            surveyorProbes.Add(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.UpdatePostLaunch))]
        [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.UpdatePreLaunch))]
        public static bool ProbeLauncher_UpdatePostLaunch_Prefix()
        {
            // Prevent manual probe control
            if (NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Scout Launcher (Neuro)") &&
                !NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Scout Launcher (Manual Control)"))
            {
                return false;
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.EquipTool))]
        public static void SurveyorProbe_EquipTool_Postfix(ProbeLauncher __instance)
        {
            if (!NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Scout Launcher (Neuro)")) return;
            probeLauncher = __instance;
            NeuroActionHandler.RegisterActions(new LauchScoutAction());
            NeuroSdk.Messages.Outgoing.Context.Send(Utils.stripHtml("Scout launcher equipped."));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.UnequipTool))]
        public static void SurveyorProbe_UnequipTool_Postfix(ProbeLauncher __instance)
        {
            if (!NeuroScope.Instance.ModHelper.Config.GetSettingsValue<bool>("Scout Launcher (Neuro)")) return;
            NeuroActionHandler.UnregisterActions("launch_scout");
            NeuroSdk.Messages.Outgoing.Context.Send(Utils.stripHtml("Scout launcher unequipped."));
        }
    }
}