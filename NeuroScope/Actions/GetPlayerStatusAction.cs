using System;
using System.Text;
using Cysharp.Threading.Tasks;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

public class GetPlayerStatusAction : NeuroAction
{
    public override string Name => "get_player_status";

    protected override string Description => "Returns the current player status.";

    protected override JsonSchema Schema => new();

    protected override ExecutionResult Validate(ActionJData actionData)
    {
        StringBuilder statusBuilder = new StringBuilder("[PLAYER STATUS]\n");
        statusBuilder.Append($"Wearing Suit: {PlayerState.IsWearingSuit()}\n");

        // Some information is only visible when the player is wearing a suit
        if (PlayerState.IsWearingSuit())
        {
            statusBuilder.Append($"Health: {Math.Round(Locator.GetPlayerBody().GetComponent<PlayerResources>().GetHealthFraction() * 100)}%\n");
            statusBuilder.Append($"Fuel: {Math.Round(Locator.GetPlayerBody().GetComponent<PlayerResources>().GetFuelFraction() * 100)}%\n");
            statusBuilder.Append($"Oxygen: {Math.Round(Locator.GetPlayerBody().GetComponent<PlayerResources>().GetOxygenFraction() * 100)}%\n");
        }
        statusBuilder.Append($"Inside Ship: {PlayerState.IsInsideShip()}\n");
        return ExecutionResult.Success(statusBuilder.ToString());
    }

    protected override async UniTask ExecuteAsync()
    {
        await UniTask.CompletedTask;
    }
}