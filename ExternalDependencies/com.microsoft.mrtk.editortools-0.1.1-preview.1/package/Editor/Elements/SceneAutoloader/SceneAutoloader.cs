using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Microsoft.MixedReality.Toolkit.EditorTools
{
	[InitializeOnLoad]
	static class SceneAutoloader
	{
		private static readonly StringPreference MainScene = new StringPreference("UXTools.SceneAutoLoader.MainScene", "Assets/Scenes/Main.unity");
		private static readonly StringPreference PreviousScene = new StringPreference("UXTools.SceneAutoLoader.PreviousScene", SceneManager.GetActiveScene().path);
		private static readonly BoolPreference LoadMainOnPlay = new BoolPreference("UXTools.SceneAutoLoader.LoadMainOnPlay", false);

		static SceneAutoloader()
		{
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
		}

		[MenuItem("UX Tools/Scene Autoload/Select Main Scene...")]
		private static void SelectMainScene()
		{
			var masterScene = EditorUtility.OpenFilePanel("Select Main Scene", Application.dataPath, "unity");
			masterScene = masterScene.Replace(Application.dataPath, "Assets");

			if (!string.IsNullOrEmpty(masterScene))
			{
				MainScene.Value = masterScene;
				LoadMainOnPlay.Value = true;
			}
		}

		[MenuItem("UX Tools/Scene Autoload/Load Main On Play", true)]
		private static bool ShowLoadMasterOnPlay()
		{
			return !LoadMainOnPlay;
		}

		[MenuItem("UX Tools/Scene Autoload/Load Main On Play")]
		private static void EnableLoadMasterOnPlay()
		{
			LoadMainOnPlay.Value = true;
		}

		[MenuItem("UX Tools/Scene Autoload/Don't Load Main On Play", true)]
		private static bool ShowDontLoadMasterOnPlay()
		{
			return LoadMainOnPlay;
		}

		[MenuItem("UX Tools/Scene Autoload/Don't Load Main On Play")]
		private static void DisableLoadMasterOnPlay()
		{
			LoadMainOnPlay.Value = false;
		}

		private static void OnPlayModeChanged(PlayModeStateChange state)
		{
			if (LoadMainOnPlay)
			{
				if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
				{
					PreviousScene.Value = SceneManager.GetActiveScene().path;
					if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
					{
						try
						{
							EditorSceneManager.OpenScene(MainScene);
						}
						catch
						{
							Debug.LogError($"Error: scene not found: {MainScene.Value}");
							EditorApplication.isPlaying = false;

						}
					}
					else
					{
						EditorApplication.isPlaying = false;
					}
				}

				if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
				{
					try
					{
						EditorSceneManager.OpenScene(PreviousScene);
					}
					catch
					{
						Debug.LogError($"Error: scene not found: {PreviousScene.Value}");
					}
				}
			}
		}
	}
}