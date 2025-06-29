using System.Linq;
using HarmonyLib;
using NeuroSdk.Actions;
using UnityEngine;

namespace NeuroScope
{

    [HarmonyPatch]
    public class Patches
    {

        [HarmonyPostfix, HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.DisplayDialogueBox2))]
        public static void CharacterDialogueTree_DisplayDialogueBox2_Postfix(CharacterDialogueTree __instance, DialogueBoxVer2 __result)
        {
            if (NeuroScope.Instance.ModHelper.Config.GetSettingsValue<string>("Dialogue").Equals("Disabled")) return;

            // Make sure old CharacterDialogueOptionActions are 
            if (NeuroActionHandler.GetRegistered("dialogue_option") != null)
            {
                NeuroActionHandler.UnregisterActions("dialogue_option");
            }

            // If there are no DialogueOptions we can just send the text as Context
            if (!__result._displayedOptions.Any())
            {
                Utils.sendContext("Dialogue", Utils.getText(__instance));
                return;
            }

            // If there are DialogueOptions, let Neuro choose an option
            ActionWindow.Create(__result._optionBox)
                .SetForce(0, "Select a dialogue option.", "", false)
                .AddAction(new Actions.CharacterDialogueOptionAction(__instance))
                .Register();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NomaiText), nameof(NomaiText.SetAsTranslated))]
        public static void NomaiText_SetAsTranslated_Prefix(NomaiText __instance, int id)
        {
            __instance.VerifyInitialized();
            if (!__instance._dictNomaiTextData.ContainsKey(id)) return;
            if (__instance._dictNomaiTextData[id].IsTranslated) return;

            Utils.sendContext("Nomai Writing", $"[NOMAI WRITING] {__instance._dictNomaiTextData[id].TextNode.InnerText}");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerCameraEffectController), nameof(PlayerCameraEffectController.OnPlayerDeath))]
        public static void PlayerCameraEffectController_OnPlayerDeath_Postfix(DeathType deathType)
        {
            Utils.sendContext("Death", $"You died{(deathType.Equals(DeathType.Default) ? "" : $" - Cause: {deathType}")}");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Steamworks.SteamUserStats), nameof(Steamworks.SteamUserStats.SetAchievement))]
        public static void SteamUserStats_SetAchievement_Prefix(string pchName)
        {
            Steamworks.SteamUserStats.GetAchievement(pchName, out bool achieved);
            if (achieved) return; // Make sure achievement is not already unlocked
            Utils.sendContext("Achievements", $"[ACHIEVEMENT UNLOCKED] " +
                $"{Steamworks.SteamUserStats.GetAchievementDisplayAttribute(pchName, "name")} - " +
                $"{Steamworks.SteamUserStats.GetAchievementDisplayAttribute(pchName, "desc")}");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NotificationManager), nameof(NotificationManager.PostNotification))]
        public static void NotificationManager_PostNotification_Prefix(NotificationManager __instance, NotificationData data)
        {
            if (!PlayerState.IsWearingSuit() || !PlayerState.IsInsideShip()) return; // Notifications are only shown when the player is wearing the spacesuit or inside the ship
            if (__instance._pinnedNotifications.Contains(data)) return; // Don't send duplicate notifications
            Utils.sendContext("Notifications", $"[NOTIFICATION] {data.displayMessage}");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Sector), nameof(Sector.OnEntry))]
        public static void Sector_OnEntry_Prefix(Sector __instance, GameObject hitObj)
        {
            SectorDetector component = hitObj.GetComponent<SectorDetector>();
            if (component == null) return;
            if (component.GetOccupantType() != DynamicOccupant.Player) return;
            string sectorName = Utils.sectorToString(__instance);
            if (sectorName == null) return;
            Utils.sendContext("Location", $"[LOCATION] Player entered {sectorName}");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Sector), nameof(Sector.OnExit))]
        public static void Sector_OnExit_Prefix(Sector __instance, GameObject hitObj)
        {
            SectorDetector component = hitObj.GetComponent<SectorDetector>();
            if (component == null) return;
            if (component.GetOccupantType() != DynamicOccupant.Player) return;
            string sectorName = Utils.sectorToString(__instance);
            if (sectorName == null) return;
            Utils.sendContext("Location", $"[LOCATION] Player left {sectorName}");
        }
    }
}