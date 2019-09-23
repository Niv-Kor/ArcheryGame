
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;

namespace Technie.PhysicsCreator
{
	public enum ToolSelection
	{
		TrianglePainting,
		Pipette
	}

	public enum PaintingBrush
	{
		Precise,
		Small,
		Medium,
		Large
	}

	[Flags]
	public enum Collumn
	{
		None		= 0,
		Visibility	= 1 << 1,
		Name		= 1 << 2,
		Colour		= 1 << 3,
		Type		= 1 << 4,
		Material	= 1 << 5,
		Trigger		= 1 << 6,
		Paint		= 1 << 7,
		Delete		= 1 << 8,
		All			= ~0
	}


	public class HullPainterWindow : EditorWindow
	{
		// The actual install path with be detected at runtime with FindInstallPath
		// If for some reason that fails, the default install path will be used instead
		public const string defaultInstallPath = "Assets/Technie/PhysicsCreator/";

		private static bool isOpen;
		public static bool IsOpen() { return isOpen; }
		public static HullPainterWindow instance;

		private int activeMouseButton = -1;

		private bool repaintSceneView = false;
		private bool regenerateOverlay = false;
		private int hullToDelete = -1;

		private SceneManipulator sceneManipulator;

		// Foldout visibility
		private bool areToolsFoldedOut = true;
		private bool areHullsFoldedOut = true;
		private bool areSettingsFoldedOut = true;
		private bool areAssetsFoldedOut = true;

		private static Collumn visibleCollumns = Collumn.All;

		private Vector2 scrollPosition;

		private Texture addHullIcon;
		private Texture errorIcon;
		private Texture deleteIcon;
		private Texture paintOnIcon;
		private Texture paintOffIcon;
		private Texture triggerOnIcon;
		private Texture triggerOffIcon;
		private Texture preciseBrushIcon;
		private Texture smallBrushIcon;
		private Texture mediumBrushIcon;
		private Texture largeBrushIcon;
		private Texture pipetteIcon;
		private Texture hullVisibleIcon;
		private Texture hullInvisibleIcon;
		private Texture toolsIcons;
		private Texture hullsIcon;
		private Texture settingsIcon;
		private Texture assetsIcon;

		private HullType defaultType = HullType.ConvexHull;
		private PhysicMaterial defaultMaterial;
		private bool defaultIsTrigger;

		private GUIStyle foldoutStyle;
		private Color dividerColour;

		[MenuItem("Window/Technie Collider Creator/Hull Painter", false, 1)]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(HullPainterWindow));
		}

		void OnEnable()
		{
			string installPath = FindInstallPath();

			dividerColour = new Color(116.0f / 255.0f, 116.0f / 255.0f, 116.0f / 255.0f);

			addHullIcon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "AddHullIcon.png");
			errorIcon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "ErrorIcon.png");
			deleteIcon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "DeleteIcon.png");

			paintOnIcon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "PaintOnIcon.png");
			paintOffIcon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "PaintOffIcon.png");

			triggerOnIcon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "TriggerOnIcon.png");
			triggerOffIcon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "TriggerOffIcon.png");

			preciseBrushIcon = AssetDatabase.LoadAssetAtPath<Texture>(installPath + "PreciseBrushIcon.png");
			smallBrushIcon = AssetDatabase.LoadAssetAtPath<Texture>(installPath + "SmallBrushIcon.png");
			mediumBrushIcon = AssetDatabase.LoadAssetAtPath<Texture>(installPath + "MediumBrushIcon.png");
			largeBrushIcon = AssetDatabase.LoadAssetAtPath<Texture>(installPath + "LargeBrushIcon.png");

			pipetteIcon = AssetDatabase.LoadAssetAtPath<Texture>(installPath + "PipetteIcon.png");

			hullVisibleIcon = AssetDatabase.LoadAssetAtPath<Texture>(installPath + "HullVisibleIcon.png");
			hullInvisibleIcon = AssetDatabase.LoadAssetAtPath<Texture>(installPath + "HullInvisibleIcon.png");

			toolsIcons = AssetDatabase.LoadAssetAtPath<Texture>(installPath + "ToolsIcon.png");
			hullsIcon = AssetDatabase.LoadAssetAtPath<Texture>(installPath + "HullIcon.png");
			settingsIcon = AssetDatabase.LoadAssetAtPath<Texture>(installPath + "SettingsIcon.png");
			assetsIcon = AssetDatabase.LoadAssetAtPath<Texture>(installPath + "AssetsIcon.png");

			Texture icon = AssetDatabase.LoadAssetAtPath<Texture> (installPath + "TechnieIcon.png");
#if UNITY_5_0
			this.title = "Hull Painter";
#else
			this.titleContent = new GUIContent ("Hull Painter", icon, "Technie Hull Painter");
#endif

			sceneManipulator = new SceneManipulator();

			isOpen = true;
			instance = this;
		}

		void OnDestroy()
		{
#if UNITY_2019_3_OR_NEWER
			SceneView.duringSceneGui -= this.OnSceneGUI;
#else
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
#endif
			
			if (sceneManipulator != null)
			{
				sceneManipulator.Destroy();
				sceneManipulator = null;
			}

			isOpen = false;
			instance = null;
		}

		void OnFocus()
		{
			// Remove to make sure it's not added, then add it once
#if UNITY_2019_3_OR_NEWER
			SceneView.duringSceneGui -= this.OnSceneGUI;
			SceneView.duringSceneGui += this.OnSceneGUI;
#else
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#endif
		}

		void OnSelectionChange()
		{
		//	Debug.Log ("Window.OnSelectionChange");

			if (sceneManipulator.Sync ())
			{
		//		Debug.Log ("Changed");
			}

			// Always repaint as we need to change inactive gui
			Repaint();
		}

		// Called from HullPainterEditor
		public void OnInspectorGUI()
		{
			if (sceneManipulator.Sync ())
			{
				Repaint();
			}
		}

		private void CreateStyles()
		{
			// Creating styles in OnEnable can throw NPEs if the window is docked
			// Instead it's more reliable to lazily init them just before we need them

			if (foldoutStyle == null)
			{
				foldoutStyle = new GUIStyle(EditorStyles.foldout);
				foldoutStyle.fontStyle = FontStyle.Bold;
			}
		}

		void OnGUI ()
		{
			// Only sync on layout so ui gets same calls
			if (Event.current.type == EventType.Layout)
			{
				sceneManipulator.Sync ();
			}

			CreateStyles();

			repaintSceneView = false;
			regenerateOverlay = false;
			hullToDelete = -1;

			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			if (currentHullPainter != null && currentHullPainter.paintingData != null)
			{
				DrawActiveGui(currentHullPainter);
			}
			else
			{
				DrawInactiveGui();
			}

			// HACK HACK
		//	if (GUILayout.Button("API Test"))
		//	{
		//		Vhacd2Api.Init ();
		//
		//		int ret = Vhacd2Api.TestExportFunc (1, 2.0f);
		//		Debug.Log ("Ret: " + ret);
		//	}
		}

		/** Gui drawn if the selected object has a vaild hull painter and initialised asset data
		 */
		private void DrawActiveGui(HullPainter currentHullPainter)
		{
			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			GUILayout.Space (10);
			
			DrawToolGui ();
			
			DrawHullGUI();

			DrawSettingsGui();
			
			DrawHullWarnings (currentHullPainter);

			DrawAssetGui();
			
			if (currentHullPainter.paintingData.hulls.Count == 0)
			{
				GUILayout.Label("No hulls created. Add a hull to start painting.");
			}

			GUILayout.Space (16);

			GUILayout.EndScrollView ();

			// Now actually perform queued up actions

			if (hullToDelete != -1)
			{
				Undo.RecordObject (currentHullPainter.paintingData, "Delete Hull");

				currentHullPainter.paintingData.RemoveHull (hullToDelete);

				EditorUtility.SetDirty (currentHullPainter.paintingData);
			}

			if (regenerateOverlay)
				sceneManipulator.Sync (); // may need to explicitly resync overlay data?

			if (repaintSceneView)
				SceneView.RepaintAll();
		}

		/** Gui drawn if the selected object does not have a valid and initialised hull painter on it
		 */
		private void DrawInactiveGui()
		{
			if (Selection.transforms.Length == 1)
			{
				// Have a single scene selection, is it viable?

				GameObject selectedObject = Selection.transforms[0].gameObject;
				MeshFilter srcMesh = SelectionUtil.FindSelectedMeshFilter();

				if (srcMesh != null)
				{
					GUILayout.Space(10);
					GUILayout.Label("Generate an asset to start painting:");
					CommonUi.DrawGenerateOrReconnectGui(selectedObject, srcMesh.sharedMesh);
				}
				else
				{
					// No mesh filter, might have a hull painter (or not)

					GUILayout.Space(10);
					GUILayout.Label("To start painting, select a single scene object");
					GUILayout.Label("The object must contain a MeshFilter");

					GUILayout.Space(10);
					GUILayout.Label("No MeshFilter on selected object", EditorStyles.centeredGreyMiniLabel);
				}
			}
			else
			{
				// No single scene selection
				// Could be nothing selected
				// Could be multiple selection
				// Could be an asset in the project selected

				GUILayout.Space(10);
				GUILayout.Label("To start painting, select a single scene object");
				GUILayout.Label("The object must contain a MeshFilter");
			}
		}

		private void DrawToolGui()
		{
			areToolsFoldedOut = EditorGUILayout.Foldout(areToolsFoldedOut, new GUIContent("Tools", toolsIcons), foldoutStyle);
			if (areToolsFoldedOut)
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Brush:", GUILayout.MaxWidth(40), GUILayout.Height(20));

					ToolSelection currentToolSelection = sceneManipulator.GetCurrentTool();
					PaintingBrush currentBrushSize = sceneManipulator.GetCurrentBrush();

					Texture[] brushIcons = new Texture[] { preciseBrushIcon, smallBrushIcon, mediumBrushIcon, largeBrushIcon };
					int brushId = (currentToolSelection == ToolSelection.TrianglePainting) ? (int)currentBrushSize : -1;
					int newBrushId = GUILayout.Toolbar(brushId, brushIcons, UnityEditor.EditorStyles.miniButton, GUILayout.Width(120), GUILayout.Height(20));

					int pipetteId = (currentToolSelection == ToolSelection.Pipette ? 0 : -1);
					int newPipetteId = GUILayout.Toolbar(pipetteId, new Texture[] { currentToolSelection == ToolSelection.Pipette ? pipetteIcon : pipetteIcon }, UnityEditor.EditorStyles.miniButton, GUILayout.Height(20), GUILayout.Width(30));

					if (newBrushId != brushId)
					{
						sceneManipulator.SetTool(ToolSelection.TrianglePainting);
						sceneManipulator.SetBrush((PaintingBrush)newBrushId);
					}
					else if (newPipetteId != pipetteId)
					{
						sceneManipulator.SetTool(newPipetteId == 0 ? ToolSelection.Pipette : ToolSelection.TrianglePainting);
					}

					if (GUILayout.Button("Generate colliders"))
					{
						GenerateColliders();
					}

					if (GUILayout.Button("Delete colliders"))
					{
						DeleteColliders();
					}
				}
				GUILayout.EndHorizontal();

			} // end foldout

			DrawUiDivider();
		}

		private void DrawHullGUI()
		{
			areHullsFoldedOut = EditorGUILayout.Foldout(areHullsFoldedOut, new GUIContent("Hulls", hullsIcon), foldoutStyle);
			if (areHullsFoldedOut)
			{
				HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

				// Figure out collumn widths based on which are actually visible

				Dictionary<Collumn, float> collumnWidths = new Dictionary<Collumn, float>();
				collumnWidths.Add(Collumn.Visibility,	IsCollumnVisible(Collumn.Visibility) ? 45 : 0);
				collumnWidths.Add(Collumn.Colour,		IsCollumnVisible(Collumn.Colour) ? 45 : 0);
				collumnWidths.Add(Collumn.Type,			IsCollumnVisible(Collumn.Type) ? 80 : 0);
				collumnWidths.Add(Collumn.Trigger,		IsCollumnVisible(Collumn.Trigger) ? 45 : 0);
				collumnWidths.Add(Collumn.Paint,		IsCollumnVisible(Collumn.Paint) ? 40 : 0);
				collumnWidths.Add(Collumn.Delete,		IsCollumnVisible(Collumn.Delete) ? 45 : 0);

				float fixedWidth = 0;
				int numOptional = 0;
				foreach (float width in collumnWidths.Values)
				{
					fixedWidth += width;
					if (width > 0)
						numOptional++;
				}
				fixedWidth += 12 + (numOptional * 4); // extra space for window chrome
				if (IsCollumnVisible(Collumn.Material))
					fixedWidth += 4;

				float baseWidth = EditorGUIUtility.currentViewWidth;
				float flexibleWidth = baseWidth - fixedWidth;
				
				if (IsCollumnVisible(Collumn.Material))
				{
					collumnWidths.Add(Collumn.Name, flexibleWidth * 0.5f);
					collumnWidths.Add(Collumn.Material, flexibleWidth * 0.5f);
				}
				else
				{
					collumnWidths.Add(Collumn.Name, flexibleWidth);
					collumnWidths.Add(Collumn.Material, 0);
				}	

				// Create dictionary of Collumn->LayoutOption for direct use by controls below
				Dictionary<Collumn, GUILayoutOption> widths = new Dictionary<Collumn, GUILayoutOption>();
				foreach (Collumn col in collumnWidths.Keys)
				{
					widths.Add(col, GUILayout.Width(collumnWidths[col]));
				}

				// Collumn headings for the hull rows

				GUILayout.BeginHorizontal();
				{
					if (IsCollumnVisible(Collumn.Visibility))
						GUILayout.Label("Visible", widths[Collumn.Visibility]);

					if (IsCollumnVisible(Collumn.Name))
						GUILayout.Label("Name",		widths[Collumn.Name]);

					if (IsCollumnVisible(Collumn.Colour))
						GUILayout.Label("Colour",	widths[Collumn.Colour]);

					if (IsCollumnVisible(Collumn.Type))
						GUILayout.Label("Type",		widths[Collumn.Type]);

					if (IsCollumnVisible(Collumn.Material))
						GUILayout.Label("Material", widths[Collumn.Material]);

					if (IsCollumnVisible(Collumn.Trigger))
						GUILayout.Label("Trigger",	widths[Collumn.Trigger]);

					if (IsCollumnVisible(Collumn.Paint))
						GUILayout.Label("Paint",	widths[Collumn.Paint]);

					if (IsCollumnVisible(Collumn.Delete))
						GUILayout.Label("Delete",	widths[Collumn.Delete]);
				}
				GUILayout.EndHorizontal();

				// The actual hull rows with all the data for an individual hull

				for (int i = 0; i < currentHullPainter.paintingData.hulls.Count; i++)
				{
					DrawHullGUILine(i, currentHullPainter.paintingData.hulls[i], widths);
				}

				// The row of macro buttons at the bottom of each hull collumn (Show all, Delete all, etc.)

				GUILayout.BeginHorizontal();
				{
					if (IsCollumnVisible(Collumn.Visibility))
					{
						bool allHullsVisible = AreAllHullsVisible();
						if (GUILayout.Button(new GUIContent(" All", allHullsVisible ? hullInvisibleIcon : hullVisibleIcon), widths[Collumn.Visibility]))
						{
							if (allHullsVisible)
								SetAllHullsVisible(false); // Hide all
							else
								SetAllHullsVisible(true); // Show all
						}
					}

					if (IsCollumnVisible(Collumn.Name))
					{
						if (GUILayout.Button(new GUIContent("Add Hull", addHullIcon), widths[Collumn.Name]))
						{
							AddHull();
						}
					}

					if (IsCollumnVisible(Collumn.Colour))
						GUILayout.Label("", widths[Collumn.Colour]);

					if (IsCollumnVisible(Collumn.Type))
						GUILayout.Label("", widths[Collumn.Type]);

					if (IsCollumnVisible(Collumn.Material))
						GUILayout.Label("", widths[Collumn.Material]);

					if (IsCollumnVisible(Collumn.Trigger))
						GUILayout.Label("", widths[Collumn.Trigger]);

					if (IsCollumnVisible(Collumn.Paint))
					{
						if (GUILayout.Button("Stop", widths[Collumn.Paint]))
						{
							StopPainting();
						}
					}

					if (IsCollumnVisible(Collumn.Delete))
					{
						if (GUILayout.Button(new GUIContent(" All", deleteIcon), widths[Collumn.Delete]))
						{
							DeleteHulls();
						}
					}
				}
				GUILayout.EndHorizontal();
			}
			DrawUiDivider();
		}

		private void DrawHullGUILine(int hullIndex, Hull hull, Dictionary<Collumn, GUILayoutOption> widths)
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			Undo.RecordObject (currentHullPainter.paintingData, "Edit Hull");

			GUILayout.BeginHorizontal ();
			{
				if (IsCollumnVisible(Collumn.Visibility))
				{
					if (GUILayout.Button(hull.isVisible ? hullVisibleIcon : hullInvisibleIcon, EditorStyles.miniButton, GUILayout.Height(16), widths[Collumn.Visibility]))
					{
						hull.isVisible = !hull.isVisible;
						regenerateOverlay = true;
					}
				}

				if (IsCollumnVisible(Collumn.Name))
					hull.name = EditorGUILayout.TextField(hull.name, GUILayout.MinWidth(60), widths[Collumn.Name] );

				if (IsCollumnVisible(Collumn.Colour))
				{
					Color prevColour = hull.colour;
					hull.colour = EditorGUILayout.ColorField("", hull.colour, widths[Collumn.Colour]);
					if (prevColour != hull.colour)
					{
						regenerateOverlay = true;
						repaintSceneView = true;
					}
				}

				if (IsCollumnVisible(Collumn.Type))
					hull.type = (HullType)EditorGUILayout.EnumPopup(hull.type, widths[Collumn.Type]);

				if (IsCollumnVisible(Collumn.Material))
					hull.material = (PhysicMaterial)EditorGUILayout.ObjectField(hull.material, typeof(PhysicMaterial), false, widths[Collumn.Material]);

				if (IsCollumnVisible(Collumn.Trigger))
				{
					if (GUILayout.Button(hull.isTrigger ? triggerOnIcon : triggerOffIcon, EditorStyles.miniButton, widths[Collumn.Trigger]))
					{
						hull.isTrigger = !hull.isTrigger;
					}
				}

				if (IsCollumnVisible(Collumn.Paint))
				{
					int prevHullIndex = currentHullPainter.paintingData.activeHull;

					bool isPainting = (currentHullPainter.paintingData.activeHull == hullIndex);
					int nowSelected = GUILayout.Toolbar(isPainting ? 0 : -1, new Texture[] { isPainting ? paintOnIcon : paintOffIcon }, EditorStyles.miniButton, widths[Collumn.Paint]);
					if (nowSelected == 0 && prevHullIndex != hullIndex)
					{
						// Now painting this index!
						currentHullPainter.paintingData.activeHull = hullIndex;
					}
				}

				if (IsCollumnVisible(Collumn.Delete))
				{
					if (GUILayout.Button(deleteIcon, EditorStyles.miniButton, widths[Collumn.Delete]))
					{
						hullToDelete = hullIndex;
						regenerateOverlay = true;
						repaintSceneView = true;
					}
				}
			}
			GUILayout.EndHorizontal ();
		}

		private void DrawSettingsGui()
		{
			areSettingsFoldedOut = EditorGUILayout.Foldout(areSettingsFoldedOut, new GUIContent("Settings", settingsIcon), foldoutStyle);
			if (areSettingsFoldedOut)
			{
				float baseWidth = EditorGUIUtility.currentViewWidth - 20; // -20px for window chrome
				float fixedWidth = 120 + 120 + 4; // sum of fixed widths below
				float flexibleWidth = baseWidth - fixedWidth;
				float[] collumnWidth =
				{
					120,
					flexibleWidth,
					120,
				};

				DrawDefaultType(collumnWidth);
				DrawDefaultTrigger(collumnWidth);
				DrawDefaultMaterial(collumnWidth);
				DrawFaceDepth(collumnWidth);
				DrawVisibilityToggles(collumnWidth);
				

				
				// TODO: EditorGUILayout.EnumFlagsField added in 2017.3 - use this for collumn visibility drop down
#if UNITY_2017_3_OR_NEWER
				
#endif
			}
			DrawUiDivider();
		}

		private void DrawCollumnToggle(Collumn colType, string label)
		{
			bool isVisible = IsCollumnVisible(colType);
			bool nowVisible = GUILayout.Toggle(isVisible, label);
			if (nowVisible)
			{
				visibleCollumns |= colType;
			}
			else
			{
				visibleCollumns &= ~colType;
			}
		}

		private void DrawDefaultType (float[] collumnWidths)
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			GUILayout.BeginHorizontal ();
			{
				GUILayout.Label("Default type:", GUILayout.Width(collumnWidths[0]));

				defaultType = (HullType)EditorGUILayout.EnumPopup(defaultType, GUILayout.Width(100));

				GUILayout.Label("", GUILayout.Width(collumnWidths[1]-100));

				if (GUILayout.Button("Apply To All", GUILayout.Width(collumnWidths[2])) )
				{
					currentHullPainter.SetAllTypes(defaultType);
				}
			}
			GUILayout.EndHorizontal();
		}

		private void DrawDefaultMaterial(float[] collumnWidths)
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Default material:", GUILayout.Width(collumnWidths[0]));

				defaultMaterial = (PhysicMaterial)EditorGUILayout.ObjectField(defaultMaterial, typeof(PhysicMaterial), false, GUILayout.Width(collumnWidths[1]+4));
				
				if (GUILayout.Button("Apply To All", GUILayout.Width(collumnWidths[2])))
				{
					currentHullPainter.SetAllMaterials(defaultMaterial);
				}
			}
			GUILayout.EndHorizontal();
		}

		private void DrawDefaultTrigger (float[] collumnWidths)
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			GUILayout.BeginHorizontal ();
			{
				GUILayout.Label("Default trigger:", GUILayout.Width(collumnWidths[0]));

				if (GUILayout.Button(defaultIsTrigger ? triggerOnIcon : triggerOffIcon, GUILayout.Width(100)))
				{
					defaultIsTrigger = !defaultIsTrigger;
				}

				GUILayout.Label("", GUILayout.Width(collumnWidths[1]-100));

				if (GUILayout.Button("Apply To All", GUILayout.Width(collumnWidths[2])))
				{
					currentHullPainter.SetAllAsTrigger(defaultIsTrigger);
				}
			}
			GUILayout.EndHorizontal();
		}

		private void DrawFaceDepth (float[] collumnWidths)
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			GUILayout.BeginHorizontal ();
			{
				GUILayout.Label("Face thickness:", GUILayout.Width(collumnWidths[0]));

				currentHullPainter.paintingData.faceThickness = EditorGUILayout.FloatField(currentHullPainter.paintingData.faceThickness, GUILayout.Width(collumnWidths[1]+4));

				float inc = 0.1f;
				if (GUILayout.Button("+", GUILayout.Width((collumnWidths[2]-4)/2)))
				{
					currentHullPainter.paintingData.faceThickness = currentHullPainter.paintingData.faceThickness + inc;
				}
				if (GUILayout.Button("-", GUILayout.Width((collumnWidths[2]-4)/2)))
				{
					currentHullPainter.paintingData.faceThickness = currentHullPainter.paintingData.faceThickness - inc;
				}
			}
			GUILayout.EndHorizontal();
		}

		private void DrawVisibilityToggles(float[] collumnWidths)
		{
			GUILayout.BeginHorizontal();

			GUILayout.Label("Show collumns:", GUILayout.Width(collumnWidths[0]));
			DrawCollumnToggle(Collumn.Visibility, "Visibility");
			DrawCollumnToggle(Collumn.Colour, "Colour");
			DrawCollumnToggle(Collumn.Type, "Type");
			DrawCollumnToggle(Collumn.Material, "Material");
			DrawCollumnToggle(Collumn.Trigger, "Trigger");

			GUILayout.EndHorizontal();
		}

		private void DrawHullWarnings (HullPainter currentHullPainter)
		{
			List<string> warnings = new List<string> ();

			for (int i=0; i<currentHullPainter.paintingData.hulls.Count; i++)
			{
				Hull hull = currentHullPainter.paintingData.hulls[i];
				if (hull.hasColliderError)
				{
					warnings.Add("'"+hull.name+"' generates a collider with "+hull.numColliderFaces+" faces");
				}
			}
			
			if (warnings.Count > 0)
			{
				GUILayout.BeginHorizontal();

				GUIStyle iconStyle = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter};
				GUILayout.Label(errorIcon, iconStyle, GUILayout.Height(40), GUILayout.ExpandWidth(true));

				GUILayout.BeginVertical();
				{
					GUILayout.Label ("Errors", EditorStyles.boldLabel);

					foreach (string str in warnings)
					{
						GUILayout.Label (str);
					}
					
					GUILayout.Label ("Unity only allows max 256 faces per hull");
					GUILayout.Label ("Simplify this hull or split into multiple hulls");
				}
				GUILayout.EndVertical();

				GUILayout.EndHorizontal();

				DrawUiDivider();
			}
		}

		private void DrawAssetGui()
		{
			areAssetsFoldedOut = EditorGUILayout.Foldout(areAssetsFoldedOut, new GUIContent("Assets", assetsIcon), foldoutStyle);
			if (areAssetsFoldedOut)
			{
				HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();
				
				string paintingPath = AssetDatabase.GetAssetPath(currentHullPainter.paintingData);
				GUILayout.Label("Painting data: "+paintingPath, EditorStyles.centeredGreyMiniLabel);
			
				string hullPath = AssetDatabase.GetAssetPath(currentHullPainter.hullData);
				GUILayout.Label("Hull data: "+hullPath, EditorStyles.centeredGreyMiniLabel);

				if (GUILayout.Button("Disconnect from assets"))
				{
					sceneManipulator.DisconnectAssets();

					currentHullPainter = null;
					repaintSceneView = true;
					regenerateOverlay = true;
				}
			}
		}

		public void OnSceneGUI ()
		{
			if (sceneManipulator.Sync ())
			{
				Repaint();
			}
			
			int controlId = GUIUtility.GetControlID (FocusType.Passive);
			
			if (Event.current.type == EventType.MouseDown && (Event.current.button == 0))
			{
				bool eventConsumed = sceneManipulator.DoMouseDown();
				if (eventConsumed)
				{
					activeMouseButton = Event.current.button;
					GUIUtility.hotControl = controlId;
					Event.current.Use();
				}

			}
			else if (Event.current.type == EventType.MouseDrag && Event.current.button == activeMouseButton)
			{
				bool eventConsumed = sceneManipulator.DoMouseDrag();
				if (eventConsumed)
				{
					GUIUtility.hotControl = controlId;
					Event.current.Use();
					Repaint();
				}

			}
			else if (Event.current.type == EventType.MouseUp && Event.current.button == activeMouseButton)
			{
				bool eventConsumed = sceneManipulator.DoMouseUp();
				if (eventConsumed)
				{
					activeMouseButton = -1;
					GUIUtility.hotControl = 0;
					Event.current.Use();
				}
			}
		}

		private void GenerateColliders()
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			if (currentHullPainter == null)
				return;
			
			Undo.RegisterCompleteObjectUndo (currentHullPainter.gameObject, "Generate Colliders");

			// Fetch the data assets

			PaintingData paintingData = currentHullPainter.paintingData;
			HullData hullData = currentHullPainter.hullData;

			string hullAssetPath = AssetDatabase.GetAssetPath (hullData);
			
			// Create / update the hull meshes

			foreach (Hull hull in paintingData.hulls)
			{
				paintingData.GenerateCollisionMesh(hull, sceneManipulator.GetTargetVertices(), sceneManipulator.GetTargetTriangles());
			}

			// Sync the in-memory hull meshes with the asset meshes in hullAssetPath

			List<Mesh> existingMeshes = GetAllMeshesInAsset (hullAssetPath);

			foreach (Mesh existing in existingMeshes)
			{
				if (!paintingData.ContainsMesh(existing))
				{
					GameObject.DestroyImmediate(existing, true);
				}
			}

			foreach (Hull hull in paintingData.hulls)
			{
				if (hull.collisionMesh != null)
				{
					if (!existingMeshes.Contains(hull.collisionMesh))
					{
						AssetDatabase.AddObjectToAsset(hull.collisionMesh, hullAssetPath);
					}
				}
				if (hull.faceCollisionMesh != null)
				{
					if (!existingMeshes.Contains(hull.faceCollisionMesh))
					{
						AssetDatabase.AddObjectToAsset(hull.faceCollisionMesh, hullAssetPath);
					}
				}
			}

			EditorUtility.SetDirty (hullData);

			AssetDatabase.SaveAssets ();

			// Add collider components to the target object

			currentHullPainter.CreateColliderComponents ();
		}

		private void AddHull()
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			if (currentHullPainter != null)
			{
				Undo.RecordObject (currentHullPainter.paintingData, "Add Hull");
				currentHullPainter.paintingData.AddHull(defaultType, defaultMaterial, defaultIsTrigger);

				EditorUtility.SetDirty (currentHullPainter.paintingData);
			}
		}

		private void SetAllHullsVisible(bool visible)
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			if (currentHullPainter != null && currentHullPainter.paintingData != null)
			{
				for (int i=0; i<currentHullPainter.paintingData.hulls.Count; i++)
				{
					currentHullPainter.paintingData.hulls[i].isVisible = visible;
				}
			}

			regenerateOverlay = true;
		}

		private void StopPainting()
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();

			if (currentHullPainter != null && currentHullPainter.paintingData != null)
			{
				currentHullPainter.paintingData.activeHull = -1;
			}
		}

		private void DeleteColliders()
		{
			Undo.SetCurrentGroupName ("Destroy Colliders");

			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();
			currentHullPainter.RemoveAllColliders ();
		}

		private void DeleteHulls ()
		{
			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter ();
			if (currentHullPainter != null && currentHullPainter.hullData != null)
			{
				currentHullPainter.paintingData.RemoveAllHulls ();
				repaintSceneView = true;
			}
		}

		private bool AreAllHullsVisible()
		{
			bool allVisible = true;

			HullPainter currentHullPainter = sceneManipulator.GetCurrentHullPainter();
			if (currentHullPainter != null && currentHullPainter.paintingData != null)
			{
				for (int i = 0; i < currentHullPainter.paintingData.hulls.Count; i++)
				{
					if (!currentHullPainter.paintingData.hulls[i].isVisible)
					{
						allVisible = false;
						break;
					}
				}
			}

			return allVisible;
		}

		private bool IsCollumnVisible(Collumn col)
		{
			return (visibleCollumns & col) > 0;
		}

		private List<Mesh> GetAllMeshesInAsset(string assetPath)
		{
			List<Mesh> meshes = new List<Mesh> ();

			foreach (UnityEngine.Object o in AssetDatabase.LoadAllAssetsAtPath(assetPath))
			{
				if (o is Mesh)
				{
					meshes.Add((Mesh)o);
				}
			}

			return meshes;
		}

		void OnSceneGUI(SceneView sceneView)
		{
			sceneManipulator.OnSceneGUI(sceneView);

			if (Event.current.type == EventType.Repaint)
			{
				if (sceneManipulator.GetCurrentTool() == ToolSelection.TrianglePainting)
				{
					Handles.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

					int pickRadius = sceneManipulator.GetBrushPixelSize();

					Ray centerRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
					Ray rightRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition + new Vector2(pickRadius, 0.0f));
					Ray upRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition + new Vector2(0.0f, pickRadius));

					Vector3 centerPos = centerRay.origin + centerRay.direction;
					Vector3 upPos = upRay.origin + upRay.direction;
					Vector3 rightPos = rightRay.origin + rightRay.direction;

					Vector3 upVec = upPos - centerPos;
					Vector3 rightVec = rightPos - centerPos;

					List<Vector3> points = new List<Vector3>();

					int numSegments = 20;

					for (int i = 0; i < numSegments; i++)
					{
						float angle0 = (float)i / (float)numSegments * Mathf.PI * 2.0f;
						float angle1 = (float)(i + 1) / (float)numSegments * Mathf.PI * 2.0f;

						Vector3 p0 = centerPos + (rightVec * Mathf.Cos(angle0)) + (upVec * Mathf.Sin(angle0));
						Vector3 p1 = centerPos + (rightVec * Mathf.Cos(angle1)) + (upVec * Mathf.Sin(angle1));

						points.Add(p0);
						points.Add(p1);
					}

					Handles.DrawLines(points.ToArray());
				}
			}
		}

		public static string FindInstallPath()
		{
			string installPath = defaultInstallPath;
			
			string[] foundIds = AssetDatabase.FindAssets ("AddHullIcon t:texture2D");
			if (foundIds.Length > 0)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath (foundIds [0]);
				int lastSlashPos = assetPath.LastIndexOf("/");
				if (lastSlashPos != -1)
				{
					string newInstallPath = assetPath.Substring(0, lastSlashPos+1);
					installPath = newInstallPath;
				}
			}

			return installPath;
		}

		private void DrawUiDivider()
		{
			DrawUiDivider(dividerColour);
		}

		public static void DrawUiDivider(Color color, int thickness = 1, int padding = 10)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			r.height = thickness;
			r.y += padding / 2;
			r.x -= 2;
			r.width += 6;
			EditorGUI.DrawRect(r, color);
		}

	}

} // namespace Technie.PhysicsCreator
