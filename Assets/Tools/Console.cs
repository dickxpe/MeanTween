// Author: Peter Dickx https://github.com/dickxpe
// MIT License - Copyright (c) 2024 Peter Dickx

using UnityEngine;

public class Console : MonoBehaviour
{
    public void Log(string message)
    {
        Debug.Log(message);
    }

    public void Log(int message)
    {
        Debug.Log(message.ToString());
    }

    public void Log(float message)
    {
        Debug.Log(message.ToString());
    }

    public void Log(Component component)
    {
        Debug.Log(component.ToString());
    }

    public static void Log(GameObject gameObject)
    {
        Debug.Log(gameObject.ToString());
    }

    public static void Log(Vector3 vector3)
    {
        Debug.Log(vector3.ToString());
    }
}