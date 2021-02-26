using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ExceptionComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MyAsyncMethod();
    }

    private async void MyAsyncMethod()
    {
        await Task.Delay(5);
        throw new Exception("Madeup Exception");
    }
}
