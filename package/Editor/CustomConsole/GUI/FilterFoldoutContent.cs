using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	internal class FilterFoldoutContent : PopupWindowContent
	{
		public override Vector2 GetWindowSize()
		{
			var enabled = DemystifySettings.instance.CustomList;
			var noConfig = ConsoleFilterConfig.AllConfigs.Count <= 0;
			if (noConfig && enabled) return new Vector2(150, EditorGUIUtility.singleLineHeight * 1.3f);
			return new Vector2(400, 300);
		}

		private Vector2 scroll;

		private bool configsFoldout
		{
			get => SessionState.GetBool("ConsoleFilterConfigListFoldout", true);
			set => SessionState.SetBool("ConsoleFilterConfigListFoldout", value);
		}

		public override void OnGUI(Rect rect)
		{
			var enabled = DemystifySettings.instance.CustomList;
			if (!enabled)
			{
				EditorGUILayout.HelpBox("To support console filtering you need to enable \"Custom List\" in settings", MessageType.Warning);
				if (GUILayout.Button("Enable Custom List", GUILayout.Height(30)))
				{
					DemystifySettings.instance.CustomList = true;
				}
			}

			if (ConsoleFilterConfig.AllConfigs.Count <= 0)
			{
				if (enabled)
				{
					GUILayout.FlexibleSpace();
					if (GUILayout.Button(new GUIContent("Create Filter Config",
						"A filter config is used to store your settings for filtering console logs. Don't worry, logs are not deleted or anything, they will just not be shown when filtered and this is can be changed at any time")))
					{
						var config = ConsoleFilterConfig.CreateAsset();
						if (config)
							config.Activate();
					}
					GUILayout.FlexibleSpace();
				}

				return;
			}

			scroll = EditorGUILayout.BeginScrollView(scroll);

			configsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(configsFoldout, "Filter Configs In Project", null, r =>
			{
				var menu = new GenericMenu();
				menu.AddItem(new GUIContent("New"), false, () => ConsoleFilterConfig.CreateAsset());
				menu.DropDown(r);
			});
			if (configsFoldout)
			{
				EditorGUI.indentLevel++;
				foreach (var config in ConsoleFilterConfig.AllConfigs)
				{
					// using (new GUILayout.HorizontalScope())
					{
						using (var activeState = new EditorGUI.ChangeCheckScope())
						{
							var res = EditorGUILayout.ToggleLeft(config.name, config.IsActive);
							if (activeState.changed)
							{
								if (res) config.Activate();
								else config.Deactivate();
							}
						}

						if (Event.current.type == EventType.MouseDown)
						{
							if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
							{
								if (Event.current.button == 0)
									EditorGUIUtility.PingObject(config);
							}
						}
					}
				}

				EditorGUI.indentLevel--;
			}

			EditorGUILayout.EndFoldoutHeaderGroup();

			Draw.FilterList(ConsoleFilter.RegisteredFilter);

			EditorGUILayout.EndScrollView();
		}
	}
}