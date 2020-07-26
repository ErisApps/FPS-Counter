﻿using System.Reflection;
using BeatSaberMarkupLanguage.Settings;
using CountersPlus.Custom;
using FPS_Counter.Settings;
using FPS_Counter.Settings.UI;
using FPS_Counter.Utilities;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace FPS_Counter
{
	[Plugin(RuntimeOptions.SingleStartInit)]
	public class Plugin
	{
		private static PluginMetadata _metadata;
		private static string? _name;
		
		private SettingsController? _settingsHost;

		public static string PluginName => _name ??= _metadata?.Name ?? Assembly.GetExecutingAssembly().GetName().Name;


		[Init]
		public void Init(IPALogger logger, PluginMetadata metaData, Config config)
		{
			_metadata = metaData;
			Logger.Log = logger;

			Configuration.Instance = config.Generated<Configuration>();
		}

		[OnStart]
		public void OnStart()
		{
			Logger.Log.Info("Checking for Counters+");
			
			PluginUtils.CountersPlusStateChanged += OnCountersPlusStateChanged;
			PluginUtils.Setup();

			BS_Utils.Utilities.BSEvents.gameSceneLoaded += OnGameSceneLoaded;
			
			BSMLSettings.instance.AddSettingsMenu(PluginName, "FPS_Counter.Settings.UI.Views.mainsettings.bsml", _settingsHost ??= new SettingsController());
		}

		[OnExit]
		public void OnExit()
		{
			PluginUtils.CountersPlusStateChanged -= OnCountersPlusStateChanged;
			PluginUtils.Cleanup();

			BS_Utils.Utilities.BSEvents.gameSceneLoaded -= OnGameSceneLoaded;

			BSMLSettings.instance.RemoveSettingsMenu(_settingsHost);
			_settingsHost = null;
		}
		
		private static void OnCountersPlusStateChanged(object _, bool enabled)
		{
			Logger.Log.Info($"Counters+ state changed. Enabled: {enabled}");
			if (enabled)
			{
				AddCustomCounter();
			}
		}

		private static void OnGameSceneLoaded()
		{
			new GameObject("FPS Counter").AddComponent<Behaviours.FPSCounter>();
		}

		private static void AddCustomCounter()
		{
			Logger.Log.Info("Creating Custom Counter");

			CustomCounter counter = new CustomCounter
			{
				SectionName = "fpsCounter",
				Name = PluginName,
				BSIPAMod = _metadata,
				Counter = PluginName,
				Icon_ResourceName = "FPS_Counter.Resources.icon.png"
			};

			CustomCounterCreator.Create(counter);
		}
	}
}