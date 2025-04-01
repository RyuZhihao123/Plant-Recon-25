using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBV : MonoBehaviour
{
    // Start is called before the first frame update

    GameObject ConstructPie(float y_base = 0.0f, float y_height = 0.1f, float radii_left = 1.0f, float radii_right = 1.0f, float raddi_random=0.0f)
    {
        List<Vector3> vec = new List<Vector3>();
        List<Vector3> norm = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();


        Vector3 center = new Vector3(0, y_base, 0);
        Vector3 center_high = new Vector3(0, y_base+y_height, 0);
        int verCount = 0;
        int pie_number = 10;
        int pie_precision = 50;
        for (int i = 0; i < pie_number; ++i)
        {

            float radii = (radii_left + radii_right) / 2.0f + raddi_random * Random.Range(-radii_left / 5.0f, radii_left / 5.0f);

            if (i == 0) radii = radii_left;
            if (i == pie_number / 2) radii = radii_right;

            for (int k = 0; k < pie_precision; k++)
            {
                // a - center
                // | /
                // b

                float startAngle = i * 360.0f / pie_number + k * 360.0f / pie_number / pie_precision;
                float endAngle = i * 360.0f / pie_number + (k + 1) * 360.0f / pie_number / pie_precision;

                Vector3 a_low = center + new Vector3(radii * Mathf.Cos(startAngle * Mathf.Deg2Rad), 0,
                    radii * Mathf.Sin(startAngle * Mathf.Deg2Rad));
                Vector3 b_low = center + new Vector3(radii * Mathf.Cos(endAngle * Mathf.Deg2Rad), 0,
                    radii * Mathf.Sin(endAngle * Mathf.Deg2Rad));
                Vector3 a_high = a_low + new Vector3(0, y_height, 0);
                Vector3 b_high = b_low + new Vector3(0, y_height, 0);

                // ²àÃæ
                vec.Add(a_low); vec.Add(a_high); vec.Add(b_low);
                vec.Add(b_high); vec.Add(b_low); vec.Add(a_high);
                vec.Add(a_high); vec.Add(center_high); vec.Add(b_high);
                vec.Add(b_low); vec.Add(center); vec.Add(a_low);

                indices.Add(verCount + 0); indices.Add(verCount + 1); indices.Add(verCount + 2);
                indices.Add(verCount + 3); indices.Add(verCount + 4); indices.Add(verCount + 5);
                indices.Add(verCount + 6); indices.Add(verCount + 7); indices.Add(verCount + 8);
                indices.Add(verCount + 9); indices.Add(verCount + 10); indices.Add(verCount + 11);

                verCount += 12;

                if (k == 0)
                {
                    vec.Add(a_high); vec.Add(a_low); vec.Add(center);
                    vec.Add(a_high); vec.Add(center); vec.Add(center_high);

                    indices.Add(verCount + 0); indices.Add(verCount + 1); indices.Add(verCount + 2);
                    indices.Add(verCount + 3); indices.Add(verCount + 4); indices.Add(verCount + 5);
                    verCount += 6;
                }

                if (k == pie_precision - 1) 
                {
                    vec.Add(b_low); vec.Add(b_high); vec.Add(center_high);
                    vec.Add(center_high); vec.Add(center); vec.Add(b_low);

                    indices.Add(verCount + 0); indices.Add(verCount + 1); indices.Add(verCount + 2);
                    indices.Add(verCount + 3); indices.Add(verCount + 4); indices.Add(verCount + 5);
                    verCount += 6;
                }

            }
        }

        Mesh mesh = new Mesh();

        mesh.vertices = vec.ToArray();
        mesh.triangles = indices.ToArray();
        
        mesh.RecalculateNormals();

        GameObject final_obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        final_obj.GetComponent<MeshFilter>().mesh = mesh;
        final_obj.GetComponent<MeshRenderer>().material.color = new Color32(100, 100, 100, 255);
        return final_obj;
    }

    void Start()
    {

        GameObject rbv = new GameObject();

        // Ê÷Ä¾1
        //float[,] pie_data = new float[8, 3] {
        //    { 0.03f, 0.03f ,0.0f}, { 0.03f, 0.03f, 0.0f}, { 0.21f, 0.21f, 0.5f}, { 0.24f, 0.21f, 0.5f},
        //    { 0.20f, 0.24f, 0.4f}, { 0.15f, 0.15f, 0.4f}, { 0.10f, 0.10f, 0.0f}, { 0.05f, 0.05f, 0.0f}};

        // »ðº×»¨1
        float[,] pie_data = new float[8, 3] {
              { 0.1f, 0.1f ,0.0f}, { 0.29f, 0.32f, 0.0f}, { 0.38f, 0.49f, 0.5f}, { 0.47f, 0.45f, 0.5f},
              { 0.53f, 0.24f, 0.4f}, { 0.38f, 0.24f, 0.4f}, { 0.26f, 0.17f, 0.4f}, { 0.1f, 0.12f, 0.0f}};

        // Ê÷Ä¾2
        //float[,] pie_data = new float[8, 3] {
        //      { 0.03f, 0.03f ,0.0f}, { 0.52f, 0.54f, 0.3f}, { 0.48f, 0.56f, 0.2f}, { 0.45f, 0.48f, 0.2f},
        //      { 0.44f, 0.44f, 0.4f}, { 0.44f, 0.38f, 0.0f}, { 0.39f, 0.36f, 0.4f}, { 0.33f, 0.32f, 0.2f}};

        float height = 0.1f;
        for (int i = 0; i < pie_data.Length; ++i)
        {
            GameObject pie = ConstructPie(i * height, height, pie_data[i, 0], pie_data[i, 1], pie_data[i, 2]);
            pie.name = "pie_" + i.ToString();
            pie.transform.parent = rbv.transform;
        }

    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
