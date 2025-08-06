
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using UnityEngine;

public class SpinSurveyProbe : NeuroAction<SurveyorProbe>
{
    public override string Name => "spin_survey_probe";

    protected override string Description => "Spin the survey probe.";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.Object,
    };

    protected override ExecutionResult Validate(ActionJData actionData, out SurveyorProbe realSurveyorProbe)
    {
        realSurveyorProbe = Locator.GetProbe();
        if (realSurveyorProbe == null || !realSurveyorProbe.IsAnchored())
        {
            return ExecutionResult.Failure("Survey probe not found or not anchored. Please wait until the probe has landed after launching it before trying to spin it.");
        }
        return ExecutionResult.Success();
    }

    protected override async UniTask ExecuteAsync(SurveyorProbe surveyorProbe)
    {
        float rotationStep = 30f;
        float totalRotation = 360f;
        float duration = 2f;
        var rotatingCamera = surveyorProbe.GetRotatingCamera();

        int steps = Mathf.RoundToInt(totalRotation / rotationStep);
        float stepDuration = duration / steps;

        for (int i = 1; i <= steps; i++)
        {
            rotatingCamera.RotateHorizontal(rotationStep);
            NeuroScope.SurveyProbePatches.probeLauncher.TakeSnapshotWithCamera(rotatingCamera);
            await UniTask.Delay((int)(stepDuration * 1000));
        }
    }
}