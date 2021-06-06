using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PrintSimpleHyperlink
{
    [MenuItem("Test/Print Links")]
    private static void Init()
    {
        Debug.Log("<a href=\"www.unity.com\">unity.com</a>");
    }
}
