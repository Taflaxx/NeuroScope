
using Cysharp.Threading.Tasks;
using NeuroScope;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

public class SpinScoutAction : NeuroAction<SurveyorProbe>
{
    public override string Name => "spin_scout";

    protected override string Description => "Spin the scout.";

    protected override JsonSchema Schema => new()
    {
        Type = JsonSchemaType.Object,
    };

    protected override ExecutionResult Validate(ActionJData actionData, out SurveyorProbe realSurveyorProbe)
    {
        realSurveyorProbe = Locator.GetProbe();
        if (realSurveyorProbe == null || !realSurveyorProbe.IsAnchored())
        {
            return ExecutionResult.Failure("Scout not found or not anchored. Please wait until the probe has landed after launching it before trying to spin it.");
        }
        return ExecutionResult.Success();
    }

    protected override async UniTask ExecuteAsync(SurveyorProbe surveyorProbe)
    {
        await Utils.turnSurveyorProbe(surveyorProbe, "right", 12);
    }
}