using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	internal static class Textures
	{
		private static Texture2D eyeClosed;
		public static Texture2D EyeClosed
		{
			get
			{
				if (!eyeClosed) eyeClosed = EditorGUIUtility.FindTexture("animationvisibilitytoggleoff");
				return eyeClosed;
			}
		}
		
		private static Texture2D eyeOpen;
		public static Texture2D EyeOpen
		{
			get
			{
				if (!eyeOpen) eyeOpen = EditorGUIUtility.FindTexture("animationvisibilitytoggleon");
				return eyeOpen;
			}
		}
	}
}