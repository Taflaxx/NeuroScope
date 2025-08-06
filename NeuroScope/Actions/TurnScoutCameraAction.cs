
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using Newtonsoft.Json.Linq;

public class TurnScoutCameraAction : NeuroAction<ActionJData>
{
    public override string Name => "turn_scout_camera";

    protected override string Description => "Turn the scout camera in a specified direction. You can optionally specify the amount steps the camera should turn (1 step is 30 degrees, up to 12 steps at a time).";

    protected override JsonSchema Schema => new()
        {
            Type = JsonSchemaType.Object,
            Required = new List<string> { "direction" },
            Properties = new Dictionary<string, JsonSchema>
            {
                ["direction"] = QJS.Enum(new List<string> { "left", "right", "up", "down" }),
                ["steps"] = new JsonSchema { Type = JsonSchemaType.Integer },
            }
        };

    protected override ExecutionResult Validate(ActionJData actionData, out ActionJData actionDataOut)
    {
        actionDataOut = actionData;
        string direction = actionData.Data?["direction"]?.Value<string>();
        if (string.IsNullOrEmpty(direction) || !new List<string> { "left", "right", "up", "down" }.Contains(direction))
        {
            return ExecutionResult.Failure("Action failed. Invalid direction specified.");
        }
        int amount = actionData.Data?["steps"]?.Value<int>() ?? 1;
        if (amount < 1 || amount > 12)
        {
            return ExecutionResult.Failure("Action failed. Amount must be between 1 and 12.");
        }
        return ExecutionResult.Success();

    }

    protected override async UniTask ExecuteAsync(ActionJData actionData)
    {
        string direction = actionData.Data?["direction"]?.Value<string>();
        int amount = actionData.Data?["steps"]?.Value<int>() ?? 1;
        await NeuroScope.Utils.turnSurveyorProbe(NeuroScope.ScoutPatches.probeLauncher.GetActiveProbe(), direction, amount);
    }
}