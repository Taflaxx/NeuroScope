using System.Linq;
using HarmonyLib;
using NeuroSdk.Actions;

namespace NeuroScope
{

    [HarmonyPatch]
    public class Patches
    {

        [HarmonyPostfix, HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.DisplayDialogueBox2))]
        public static void CharacterDialogueTree_DisplayDialogueBox2_Postfix(CharacterDialogueTree __instance, DialogueBoxVer2 __result)
        {
            if (NeuroScope.Instance.ModHelper.Config.GetSettingsValue<string>("Dialogue").Equals("Disabled"))
            {
                NeuroScope.Instance.ModHelper.Console.WriteLine("Sending dialogue disabled");
                return;
            }

            // Make sure old CharacterDialogueOptionActions are removed
            NeuroActionHandler.UnregisterActions("dialogue_option");

            // If there are no DialogueOptions we can just send the text as Context
            if (!__result._displayedOptions.Any())
            {
                NeuroSdk.Messages.Outgoing.Context.Send(Utils.getText(__instance),
                    !NeuroScope.Instance.ModHelper.Config.GetSettingsValue<string>("Dialogue").Equals("Enabled"));
                return;
            }

            // If there are DialogueOptions, let Neuro choose an option
            ActionWindow.Create(__result._optionBox)
                .SetForce(0, "Select a dialogue option.", "", false)
                .AddAction(new Actions.CharacterDialogueOptionAction(__instance))
                .Register();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerCameraEffectController), nameof(PlayerCameraEffectController.OnPlayerDeath))]
        public static void PlayerCameraEffectController_OnPlayerDeath_Postfix(DeathType deathType)
        {
            if (NeuroScope.Instance.ModHelper.Config.GetSettingsValue<string>("Death").Equals("Disabled")) return;
            NeuroSdk.Messages.Outgoing.Context.Send($"You died{(deathType.Equals(DeathType.Default) ? "" : $" from {deathType}")}",
                !NeuroScope.Instance.ModHelper.Config.GetSettingsValue<string>("Death").Equals("Enabled"));
        }
    }
}