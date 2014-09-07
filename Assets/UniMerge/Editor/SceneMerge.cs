//Matt Schoen
//5-29-2013
//
// This software is the copyrighted material of its author, Matt Schoen, and his company Defective Studios.
// It is available for sale on the Unity Asset store and is subject to their restrictions and limitations, as well as
// the following: You shall not reproduce or re-distribute this software without the express written (e-mail is fine)
// permission of the author. If permission is granted, the code (this file and related files) must bear this license 
// in its entirety. Anyone who purchases the script is welcome to modify and re-use the code at their personal risk 
// and under the condition that it not be included in any distribution builds. The software is provided as-is without 
// warranty and the author bears no responsibility for damages or losses caused by the software.  
// This Agreement becomes effective from the day you have installed, copied, accessed, downloaded and/or otherwise used
// the software.

#define DEV			//Comment this out to not auto-populate scene merge

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SceneMerge : EditorWindow {
	const string messagePath = "Assets/merges.txt";
	public static Object mine, theirs;
	private static bool merged;
	//If these names end up conflicting with names within your scene, change them here
	public const string mineContainerName = "mine", theirsContainerName = "theirs";
	public static GameObject mineContainer, theirsContainer;

	public static float colWidth;

	static SceneData mySceneData, theirSceneData;
	Vector2 scroll;

	[MenuItem("Window/UniMerge/Scene Merge %&m")]
	static void Init() {
		GetWindow(typeof(SceneMerge));
	}
#if DEV
	//If these names end up conflicting with names within your project, change them here
	public const string mineSceneName = "Mine", theirsSceneName = "Theirs";
	void OnEnable() {
	//Get path
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		//Unity 3 path stuff?
#else
		string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
		UniMergeConfig.DEFAULT_PATH = scriptPath.Substring(0, scriptPath.IndexOf("Editor") - 1);
#endif
		if(Directory.Exists(UniMergeConfig.DEFAULT_PATH + "/Demo/Scene Merge")){
			string[] assets = Directory.GetFiles(UniMergeConfig.DEFAULT_PATH + "/Demo/Scene Merge");
			foreach(var asset in assets) {
				if(asset.EndsWith(".unity")) {
					if(asset.Contains(mineSceneName)) {
						mine = AssetDatabase.LoadAssetAtPath(asset.Replace('\\', '/'), typeof(Object));
					}
					if(asset.Contains(theirsSceneName)) {
						theirs = AssetDatabase.LoadAssetAtPath(asset.Replace('\\', '/'), typeof(Object));
					}
				}
			}
		}
	}
#endif

	void OnGUI(){
#if  UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
		//Layout fix for older versions?
#else
	    EditorGUIUtility.labelWidth = 100;
#endif
		//Ctrl + w to close
		if(Event.current.Equals(Event.KeyboardEvent("^w"))) {
			Close();
			GUIUtility.ExitGUI();
		}
		/*
		 * SETUP
		 */
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		EditorGUIUtility.LookLikeControls();
#endif
		ObjectMerge.alt = false;
		//Adjust colWidth as the window resizes
		colWidth = (position.width - UniMergeConfig.midWidth*2 - UniMergeConfig.margin)/2;
		if(mine == null || theirs == null
			|| mine.GetType() != typeof(Object) || mine.GetType() != typeof(Object)
			) //|| !AssetDatabase.GetAssetPath(mine).Contains(".unity") || !AssetDatabase.GetAssetPath(theirs).Contains(".unity"))
			merged = GUI.enabled = false;
		if(GUILayout.Button("Merge")) {
			Merge(mine, theirs);
			GUIUtility.ExitGUI();
		}
		GUI.enabled = merged;
		GUILayout.BeginHorizontal();
		{
			GUI.enabled = mineContainer;
            if (!GUI.enabled)
                merged = false;
			if(GUILayout.Button("Unpack Mine")) {
				DestroyImmediate(theirsContainer);
				List<Transform> tmp = new List<Transform>();
				foreach(Transform t in mineContainer.transform)
					tmp.Add(t);
				foreach(Transform t in tmp)
					t.parent = null;
				DestroyImmediate(mineContainer);
				mySceneData.ApplySettings();
			}
			GUI.enabled = theirsContainer;
            if (!GUI.enabled)
                merged = false;
			if(GUILayout.Button("Unpack Theirs")) {
				DestroyImmediate(mineContainer);
				List<Transform> tmp = new List<Transform>();
				foreach(Transform t in theirsContainer.transform)
					tmp.Add(t);
				foreach(Transform t in tmp)
					t.parent = null;
				DestroyImmediate(theirsContainer);
				theirSceneData.ApplySettings();
			}
		}
		GUILayout.EndHorizontal();

        GUI.enabled = true;
		ObjectMerge.DrawRowHeight();

		GUILayout.BeginHorizontal();
		{
			GUILayout.BeginVertical(GUILayout.Width(colWidth));
			{
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				mine = EditorGUILayout.ObjectField("Mine", mine, typeof(Object));
#else
				mine = EditorGUILayout.ObjectField("Mine", mine, typeof(Object), true);
#endif
			}
			GUILayout.EndVertical();
			GUILayout.Space(UniMergeConfig.midWidth*2);
			GUILayout.BeginVertical(GUILayout.Width(colWidth));
			{
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				theirs = EditorGUILayout.ObjectField("Theirs", theirs, typeof(Object));
#else
				theirs = EditorGUILayout.ObjectField("Theirs", theirs, typeof(Object), true);
#endif
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
		if(mine == null || theirs == null)
			merged = false;
		if(merged) {
			scroll = GUILayout.BeginScrollView(scroll);
			//Fog
			ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
				indent = 0,
				colWidth = colWidth,
				compare = delegate {
					bool same = GenericCompare();
					if(same)
						same = mySceneData.fog == theirSceneData.fog;
					return same;
				},
				left = delegate {
					if(mine)
						mySceneData.fog = EditorGUILayout.Toggle("Fog", mySceneData.fog);
				},
				leftButton = delegate {
					mySceneData.fog = theirSceneData.fog;
				},
				rightButton = delegate {
					theirSceneData.fog = mySceneData.fog;
				},
				right = delegate {
					if(theirs)
						theirSceneData.fog = EditorGUILayout.Toggle("Fog", theirSceneData.fog);
				},
				drawButtons = mine && theirs
			});
			//Fog Color
			ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
				indent = 0,
				colWidth = colWidth,
				compare = delegate {
					bool same = GenericCompare();
					if(same)
						same = mySceneData.fogColor == theirSceneData.fogColor;
					return same;
				},
				left = delegate {
					if(mine)
						mySceneData.fogColor = EditorGUILayout.ColorField("Fog Color", mySceneData.fogColor);
				},
				leftButton = delegate {
					mySceneData.fogColor = theirSceneData.fogColor;
				},
				rightButton = delegate {
					theirSceneData.fogColor = mySceneData.fogColor;
				},
				right = delegate {
					if(theirs)
						theirSceneData.fogColor = EditorGUILayout.ColorField("Fog Color", theirSceneData.fogColor);
				},
				drawButtons = mine && theirs
			});
			//Fog Mode
			ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
				indent = 0,
				colWidth = colWidth,
				compare = delegate {
					bool same = GenericCompare();
					if(same)
						same = mySceneData.fogMode == theirSceneData.fogMode;
					return same;
				},
				left = delegate {
					if(mine)
						mySceneData.fogMode = (FogMode)EditorGUILayout.EnumPopup("Fog Mode", mySceneData.fogMode);
				},
				leftButton = delegate {
					mySceneData.fogMode = theirSceneData.fogMode;
				},
				rightButton = delegate {
					theirSceneData.fogMode = mySceneData.fogMode;
				},
				right = delegate {
					if(theirs)
						theirSceneData.fogMode = (FogMode)EditorGUILayout.EnumPopup("Fog Mode", theirSceneData.fogMode);
				},
				drawButtons = mine && theirs
			});
			//Fog Density
			ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
				indent = 0,
				colWidth = colWidth,
				compare = delegate {
					bool same = GenericCompare();
					if(same)
                        same = mySceneData.fogDensity == theirSceneData.fogDensity;
					return same;
				},
				left = delegate {
					if(mine)
						mySceneData.fogDensity = EditorGUILayout.FloatField("Linear Density", mySceneData.fogDensity);
				},
				leftButton = delegate {
					mySceneData.fogDensity = theirSceneData.fogDensity;
				},
				rightButton = delegate {
					theirSceneData.fogDensity = mySceneData.fogDensity;
				},
				right = delegate {
					if(theirs)
						theirSceneData.fogDensity = EditorGUILayout.FloatField("Linear Density", theirSceneData.fogDensity);
				},
				drawButtons = mine && theirs
			});
			//Linear Fog Start
			ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
				indent = 0,
				colWidth = colWidth,
				compare = delegate {
					bool same = GenericCompare();
					if(same)
						same = mySceneData.fogStartDistance == theirSceneData.fogStartDistance;
					return same;
				},
				left = delegate {
					if(mine)
						mySceneData.fogStartDistance = EditorGUILayout.FloatField("Linear Fog Start", mySceneData.fogStartDistance);
				},
				leftButton = delegate {
					mySceneData.fogStartDistance = theirSceneData.fogStartDistance;
				},
				rightButton = delegate {
					theirSceneData.fogStartDistance = mySceneData.fogStartDistance;
				},
				right = delegate {
					if(theirs)
						theirSceneData.fogStartDistance = EditorGUILayout.FloatField("Linear Fog Start", theirSceneData.fogStartDistance);
				},
				drawButtons = mine && theirs
			});
			//Linear Fog End
			ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
				indent = 0,
				colWidth = colWidth,
				compare = delegate {
					bool same = GenericCompare();
					if(same)
						same = mySceneData.fogEndDistance == theirSceneData.fogEndDistance;
					return same;
				},
				left = delegate {
					if(mine)
						mySceneData.fogEndDistance = EditorGUILayout.FloatField("Linear Fog End", mySceneData.fogEndDistance);
				},
				leftButton = delegate {
					mySceneData.fogEndDistance = theirSceneData.fogEndDistance;
				},
				rightButton = delegate {
					theirSceneData.fogEndDistance = mySceneData.fogEndDistance;
				},
				right = delegate {
					if(theirs)
						theirSceneData.fogEndDistance = EditorGUILayout.FloatField("Linear Fog End", theirSceneData.fogEndDistance);
				},
				drawButtons = mine && theirs
			});
			//Ambient Light
			ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
				indent = 0,
				colWidth = colWidth,
				compare = delegate {
					bool same = GenericCompare();
					if(same)
						same = mySceneData.ambientLight == theirSceneData.ambientLight;
					return same;
				},
				left = delegate {
					if(mine)
						mySceneData.ambientLight = EditorGUILayout.ColorField("Ambient Light", mySceneData.ambientLight);
				},
				leftButton = delegate {
					mySceneData.ambientLight = theirSceneData.ambientLight;
				},
				rightButton = delegate {
					theirSceneData.ambientLight = mySceneData.ambientLight;
				},
				right = delegate {
					if(theirs)
						theirSceneData.ambientLight = EditorGUILayout.ColorField("Ambient Light", theirSceneData.ambientLight);
				},
				drawButtons = mine && theirs
			});
			//Skybox
			ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
				indent = 0,
				colWidth = colWidth,
				compare = delegate {
					bool same = GenericCompare();
					if(same)
						same = mySceneData.skybox == theirSceneData.skybox;
					return same;
				},
				left = delegate {
					if(mine)
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
						mySceneData.skybox = (Material)EditorGUILayout.ObjectField("Skybox Material", mySceneData.skybox, typeof(Material));
#else
						mySceneData.skybox = (Material)EditorGUILayout.ObjectField("Skybox Material", mySceneData.skybox, typeof(Material), false);
#endif
				},
				leftButton = delegate {
					mySceneData.skybox = theirSceneData.skybox;
				},
				rightButton = delegate {
					theirSceneData.skybox = mySceneData.skybox;
				},
				right = delegate {
					if(theirs)
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
					theirSceneData.skybox = (Material)EditorGUILayout.ObjectField("Skybox Material", theirSceneData.skybox, typeof(Material));
#else
						theirSceneData.skybox = (Material)EditorGUILayout.ObjectField("Skybox Material", theirSceneData.skybox, typeof(Material), false);
#endif
				},
				drawButtons = mine && theirs
			});
			//Halo Strength
			ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
				indent = 0,
				colWidth = colWidth,
				compare = delegate {
					bool same = GenericCompare();
					if(same)
						same = mySceneData.haloStrength == theirSceneData.haloStrength;
					return same;
				},
				left = delegate {
					if(mine)
						mySceneData.haloStrength = EditorGUILayout.FloatField("Halo Strength", mySceneData.haloStrength);
				},
				leftButton = delegate {
					mySceneData.haloStrength = theirSceneData.haloStrength;
				},
				rightButton = delegate {
					theirSceneData.haloStrength = mySceneData.haloStrength;
				},
				right = delegate {
					if(theirs)
						theirSceneData.haloStrength = EditorGUILayout.FloatField("Halo Strength", theirSceneData.haloStrength);
				},
				drawButtons = mine && theirs
			});
			//Flare Strength
			ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
				indent = 0,
				colWidth = colWidth,
				compare = delegate {
					bool same = GenericCompare();
					if(same)
						same = mySceneData.flareStrength == theirSceneData.flareStrength;
					return same;
				},
				left = delegate {
					if(mine)
						mySceneData.flareStrength = EditorGUILayout.FloatField("Flare Strength", mySceneData.flareStrength);
				},
				leftButton = delegate {
					mySceneData.flareStrength = theirSceneData.flareStrength;
				},
				rightButton = delegate {
					theirSceneData.flareStrength = mySceneData.flareStrength;
				},
				right = delegate {
					if(theirs)
						theirSceneData.flareStrength = EditorGUILayout.FloatField("Flare Strength", theirSceneData.flareStrength);
				},
				drawButtons = mine && theirs
			});
			//Flare Fade Speed
			ObjectMerge.DrawGenericRow(new ObjectMerge.GenericRowArguments {
				indent = 0,
				colWidth = colWidth,
				compare = delegate {
					bool same = GenericCompare();
					if(same)
						same = mySceneData.flareFadeSpeed == theirSceneData.flareFadeSpeed;
					return same;
				},
				left = delegate {
					if(mine)
						mySceneData.flareFadeSpeed = EditorGUILayout.FloatField("Flare Fade Speed", mySceneData.flareFadeSpeed);
				},
				leftButton = delegate {
					mySceneData.flareFadeSpeed = theirSceneData.flareFadeSpeed;
				},
				rightButton = delegate {
					theirSceneData.flareFadeSpeed = mySceneData.flareFadeSpeed;
				},
				right = delegate {
					if(theirs)
						theirSceneData.flareFadeSpeed = EditorGUILayout.FloatField("Flare Fade Speed", theirSceneData.flareFadeSpeed);
				},
				drawButtons = mine && theirs
			});
			GUILayout.EndScrollView();
		}
	}

	static bool GenericCompare() {
		bool same = mine;
		if(same)
			same = theirs;
		return same;
	}
	public static void CLIIn() {
		string[] args = System.Environment.GetCommandLineArgs();
		foreach(string arg in args)
			Debug.Log(arg);
		Merge(args[args.Length - 2], args[args.Length - 1]);
	}
	void Update() {
		TextAsset mergeFile = (TextAsset)AssetDatabase.LoadAssetAtPath(messagePath, typeof(TextAsset));
		if(mergeFile) {
			string[] files = mergeFile.text.Split('\n');
			AssetDatabase.DeleteAsset(messagePath);
			foreach(string file in files)
				Debug.Log(file);
			//TODO: Add prefab case
			DoMerge(files);
		}
	}
	public static void DoMerge(string[] paths) {
		if(paths.Length > 2) {
			Merge(paths[0], paths[1]);
		} else Debug.LogError("need at least 2 paths, " + paths.Length + " given");
	}
	public static void Merge(Object myScene, Object theirScene) {
		if(myScene == null || theirScene == null)
			return;
		Merge(AssetDatabase.GetAssetPath(myScene), AssetDatabase.GetAssetPath(theirScene));
	}
	public static void Merge(string myPath, string theirPath) {
		if(string.IsNullOrEmpty(myPath) || string.IsNullOrEmpty(theirPath))
			return;
		if(AssetDatabase.LoadAssetAtPath(myPath, typeof(Object)) && AssetDatabase.LoadAssetAtPath(theirPath, typeof(Object))) {
			if(EditorApplication.SaveCurrentSceneIfUserWantsTo()) {
				//Load "theirs" to get RenderSettings, etc.
				EditorApplication.OpenScene(theirPath);
				theirSceneData.ambientLight = RenderSettings.ambientLight;
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#else				
				theirSceneData.flareFadeSpeed = RenderSettings.flareFadeSpeed;
#endif
				theirSceneData.flareStrength = RenderSettings.flareStrength;
				theirSceneData.fog = RenderSettings.fog;
				theirSceneData.fogColor = RenderSettings.fogColor;
				theirSceneData.fogDensity = RenderSettings.fogDensity;
				theirSceneData.fogEndDistance = RenderSettings.fogEndDistance;
				theirSceneData.fogMode = RenderSettings.fogMode;
				theirSceneData.fogStartDistance = RenderSettings.fogStartDistance;
				theirSceneData.haloStrength = RenderSettings.haloStrength;
				theirSceneData.skybox = RenderSettings.skybox;
				//Load "mine" to start the merge
				EditorApplication.OpenScene(myPath);
				mySceneData.ambientLight = RenderSettings.ambientLight;
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#else
				mySceneData.flareFadeSpeed = RenderSettings.flareFadeSpeed;
#endif
				mySceneData.flareStrength = RenderSettings.flareStrength;
				mySceneData.fog = RenderSettings.fog;
				mySceneData.fogColor = RenderSettings.fogColor;
				mySceneData.fogDensity = RenderSettings.fogDensity;
				mySceneData.fogEndDistance = RenderSettings.fogEndDistance;
				mySceneData.fogMode = RenderSettings.fogMode;
				mySceneData.fogStartDistance = RenderSettings.fogStartDistance;
				mySceneData.haloStrength = RenderSettings.haloStrength;
				mySceneData.skybox = RenderSettings.skybox;

				mineContainer = new GameObject {name = mineContainerName};
				GameObject[] allObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				foreach(GameObject obj in allObjects) {
					if(obj.transform.parent == null
						&& EditorUtility.GetPrefabType(obj) != PrefabType.Prefab
						&& EditorUtility.GetPrefabType(obj) != PrefabType.ModelPrefab
						&& obj.hideFlags == 0)		//Want a better way to filter out "internal" objects
						obj.transform.parent = mineContainer.transform;
				}
#else
				foreach(GameObject obj in allObjects) {
					if(obj.transform.parent == null
						&& PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab
						&& PrefabUtility.GetPrefabType(obj) != PrefabType.ModelPrefab
						&& obj.hideFlags == 0)		//Want a better way to filter out "internal" objects
						obj.transform.parent = mineContainer.transform;
				}
#endif
				EditorApplication.OpenSceneAdditive(theirPath);
				theirsContainer = new GameObject {name = theirsContainerName};
				allObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				foreach(GameObject obj in allObjects) {
					if(obj.transform.parent == null && obj.name != mineContainerName
						&& EditorUtility.GetPrefabType(obj) != PrefabType.Prefab
						&& EditorUtility.GetPrefabType(obj) != PrefabType.ModelPrefab
						&& obj.hideFlags == 0)		//Want a better way to filter out "internal" objects
						obj.transform.parent = theirsContainer.transform;
				}
#else
				foreach(GameObject obj in allObjects) {
					if(obj.transform.parent == null && obj.name != mineContainerName
						&& PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab
						&& PrefabUtility.GetPrefabType(obj) != PrefabType.ModelPrefab
						&& obj.hideFlags == 0)		//Want a better way to filter out "internal" objects
						obj.transform.parent = theirsContainer.transform;
				}
#endif

				GetWindow(typeof(ObjectMerge));
				ObjectMerge.mine = mineContainer;
				ObjectMerge.theirs = theirsContainer;
				merged = true;
			}
		}
	}
}
public struct SceneData {
	public Color	ambientLight, 
					fogColor;

	public float	flareFadeSpeed,
					flareStrength,
					fogDensity,
					fogEndDistance,
					fogStartDistance,
					haloStrength;

	public bool		fog;

	public FogMode	fogMode;

	public Material skybox;

	//Hmm... can't get at haloTexture or spotCookie
	//public Texture haloTexture, spotCookie;

	public void ApplySettings() {
		RenderSettings.ambientLight = ambientLight;
		RenderSettings.fogColor = fogColor;
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#else
		RenderSettings.flareFadeSpeed = flareFadeSpeed;
#endif
		RenderSettings.flareStrength = flareStrength;
		RenderSettings.fogDensity = fogDensity;
		RenderSettings.fogEndDistance = fogEndDistance;
		RenderSettings.fogStartDistance = fogStartDistance;
		RenderSettings.haloStrength = haloStrength;
		RenderSettings.fog = fog;
		RenderSettings.fogMode = fogMode;
		RenderSettings.skybox = skybox;
	}
}