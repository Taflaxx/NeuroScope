
using Cysharp.Threading.Tasks;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

public class TakeScoutPhotoAction : NeuroAction<SurveyorProbe>
{
    public override string Name => "take_scout_photo";

    protected override string Description => "Take a photo with the scout launched from the scout launcher.";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.Object,
    };

    protected override ExecutionResult Validate(ActionJData actionData, out SurveyorProbe realSurveyorProbe)
    {
        realSurveyorProbe = null;
        if (NeuroScope.ScoutPatches.probeLauncher == null)
        {
            return ExecutionResult.Failure("Scout launcher is not equipped.");
        }

        realSurveyorProbe = NeuroScope.ScoutPatches.probeLauncher.GetActiveProbe();
        if (realSurveyorProbe == null || !realSurveyorProbe.IsAnchored())
        {
            return ExecutionResult.Failure("Scout not found or not anchored. Please wait until the scout has landed after launching it before trying to spin it.");
        }
        return ExecutionResult.Success();
    }

    protected override async UniTask ExecuteAsync(SurveyorProbe surveyorProbe)
    {
        NeuroScope.ScoutPatches.probeLauncher.TakeSnapshotWithCamera(surveyorProbe.GetRotatingCamera());
        await UniTask.CompletedTask;
    }
}