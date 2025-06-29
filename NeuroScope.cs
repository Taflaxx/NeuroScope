using HarmonyLib;
using NeuroSdk.Actions;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace NeuroScope;

public class NeuroScope : ModBehaviour
{
	public static NeuroScope Instance;

	public void Awake()
	{
		Instance = this;
		// You won't be able to access OWML's mod helper in Awake.
		// So you probably don't want to do anything here.
		// Use Start() instead.
	}

	public void Start()
	{
		// Starting here, you'll have access to OWML's mod helper.
		ModHelper.Console.WriteLine($"My mod {nameof(NeuroScope)} is loaded!", MessageType.Success);

		new Harmony("Taflax.NeuroScope").PatchAll(Assembly.GetExecutingAssembly());

		// Example of accessing game code.
		OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen); // We start on title screen
		LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;

		System.Environment.SetEnvironmentVariable("NEURO_SDK_WS_URL", "ws://localhost:8000");
		NeuroSdk.NeuroSdkSetup.Initialize("Outer Wilds");
		NeuroActionHandler.RegisterActions(new GetPlayerStatusAction());
	}

	public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
	{
		if (newScene != OWScene.SolarSystem) return;
		ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
	}
}

