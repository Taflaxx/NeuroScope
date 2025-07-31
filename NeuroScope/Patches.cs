using System.Linq;
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

            // If there are no DialogueOptions we can just send the text as Context
            if (!__result._displayedOptions.Any())
            {
                Utils.sendContext("Dialogue", Utils.getText(__instance));
                return;
            }

            // If there are DialogueOptions, let Neuro choose an option
            ActionWindow.Create(__result._optionBox)
                .SetForce(NeuroScope.Instance.ModHelper.Config.GetSettingsValue<int>("Dialogue Force Timer"), "Select a dialogue option.", "", false)
                .AddAction(new Actions.CharacterDialogueOptionAction(__instance, __result))
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

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerCameraEffectController), nameof(PlayerCameraEffectController.WakeUp))]
        public static void PlayerCameraEffectController_WakeUp_Postfix()
        {
            Utils.sendContext("Death", $"You woke up");
            NeuroActionHandler.RegisterActions(new GetPlayerStatusAction(), new GetShipStatusAction());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerCameraEffectController), nameof(PlayerCameraEffectController.OnPlayerDeath))]
        public static void PlayerCameraEffectController_OnPlayerDeath_Postfix(DeathType deathType)
        {
            Utils.sendContext("Death", $"You died{(deathType.Equals(DeathType.Default) ? "" : $" - Cause: {deathType}")}");

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
            }
        }
    }
}