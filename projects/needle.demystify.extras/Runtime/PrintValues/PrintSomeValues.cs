using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintSomeValues : MonoBehaviour
{
    private void Update()
    {
        Debug.Log("This is not grouped " + Time.time);
        Debug.Log("<group> Random Value: " + Random.value.ToString("0.00")); // + ", " + Random.insideUnitSphere);
        Debug.Log("<group> Bit more complex: " + Random.value.ToString("0.00") + ", " + Random.insideUnitSphere);
        Debug.Log("<group> Frame: " + Time.frameCount + ", " + Time.time + ", " + Random.rotation);
    }
}
