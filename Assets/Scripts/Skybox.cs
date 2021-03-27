using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skybox : MonoBehaviour
{
    public float rotationSpeed = 1;

    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", rotationSpeed * Time.time);
    }
}
