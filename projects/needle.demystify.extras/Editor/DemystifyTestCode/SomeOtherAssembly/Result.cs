using System.Threading.Tasks;
using UnityEngine;

namespace Some.Arbitrary.Namespace
{
	
	public class Result<T, U>
	{

		public static Some.Arbitrary.Namespace.Result<T2, U> ReturnSomethingWeird<T2>() where T2 : T
		{
			Debug.Log("void UnityEngine.Debug.Log(object message)");
			return new Some.Arbitrary.Namespace.Result<T2, U>();
		}

	}
}