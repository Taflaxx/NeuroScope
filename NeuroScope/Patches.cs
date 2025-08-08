using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using HarmonyLib;
using NeuroSdk.Actions;
using UnityEngine;

namespace NeuroScope
{

    [HarmonyPatch]
    public class Patches
    {

        private static float _lastSignalTime = -10f;
        private static AudioSignal _lastSignal = null;

        /// <summary>
        /// This finalizer makes sure that any exceptions in the CharacterDialogueTree.DisplayDialogueBox2 Patch are suppressed.
        /// Otherwise, the dialogue would get stuck and the game would need to be restarted.
        /// </summary>
        [HarmonyFinalizer, HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.DisplayDialogueBox2))]
        public static System.Exception Finalizer(System.Exception __exception)
        {
            if (__exception == null) return null; // No exception, nothing to do
            NeuroScope.Instance.ModHelper.Console.WriteLine($"[NeuroScope] Exception in CharacterDialogueTree: {__exception.Message}\n{__exception.StackTrace}");
            return null;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.DisplayDialogueBox2))]
        public static void CharacterDialogueTree_DisplayDialogueBox2_Postfix(CharacterDialogueTree __instance, DialogueBoxVer2 __result)
        {
            if (NeuroScope.Instance.ModHelper.Config.GetSettingsValue<string>("Dialogue").Equals("Disabled")) return;

            // Make sure old CharacterDialogueOptionActions are unregistered
            if (NeuroActionHandler.GetRegistered("dialogue_option") != null)
            {
                NeuroActionHandler.UnregisterActions("dialogue_option");
            }
            
            Utils.sendContext("Dialogue", Utils.getText(__instance));
            if (!__result._displayedOptions.Any()) return;

            String optionsText = string.Join("\n", __result._displayedOptions.Select(option => Utils.stripHtml(option._text)));
            Utils.sendContext("Dialogue", $"[DIALOGUE] You can respond to {(__instance._characterName == "" ? "the NPC" : TextTranslation.Translate(__instance._characterName))} with the following options:\n" + optionsText);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NomaiText), nameof(NomaiText.SetAsTranslated))]
        public static void NomaiText_SetAsTranslated_Prefix(NomaiText __instance, int id)
        {
            __instance.VerifyInitialized();
            if (!__instance._dictNomaiTextData.ContainsKey(id)) return;
            if (__instance._dictNomaiTextData[id].IsTranslated) return;

            Utils.sendContext("Nomai Writing", $"[NOMAI WRITING] {__instance._dictNomaiTextData[id].TextNode.InnerText}");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerCameraEffectController), nameof(PlayerCameraEffectController.WakeUp))]
        public static void PlayerCameraEffectController_WakeUp_Postfix()
        {
            Utils.sendContext("Death", $"Player woke up");
            NeuroActionHandler.RegisterActions(new GetPlayerStatusAction(), new GetShipStatusAction());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerCameraEffectController), nameof(PlayerCameraEffectController.OnPlayerDeath))]
        public static void PlayerCameraEffectController_OnPlayerDeath_Postfix(DeathType deathType)
        {
            string cause = $" - Cause: {deathType}";
            // Don't spoil the supernova in the first 10 loops
            if (deathType.Equals(DeathType.Default) || (deathType.Equals(DeathType.Supernova) && TimeLoop.GetLoopCount() <= 10))
            {
                cause = "";
            }
            Utils.sendContext("Death", $"Player died{cause}");

            // Unregister all Actions
            NeuroActionHandler.UnregisterActions("dialogue_option");
            NeuroActionHandler.UnregisterActions("get_player_status");
            NeuroActionHandler.UnregisterActions("get_ship_status");
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
            if (__instance._name == Sector.Name.QuantumMoon) return; // Prevent spoiling the Quantum Moon
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
            if (PlayerState.IsDead()) return;
            if (__instance._name == Sector.Name.QuantumMoon) return; // Prevent spoiling the Quantum Moon
            SectorDetector component = hitObj.GetComponent<SectorDetector>();
            if (component == null) return;
            if (component.GetOccupantType() != DynamicOccupant.Player) return;
            string sectorName = Utils.sectorToString(__instance);
            if (sectorName == null) return;
            Utils.sendContext("Location", $"[LOCATION] Player left {sectorName}");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ShipLogFact), nameof(ShipLogFact.Reveal))]
        public static void ShipLogFact_Reveal_Prefix(ShipLogFact __instance)
        {
            if (__instance.IsRevealed()) return; // Make sure we only send the fact once
            ShipLogEntry shipLogEntry = Locator.GetShipLogManager().GetEntry(__instance._entryID);
            string astroObjectName = AstroObject.AstroObjectNameToString(AstroObject.StringIDToAstroObjectName(shipLogEntry._astroObjectID));
            Utils.sendContext("Ship Log Fact", $"[NEW SHIP LOG FACT] {astroObjectName} - {shipLogEntry.GetName(false)}: {__instance.GetText()}");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SignalscopeUI), nameof(SignalscopeUI.UpdateLabels))]
        public static void SignalscopeUI_UpdateLabels_Postfix(SignalscopeUI __instance)
        {
            AudioSignal strongestSignal = __instance._signalscopeTool.GetStrongestSignal();
            if (_lastSignal == strongestSignal && Time.time - _lastSignalTime < 10f) return;
            if (strongestSignal == null || strongestSignal.GetSignalStrength() < 0.9f) return;
            string text = PlayerData.KnowsSignal(strongestSignal.GetName()) ? AudioSignal.SignalNameToString(strongestSignal.GetName()) : UITextLibrary.GetString(UITextType.UnknownSignal);
            Utils.sendContext("Signalscope", $"[SIGNALSCOPE] Listening to Signal: '{text}' | Distance: {Mathf.Round(strongestSignal.GetDistanceFromScope())}m | Frequency: '{AudioSignal.FrequencyToString(strongestSignal._frequency, false)}'");
            _lastSignalTime = Time.time;
            _lastSignal = strongestSignal;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GlobalMessenger), nameof(GlobalMessenger.FireEvent))]
        public static void GlobalMessenger_FireEvent_Postfix(string eventType)
        {
            switch (eventType)
            {
                case "PlayerEnterQuantumMoon":
                    Utils.sendContext("Location", "[LOCATION] Player entered the Quantum Moon");
                    break;
                case "PlayerExitQuantumMoon":
                    Utils.sendContext("Location", "[LOCATION] Player left the Quantum Moon");
                    break;
                case "PlayerEnterBlackHole":
                    Utils.sendContext("Location", "[LOCATION] Player entered a Black Hole");
                    break;
                case "PlayerEscapedTimeLoop":
                    Utils.sendContext("Death", "Player escaped the time loop");
                    break;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Campfire), nameof(Campfire.StartRoasting))]
        public static void Campfire_StartRoasting_Postfix()
        {
            Utils.sendContext("Misc", $"Player started roasting a marchmallow");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Marshmallow), nameof(Marshmallow.Eat))]
        public static void Marshmallow_Eat_Prefix(Marshmallow __instance)
        {
            Utils.sendContext("Misc", $"Player ate a {(__instance.IsBurned() ? "burned " : "")}marshmallow");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Campfire), nameof(Campfire.StartSleeping))]
        public static void Campfire_StartSleeping_Postfix()
        {
            Utils.sendContext("Misc", $"Player started sleeping at a campfire");
        }
    }
}