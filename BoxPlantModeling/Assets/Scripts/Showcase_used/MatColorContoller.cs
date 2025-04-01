using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatColorContoller : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(181.0f / 255.0f, 255.0f / 255.0f, 144.0f / 255.0f, 0.5f));
    }
}
