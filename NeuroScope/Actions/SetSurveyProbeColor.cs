
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using UnityEngine;

public class SetSurveyProbeColor : NeuroAction
{
    public override string Name => "set_survey_probe_color";

    protected override string Description => "Set the color in hex format (#RRGGBB) and optional brightness from 0 to 5 (default: 1) of the survey probe lights.";

    protected override JsonSchema Schema => new()
        {
            Type = JsonSchemaType.Object,
            Required = new List<string> { "color" },
            Properties = new Dictionary<string, JsonSchema>
            {
                ["color"] = new JsonSchema { Type = JsonSchemaType.String },
                ["brightness"] = new JsonSchema { Type = JsonSchemaType.Integer }
            }
        };

    protected override ExecutionResult Validate(ActionJData actionData)
    {
        Color color;
        if (!ColorUtility.TryParseHtmlString(actionData.Data?["color"]?.ToString(), out color))
            return ExecutionResult.Failure("Invalid color format. Use hex format (e.g., #RRGGBB).");
        int intensity = actionData.Data?["brightness"]?.ToObject<int>() ?? 1;
        if (intensity < 0 || intensity > 5)
            return ExecutionResult.Failure("Brightness must be between 0 and 5.");

        NeuroScope.NeuroScope.surveyorProbeColor = color;
        NeuroScope.NeuroScope.surveyorProbeIntensity = intensity;
        return ExecutionResult.Success("Survey probe lights updated successfully.");
    }

    protected override async UniTask ExecuteAsync()
    {
        NeuroScope.Utils.updateSurveyProbeLights();
        await UniTask.CompletedTask;
    }
}