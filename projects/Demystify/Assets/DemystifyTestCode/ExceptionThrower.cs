using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demystify._Tests;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using Debug = UnityEngine.Debug;

public static class ExceptionThrower
{
	private static void Invoke(Action action)
	{
		try
		{
			action();
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}

	private static int index;
	private static readonly List<Action> actions = new List<Action>();

	private class MyGenericClass<T>
	{
		public MyGenericClass((string, string) someParam, Action action)
		{
			Action m = async () => await Callback(action);
			m();
		}
		
		public MyGenericClass(Action action) : this(("hello", "world"), action)
		{
		}

		public async Task<string> Callback(Action callback, int depth = 0)
		{
			while (depth < 2)
			{
				await Task.Delay(1);
				await Callback(callback, ++depth);
				return null;
			}

			callback?.Invoke();
			return "123";
		}
	}

	private class AnotherCallSomewhere
	{
		public AnotherCallSomewhere(Action callback)
		{
			new MyGenericClass<string>(callback);
		}
	}

	private static void Loop()
	{
		if (index >= actions.Count)
		{
			EditorApplication.update -= Loop;
			return;
		}

		var action = actions[index];
		var anotherCallSomewhere = new AnotherCallSomewhere(action);
		index++;
	}

	[MenuItem("Mystery/" + nameof(ThrowExceptions))]
	public static void ThrowExceptions()
	{
		EditorApplication.update += Loop;
		
		actions.Add(() => throw new NullReferenceException("Throwing immediately"));
		actions.Add(() =>
		{
			var list = new List<int>();
			for (var i = 0; i < 1000; i++)
			{
				var entry = list[i];
				Debug.Log("Never called " + entry);
			}
		});

		// Run(() => throw new NullReferenceException("1"));
		// actions.Add(() => throw new NullReferenceException());
		// // actions.Add(() => throw new EvaluateException());
		// // actions.Add(() => throw new ArithmeticException());
		// actions.Add(() => throw new AggregateException(new UnityException("OMG")));
	}


	[MenuItem("Mystery/" + nameof(RunSample))]
	public static void RunSample()
	{
		Program.Main();
	}
}