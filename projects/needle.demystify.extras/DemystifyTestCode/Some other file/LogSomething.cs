using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LogSomething
{
    [MenuItem("Mystery/LogNow")]
    public static void LogNow()
    {
        Debug.Log("HELLO");
    }
}
