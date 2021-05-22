using System.Threading.Tasks;
using Some.Arbitrary.Namespace;
using UnityEditor;
using UnityEngine;

public static class ReturnGeneric
{
	[MenuItem("Mystery/" + nameof(DoReturnGeneric))]
	public static void DoReturnGeneric()
	{
		Result<object, object>.ReturnSomethingWeird<object>();
	}

}



