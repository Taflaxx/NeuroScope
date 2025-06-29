using System;
using System.Text;
using Cysharp.Threading.Tasks;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

public class GetShipStatusAction : NeuroAction
{
    public override string Name => "get_ship_status";

    protected override string Description => "Returns the current spaceship status.";

    protected override JsonSchema Schema => new();

    protected override ExecutionResult Validate(ActionJData actionData)
    {
        if (Locator.GetShipBody() == null) return ExecutionResult.Failure("No ship found.");

        StringBuilder statusBuilder = new StringBuilder("[SHIP STATUS]\n");
        statusBuilder.Append($"Player inside ship: {PlayerState.IsInsideShip()}\n");
        foreach (var hull in Locator.GetShipBody().GetComponent<ShipDamageController>()._shipHulls)
        {
            statusBuilder.Append($"{UITextLibrary.GetString(hull.hullName)}: {Math.Round(hull._integrity * 100)}%\n");
        }

        foreach (var component in Locator.GetShipBody().GetComponent<ShipDamageController>()._shipComponents)
        {
            statusBuilder.Append($"{UITextLibrary.GetString(component.componentName)}: {(component._damaged ? "DAMAGED" : "HEALTHY")}\n");
        }
        return ExecutionResult.Success(statusBuilder.ToString());
    }

    protected override async UniTask ExecuteAsync()
    {
        await UniTask.CompletedTask;
    }
}