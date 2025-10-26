using UnityEditor;
using UnityEngine;

namespace Needle.Editors
{
	internal static class Assets
	{
		private const string _fullLogoGuid = "3924433c6b50b804e98ee4c9b4628ac8";
		private static Texture2D _fullLogo;
		public static Texture2D FullLogo
		{
			get
			{
				if (_fullLogo) return _fullLogo;
				var path = AssetDatabase.GUIDToAssetPath(_fullLogoGuid);
				if (path != null)
				{
					return _fullLogo = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				}

				_fullLogo = Texture2D.blackTexture;
				return _fullLogo;
			}
		}
		
		private const string _fullLogoDarkModeGuid = "c972f5c4554ac264185a0684c9652a11";
		private static Texture2D _fullLogoDarkMode;
		public static Texture2D FullLogoDarkMode
		{
			get
			{
				if (_fullLogoDarkMode) return _fullLogoDarkMode;
				var path = AssetDatabase.GUIDToAssetPath(_fullLogoDarkModeGuid);
				if (path != null)
				{
					return _fullLogoDarkMode = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				}

				_fullLogoDarkMode = Texture2D.blackTexture;
				return _fullLogoDarkMode;
			}
		}
		
		
		
		private const string _iconGuid = "48269d7bfb9a97048a96fcd640809667";
		private static Texture2D _logo;
		public static Texture2D Logo
		{
			get
			{
				if (_logo) return _logo;
				var path = AssetDatabase.GUIDToAssetPath(_iconGuid);
				if (path != null)
				{
					return _logo = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				}

				_logo = Texture2D.blackTexture;
				return _logo;
			}
		}

		private const string _iconButtonGuid = "d34be00783aca1746af4a558b0c3dcff";
		private static Texture2D _logo_button;

		public static Texture2D LogoButton
		{
			get
			{
				if (_logo) return _logo;
				var path = AssetDatabase.GUIDToAssetPath(_iconButtonGuid);
				if (path != null)
				{
					return _logo_button = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				}

				_logo_button = Texture2D.blackTexture;
				return _logo_button;
			}
		}
		private static GUIStyle _labelStyle;

		public static void DrawGUIFullLogo()
		{
			var logo = EditorGUIUtility.isProSkin ? FullLogoDarkMode : FullLogo;
			if (logo)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space(15);

				var rect = GUILayoutUtility.GetRect(80, 40);

				if (Event.current.type == EventType.Repaint)
					GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);

				if (_labelStyle == null)
				{
					_labelStyle = new GUIStyle(EditorStyles.miniLabel);
					_labelStyle.alignment = TextAnchor.MiddleLeft;
				}

				GUILayout.Label(new GUIContent("Console by Needle"), _labelStyle, GUILayout.Height(38));

				EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
				if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition))
					Application.OpenURL("https://needle.tools");
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space(0);
			}
		}

		
		public static void DrawGUILogoMiniButton()
		{
			float maxHeight = 15;
			var logo = Assets.Logo;
			if (logo)
			{
				var rect = GUILayoutUtility.GetRect(maxHeight+5, maxHeight);
				rect.height = maxHeight;
				rect.y += 2f;
				rect.width = maxHeight;
				if (Event.current.type == EventType.Repaint)
					GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);
				GUI.Label(rect, new GUIContent(string.Empty, "Needle Console"), GUIStyle.none);

				UnityEditor.EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
				if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition))
					Application.OpenURL("https://needle.tools");
			}
		}
	}
}