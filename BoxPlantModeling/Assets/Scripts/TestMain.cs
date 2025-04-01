using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataStructures.ViliWonka.KDTree;


public class TestMain : MonoBehaviour
{
    public GameObject m_object;
    void Start()
    {
        //Geometry.CreatePrefab("Flowers/flower1",Vector3.zero, Quaternion.identity);
        //GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //obj.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        //obj.GetComponent<MeshFilter>().mesh =
        //    Geometry.CreateLeaf(pt:new Vector3(0,1,0), dir: new Vector3(0,1,0),
        //    h: 1f, w: 1f, horAng: 3f, verAng: 3f,
        //    slidesHor: 4, slidesVer: 8);

        org.matheval.Expression exp = new org.matheval.Expression("l*a");
        exp.DisableFunction(new string[] { "e" });
        exp.Bind("l", 0.2f);
        exp.Bind("a", 0.8f);
        object answer = exp.Eval();
        Debug.Log(answer.ToString());
    }

    void OnDrawGizmos()
    {

    }
}
