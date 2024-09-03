using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DesktopPathDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        Debug.Log("Desktop Path: " + desktopPath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
