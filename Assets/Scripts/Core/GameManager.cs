using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    int framerate = 60;
    void Awake()
    {
        Application.targetFrameRate = framerate;
    }
}
