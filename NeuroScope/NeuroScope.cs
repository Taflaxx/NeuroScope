using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using OWML.Common;
using UnityEngine;

namespace NeuroScope;

public class NeuroScope : OWML.ModHelper.ModBehaviour
{
	public static NeuroScope Instance;

	public void Awake()
	{
		Instance = this;
	}

	public void Start()
	{
		ModHelper.Console.WriteLine($"{nameof(NeuroScope)} is loaded!", MessageType.Success);
		new Harmony("Taflax.NeuroScope").PatchAll(Assembly.GetExecutingAssembly());

		if (!ModHelper.Config.GetSettingsValue<string>("Websocket URL").Equals(""))
		{
			System.Environment.SetEnvironmentVariable("NEURO_SDK_WS_URL", ModHelper.Config.GetSettingsValue<string>("Websocket URL"));
		}
		if (System.Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL") == null)
		{
			ModHelper.Console.WriteLine("No Websocket URL set", MessageType.Error);
		}
		else
		{
			ModHelper.Console.WriteLine($"Websocket URL set to '{System.Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL")}'", MessageType.Info);
			NeuroSdk.NeuroSdkSetup.Initialize("Outer Wilds");
		}
		
	}
}

