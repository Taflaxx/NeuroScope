
using Cysharp.Threading.Tasks;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

public class LauchScoutAction : NeuroAction
{
    public override string Name => "launch_scout";

    protected override string Description => "Use the scout launcher to launch a scout.";

    protected override JsonSchema Schema => new()
        {
            Type = JsonSchemaType.Object,
        };

    protected override ExecutionResult Validate(ActionJData actionData)
    {
        if (NeuroScope.ScoutPatches.probeLauncher == null) return ExecutionResult.Failure("Scout launcher is not equipped.");
        return ExecutionResult.Success("Scout launched.");
    }

    protected override async UniTask ExecuteAsync()
    {
        NeuroScope.ScoutPatches.probeLauncher.LaunchProbe();
        NeuroActionHandler.RegisterActions(new TakeScoutPhotoAction(), new RetrieveScoutAction(), new SpinScoutAction(), new TurnScoutCameraAction());
        await UniTask.CompletedTask;
    }
}