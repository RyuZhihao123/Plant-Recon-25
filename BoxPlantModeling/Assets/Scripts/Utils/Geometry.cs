using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geometry
{
    /// <summary>
    /// �õ�ĳ����������ⴹֱ����
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static Vector3 GetOneNormalVectorFrom(Vector3 dir)
    {
        if (dir.x == 0)
            return new Vector3(0, 0, -1);
        else
            return new Vector3(-dir.z / dir.x, 0, 1).normalized;
    }


    // �õ�ĳ�������Աߵ�һ����ת����
    public static Vector3 GetOneSideVectorFrom(Vector3 dir, float startAng = 30.0f, float endAng = 90.0f)
    {
        Vector3 _dir = dir.normalized;
        Vector3 _norm = Geometry.GetOneNormalVectorFrom(_dir);

        _norm = Quaternion.AngleAxis(Random.Range(1.0f, 359.0f), _dir) * _norm;  // �����תһ���Ƕ�

        float alpha = Random.Range(startAng, endAng) / 90.0f;

        return ((1.0f - alpha) * _dir + alpha * _norm).normalized;
    }

    /// <summary>
    /// ����һ��Ԥ������ָ����λ��
    /// </summary>
    public static GameObject CreatePrefab(string filename, Vector3 pt, Quaternion rotation, Vector3 scale)
    {

        GameObject instance = (GameObject)GameObject.Instantiate(
            Resources.Load(filename), pt, rotation);

        instance.transform.localScale = scale;

        return instance;
    }

    // ���ɾ�״��Ҷ��
    // (pt��dir)�ǵײ��е������ͳ���(h,w)��Ҷ�ӵĳߴ�
    // horAngle/verAng���ƾ���
    // slides �ָ�Ƭ����ע��hor�ǰ�����Ŀ��ver�������������Ŀ��
    public static Mesh CreateLeaf(
        Leaf leaf, int slidesHor = 4, int slidesVer = 8)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vec = new List<Vector3>();
        List<Vector3> norm = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        {
            Vector3 dir = leaf.dir;
            dir.Normalize();  // ������
            Vector3 left = Vector3.left;  // ����
            if (dir != Vector3.up)
                left = Vector3.Cross(Vector3.up, dir).normalized;

            // Ӧ����Ԫ����ת
            dir = leaf.rotation * dir;
            left = leaf.rotation * left;

            if (leaf.isCrossShape == true)
            {
                // ��һ����
                //var (vec_faceA, norm_faceA, uvs_faceA, idx_faceA)
                //    = Geometry.GenerateOneFace(leaf.a, dir, left, leaf.w, leaf.h, 0);

                //vec.AddRange(vec_faceA);
                //norm.AddRange(norm_faceA);
                //uvs.AddRange(uvs_faceA);
                //indices.AddRange(idx_faceA);

                //// ��ת90deg�����
                //var (vec_faceB, norm_faceB, uvs_faceB, idx_faceB)
                //    = Geometry.GenerateOneFace(leaf.a, dir, Vector3.Cross(dir, left).normalized, leaf.w, leaf.h, 8);

                //vec.AddRange(vec_faceB);
                //norm.AddRange(norm_faceB);
                //uvs.AddRange(uvs_faceB);
                //indices.AddRange(idx_faceB);

                // ��ת90deg�����
                Vector3 right = Vector3.Cross(dir, left).normalized;
                int count = 2; // Ĭ��1
                int index_count = 0;
                for (int i = 0; i <= count; i++)
                {
                    float alpha = (count - i) / (float)count;  // 1.0 - 0.0

                    {
                        Vector3 interpoloated_dirHor = alpha * left + (1.0f - alpha) * right;

                        var (vec_faceB, norm_faceB, uvs_faceB, idx_faceB)
                        = Geometry.GenerateOneFace(leaf.a, dir, interpoloated_dirHor.normalized, leaf.w, leaf.h, index_count);

                        index_count += 8;
                        vec.AddRange(vec_faceB);
                        norm.AddRange(norm_faceB);
                        uvs.AddRange(uvs_faceB);
                        indices.AddRange(idx_faceB);
                    }
                    if (i == 0 || i == count)
                        continue;
                    {
                        Vector3 interpoloated_dirHor = alpha * left - (1.0f - alpha) * right;
    
                        var (vec_faceB, norm_faceB, uvs_faceB, idx_faceB)
                        = Geometry.GenerateOneFace(leaf.a, dir, interpoloated_dirHor.normalized, leaf.w, leaf.h, index_count);

                        index_count += 8;

                        vec.AddRange(vec_faceB);
                        norm.AddRange(norm_faceB);
                        uvs.AddRange(uvs_faceB);
                        indices.AddRange(idx_faceB);
                    }
                }
            }
            else
            {

                if(Interactive_OP.p_isUseSubTree)
                {
                    left = Quaternion.AngleAxis(Random.Range(0, 180), dir) * left;
                }
                // ���д洢������
                List<List<Vector3>> rows = new List<List<Vector3>>();  // ���涥��
                List<List<Vector3>> rows_norm = new List<List<Vector3>>();  // ���淨��

                Vector3 curDir = dir;  // ��ǰ������
                Vector3 curPt = leaf.a;    // ��ǰ�е�����
                float xdelta = (leaf.w / slidesHor / 2f); // x�Ჽ��
                float ydelta = (leaf.h / slidesVer);      // y�Ჽ��

                for (int n = 0; n < slidesVer; ++n) // ��������
                {
                    Vector3 curLeft = left;     // ���еľֲ�����
                    Vector3 curRight = -left;   // ���еľֲ��ҷ���


                    var curRow = new List<Vector3>();  // �����µ�һ��
                    rows.Add(curRow);
                    var curRow_norm = new List<Vector3>();  // �����µ�һ��
                    rows_norm.Add(curRow_norm);

                    curRow.Add(curPt);  // �ѵ�ǰ�е�����ӽ���
                    curRow_norm.Add(Vector3.Cross(curDir, curLeft));

                    for (int m = 1; m < slidesHor; ++m)  // ������ǰ��
                    {
                        curLeft = Quaternion.AngleAxis(-leaf.horAng, curDir) * curLeft;  // �µľֲ�����
                        curRight = Quaternion.AngleAxis(leaf.horAng, curDir) * curRight; // �µľֲ��ҷ���
                        Vector3 newLeftPt = curRow[0] + curLeft * xdelta;                   // ��ǰ������
                        Vector3 newRightPt = curRow[curRow.Count - 1] + curRight * xdelta;  // ��ǰ���Ҳ��

                        curRow.Insert(0, newLeftPt);
                        curRow.Add(newRightPt);

                        curRow_norm.Insert(0, Vector3.Cross(curDir, curLeft).normalized);  // ������ķ���
                        curRow_norm.Add(Vector3.Cross(curDir, -curRight).normalized);
                    }



                    curDir = Quaternion.AngleAxis(leaf.verAng, left) * curDir;  // ��һ�е�������
                    curPt = curPt + curDir * ydelta;  // ��һ�е��е�����
                }

                // ������������(���棩
                int rowCount = rows[0].Count;
                for (int y = 0; y < rows.Count; ++y)
                {

                    for (int x = 0; x < rowCount; ++x)
                    {
                        vec.Add(rows[y][x]);
                        norm.Add(rows_norm[y][x]);
                        uvs.Add(new Vector2(x / (float)(rowCount - 1), y / (float)(rows.Count - 1)));
                        //Debug.Log("y:" + y.ToString() + " x:" + x.ToString() + " " + vec.Count.ToString());
                        if (y != rows.Count - 1 && x != rowCount - 1)
                        {
                            indices.Add(y * rowCount + x);
                            indices.Add(y * rowCount + x + 1);
                            indices.Add((y + 1) * rowCount + x);

                            indices.Add(y * rowCount + x + 1);
                            indices.Add((y + 1) * rowCount + x + 1);
                            indices.Add((y + 1) * rowCount + x);
                        }
                    }
                }

                // �����������񣨵��棩
                for (int y = 0; y < rows.Count; ++y)
                {
                    for (int x = 0; x < rowCount; ++x)
                    {
                        vec.Add(rows[y][x]);
                        norm.Add(-rows_norm[y][x]);
                        uvs.Add(new Vector2(1.0f - x / (float)(rowCount - 1), y / (float)(rows.Count - 1)));
                        //Debug.Log("y:" + y.ToString() + " x:" + x.ToString() + " " + vec.Count.ToString());
                        if (y != rows.Count - 1 && x != rowCount - 1)
                        {
                            indices.Add(y * rowCount + x);
                            indices.Add((y + 1) * rowCount + x);
                            indices.Add(y * rowCount + x + 1);

                            indices.Add(y * rowCount + x + 1);
                            indices.Add((y + 1) * rowCount + x);
                            indices.Add((y + 1) * rowCount + x + 1);
                        }
                    }
                }
            }
        }
        mesh.vertices = vec.ToArray();
        mesh.normals = norm.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = indices.ToArray();
        return mesh;
    }

    public static (List<Vector3>, List<Vector3>, List<Vector2>, List<int>)
        GenerateOneFace(Vector3 pt, Vector3 dir, Vector3 left, float w, float h, int startIndicesID)
    {
        List<Vector3> vec = new List<Vector3>();
        List<Vector3> norm = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();


        Vector3 v0 = pt - left * w / 2;
        Vector3 v1 = pt + left * w / 2;
        Vector3 v2 = pt + dir * h - left * w / 2;
        Vector3 v3 = pt + dir * h + left * w / 2;


        Vector3 cur_norm = Vector3.Cross(dir, left).normalized;

        Vector2 uv0 = new Vector2(0.0f, 0.0f);
        Vector2 uv1 = new Vector2(1.0f, 0.0f);
        Vector2 uv2 = new Vector2(0.0f, 1.0f);
        Vector2 uv3 = new Vector2(1.0f, 1.0f);

        // top face
        vec.Add(v0); vec.Add(v1); vec.Add(v2); vec.Add(v3);
        uvs.Add(uv0); uvs.Add(uv1); uvs.Add(uv2); uvs.Add(uv3);
        norm.Add(cur_norm); norm.Add(cur_norm); norm.Add(cur_norm); norm.Add(cur_norm);

        indices.Add(startIndicesID + 0); indices.Add(startIndicesID + 2); indices.Add(startIndicesID + 1);
        indices.Add(startIndicesID + 1); indices.Add(startIndicesID + 2); indices.Add(startIndicesID + 3);
        // bot face
        vec.Add(v0); vec.Add(v1); vec.Add(v2); vec.Add(v3);
        uvs.Add(uv1); uvs.Add(uv0); uvs.Add(uv3); uvs.Add(uv2);
        //norm.Add(-cur_norm); norm.Add(-cur_norm); norm.Add(-cur_norm); norm.Add(-cur_norm);
        norm.Add(cur_norm); norm.Add(cur_norm); norm.Add(cur_norm); norm.Add(cur_norm);
        indices.Add(startIndicesID + 0 + 4); indices.Add(startIndicesID + 1 + 4); indices.Add(startIndicesID + 2 + 4);
        indices.Add(startIndicesID + 1 + 4); indices.Add(startIndicesID + 3 + 4); indices.Add(startIndicesID + 2 + 4);

        return (vec, norm, uvs, indices);
    }

    //����graphģʽ�µ�����
    public static GameObject GetHierarchicalGraph(Plant plant)
    {
        float linewidth = 0.02f;
        float sphereRadius = 0.08f;


        GameObject plant_graph = new GameObject("Plant_Graph");

        // first: construct skeleton structure
        Queue<Internode> queue = new Queue<Internode>();
        queue.Enqueue(plant.root);

        List<Vector3> skeleton_lines = new List<Vector3>();

        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();
            Internode next = cur;
            while (true)
            {
                // Looking for either the "end nodes" or "branching nodes".
                if (next.childs.Count == 0 || next.childs.Count > 1 || next.kits.Count != 0)
                {
                    skeleton_lines.Add(cur.a);
                    skeleton_lines.Add(next.b);

                    foreach (var child in next.childs)
                    {
                        queue.Enqueue(child);
                    }
                    break;
                }
                else  // only one child node.
                {
                    next = next.childs[0];
                    continue;
                }
            }
        }

        //// ����Ҷ�Ӻͻ���
        //queue.Enqueue(plant.root);

        //while (queue.Count != 0)
        //{
        //    Internode cur = queue.Dequeue();

        //    for (int i = 0; i < cur.kits.Count; ++i)
        //    {
        //        if (cur.kits[i].GetType() == typeof(Leaf))  // �����Ҷ��
        //        {
        //            Leaf lf = (Leaf)cur.kits[i];

        //            Vector3 lf_pt = lf.a + (lf.rotation * lf.dir).normalized * lf.h / 2;
        //            //Vector3 lf_pt = lf.a + (cur.b - cur.a).normalized * lf.h / 3;

        //            // ����linerenderer������
        //            GameObject gObject = Geometry.CreateLineRenderer(
        //                "kitline_" + i.ToString(), lf.a, lf_pt,
        //                Color.black, linewidth/2);

        //            gObject.transform.parent = plant_graph.transform;

        //            //����Kit���
        //            GameObject kit_model = Geometry.CreateSphere(
        //                "kit_" + i.ToString(),  lf_pt, sphereRadius,
        //                Color.black);

        //            kit_model.transform.parent = plant_graph.transform;
        //        }
        //        if (cur.kits[i].GetType() == typeof(Flower))  // ����ǻ���
        //        {
        //            Flower lf = (Flower)cur.kits[i];

        //            // ���ڲ���ģ�͵�������ͬ�����Կ�����Ҫ���������up forward����
        //            //Vector3 lf_pt = lf.a + (lf.rotation * Vector3.up).normalized * lf.local_scale.x / 2;
        //            //Vector3 lf_pt = lf.a + (lf.rotation * Vector3.forward).normalized * lf.local_scale.x / 2;
        //            Vector3 lf_pt = lf.a + (cur.b - cur.a).normalized * lf.local_scale.x / 2;

        //            // ����linerenderer������
        //            GameObject gObject = Geometry.CreateLineRenderer(
        //                "kitline_" + i.ToString(), lf.a, lf_pt,
        //                Color.black, linewidth/2);

        //            gObject.transform.parent = plant_graph.transform;

        //            //����Kit���
        //            GameObject kit_model = Geometry.CreateSphere(
        //                "kit_" + i.ToString(), lf_pt, sphereRadius,
        //                Color.black);

        //            kit_model.transform.parent = plant_graph.transform;
        //        }
        //    }

        //    foreach (var child in cur.childs)
        //    {
        //        queue.Enqueue(child);
        //    }
        //}

        for (int i = 0; i < skeleton_lines.Count; i += 2)
        {
            // ����linerenderer������
            GameObject gObject = Geometry.CreateLineRenderer(
                "branch_" + i.ToString(), skeleton_lines[i + 0], skeleton_lines[i + 1],
                Color.gray, linewidth);

            gObject.transform.parent = plant_graph.transform;

            // ����С����
            if (i == 0)  // ���ڵ�λ��
            {
                var nodeObj1 = Geometry.CreateSphere("node_" + i.ToString(), skeleton_lines[0],
                    sphereRadius, Color.green);
                nodeObj1.transform.parent = plant_graph.transform;
            }
            var nodeObj2 = Geometry.CreateSphere("node_" + i.ToString(), skeleton_lines[i + 1],
                sphereRadius, Color.green);
            nodeObj2.transform.parent = plant_graph.transform;
        }

        return plant_graph;
    }



    public static GameObject GetMyStructureNetGraph_GameObject(Plant plant, int curlevel = 0)
    {
        // ����ͳ��һ����ֵoh�����Բ��õ���
        {
            int count_leaf = 0;
            int count_branch = 0;
            int count_flower = 0;
            foreach (var item in plant.m_hierarchy_boxes)
            {
            
                if (item.Value.Item2 == false)  // ����ǲ�����չ���
                {
                    if (item.Key.boxType == MinBoundingBox.BoxType.leaf)
                        count_leaf++;
                    if (item.Key.boxType == MinBoundingBox.BoxType.flower)
                        count_flower++;
                    if (item.Key.boxType == MinBoundingBox.BoxType.endBranch)
                        count_branch++;
                }
            }

            TCP_Client.SendMessage("A " + string.Format("Staticstics:(leaf={0}, flower={1}, branch={2}), sum={3}",
                count_leaf,count_flower,count_branch,
                (count_leaf+count_flower+count_branch)));
        }
        // �µ�Bounding box����ɫ���ʴ���
        if (Global_Materials.m_new_box_material_mode == true)
        {
            GameObject plant_graph = new GameObject("Plant_Graph");

            if (plant.max_box_level == -1)
                return plant_graph;

            foreach (var item in plant.m_hierarchy_boxes)
            {
                if (item.Value.Item1 > curlevel)  // �������
                    continue;
                if (item.Value.Item2 == true && item.Value.Item1 < curlevel)  // ���С��curlevel�Ŀ���չ���
                    continue;

                // item.Key -> item.Value
                GameObject box_object = item.Key.GetGameObject(usingAbsolutePosition: false);  //

                box_object.GetComponent<Renderer>().material
                    = Global_Materials.GetMaterialFromBoxType(item.Key.boxType);

                // ������????
                box_object.GetComponent<Renderer>().material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
                box_object.GetComponent<Renderer>().material.SetFloat("_SpecularHighlights", 0f);

                box_object.transform.parent = plant_graph.transform;
            }
            return plant_graph;
        }
        // �ɵ�bounding box����ɫ���ʴ���
        else
        {
            GameObject plant_graph = new GameObject("Plant_Graph");

            if (plant.max_box_level == -1)
                return plant_graph;

            // Ĭ����ɫ����
            Color[] colors = new Color[] {
            //new Color(0.2f, 0.7f, 0.2f, 0.5f),  // Ҷ��
            new Color(181.0f/255.0f, 255.0f/255.0f, 144.0f/255.0f, 0.5f),  // Ҷ�� 181.0f/255.0f, 255.0f/255.0f, 144.0f/255.0f, 0.5f
            new Color(0.9f, 0.0f, 0.0f, 0.5f),  // �� 0.9f, 0.0f, 0.0f, 0.5f
            new Color(0.8f, 0.6f, 0.8f, 0.5f),  // ĩ-֦��
            new Color(0.2f, 0.4f, 0.4f, 0.5f),  // �м�-box(����չ)
            new Color(0.4f, 0.5f, 0.0f, 0.5f),
            new Color(0.2f, 0.5f, 0.1f, 0.5f),
            new Color(0.3f, 0.3f, 0.5f, 0.5f),
            new Color(0.5f, 0.0f, 0.5f, 0.5f),
            new Color(0.5f, 0.5f, 0.0f, 0.5f),
        };

            // JsonDatasetGenerator.JsonNode plant_json = JsonDatasetGenerator.SaveJson2(plant, "");

            foreach (var item in plant.m_hierarchy_boxes)
            {
                if (item.Value.Item1 > curlevel)  // �������
                    continue;
                if (item.Value.Item2 == true && item.Value.Item1 < curlevel)  // ���С��curlevel�Ŀ���չ���
                    continue;

                // item.Key -> item.Value
                GameObject box_object = item.Key.GetGameObject();
                box_object.GetComponent<Renderer>().material.SetColor("_Color", colors[(int)item.Key.boxType]);
                //box_object.GetComponent<Renderer>().material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
                //box_object.GetComponent<Renderer>().material.SetFloat("_SpecularHighlights", 0f);
                box_object.transform.parent = plant_graph.transform;
            }

            return plant_graph;
        }


    }

    public static GameObject CreateLineRenderer(string name, Vector3 p1, Vector3 p2, Color color, float linewidth)
    {
        GameObject gObject = new GameObject(name);
        LineRenderer lRend = gObject.AddComponent<LineRenderer>();

        lRend.material = new Material(Shader.Find("Sprites/Default"));
        lRend.startWidth = lRend.endWidth = linewidth;
        lRend.startColor = lRend.endColor = color;
        lRend.SetPosition(0, p1);
        lRend.SetPosition(1, p2);

        return gObject;
    }

    public static GameObject CreateSphere(string name, Vector3 pt, float localscale, Color color)
    {
        var nodeObj1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nodeObj1.name = name;
        nodeObj1.GetComponent<Renderer>().material.color = color;
        nodeObj1.GetComponent<Transform>().position = pt;
        nodeObj1.GetComponent<Transform>().localScale = new Vector3(localscale, localscale, localscale);

        return nodeObj1;
    }

    public static GameObject CreateCube(string name, Vector3 pt, float localscale, Color color)
    {
        var nodeObj1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nodeObj1.name = name;
        nodeObj1.GetComponent<Renderer>().material.color = color;
        nodeObj1.GetComponent<Transform>().position = pt;
        nodeObj1.GetComponent<Transform>().localScale = new Vector3(localscale, localscale, localscale);

        return nodeObj1;
    }
}

/// <summary>
/// Bounding box Helper
/// </summary>

public class MinBoundingBox
{

    public enum BoxType { leaf, flower, endBranch, intermediateBox };

    public BoxType boxType = BoxType.intermediateBox;  // ���ӵ�����

    public Vector3 dir1;  // ��Ҫ����(���䷽��)
    public Vector3 dir2;  // ��������2
    public Vector3 dir3;  // ��������3

    public float len1, len2, len3; // ����
    public Vector3 center;  // ���ӵ���������

    public List<MinBoundingBox> children;


    // ���е�Ԥ����leaf�ͻ������Ϣ
    public static Dictionary<string, MinBoundingBox> init_flower_bbox = new Dictionary<string, MinBoundingBox>(); // ע��name����.��׺
    public static Dictionary<string, (float, float)> init_leaf_ratio = new Dictionary<string, (float, float)>(); // ע��name����.��׺������ķֱ�Ϊ��͸ߵ�����ratio

    public List<Vector3> GetVertices()   // ֱ�ӵõ����Ե�Box�Ķ���
    {
        List<Vector3> vertices = new List<Vector3>();

        Vector3 d1 = 0.5f * len1 * dir1;
        Vector3 d2 = 0.5f * len2 * dir2;
        Vector3 d3 = 0.5f * len3 * dir3;

        Vector3 cornerpoint0 = center - d1 - d2 - d3;
        Vector3 cornerpoint1 = center - d1 + d2 - d3;
        Vector3 cornerpoint2 = center + d1 - d2 - d3;
        Vector3 cornerpoint3 = center + d1 + d2 - d3;
        Vector3 cornerpoint4 = center - d1 - d2 + d3;
        Vector3 cornerpoint5 = center - d1 + d2 + d3;
        Vector3 cornerpoint6 = center + d1 - d2 + d3;
        Vector3 cornerpoint7 = center + d1 + d2 + d3;

        vertices.Add(cornerpoint0); vertices.Add(cornerpoint1); vertices.Add(cornerpoint2);
        vertices.Add(cornerpoint3); vertices.Add(cornerpoint4); vertices.Add(cornerpoint5);
        vertices.Add(cornerpoint6); vertices.Add(cornerpoint7);

        return vertices;
    }

    public List<Vector3> GetVertices_WithoutRotation()   // ֱ�ӵõ�û��Ӧ����ת��Box�Ķ���
    {
        List<Vector3> vertices = new List<Vector3>();

        Vector3 d1 = 0.5f * len1 * Vector3.up;
        Vector3 d2 = 0.5f * len2 * Vector3.forward;
        Vector3 d3 = 0.5f * len3 * Vector3.right;
        Vector3 c = Vector3.zero;
        Vector3 cornerpoint0 = c - d1 - d2 - d3;
        Vector3 cornerpoint1 = c - d1 + d2 - d3;
        Vector3 cornerpoint2 = c + d1 - d2 - d3;
        Vector3 cornerpoint3 = c + d1 + d2 - d3;
        Vector3 cornerpoint4 = c - d1 - d2 + d3;
        Vector3 cornerpoint5 = c - d1 + d2 + d3;
        Vector3 cornerpoint6 = c + d1 - d2 + d3;
        Vector3 cornerpoint7 = c + d1 + d2 + d3;

        vertices.Add(cornerpoint0); vertices.Add(cornerpoint1); vertices.Add(cornerpoint2);
        vertices.Add(cornerpoint3); vertices.Add(cornerpoint4); vertices.Add(cornerpoint5);
        vertices.Add(cornerpoint6); vertices.Add(cornerpoint7);

        return vertices;
    }

    public GameObject GetGameObject(bool usingAbsolutePosition = true)
    {
        if (Global_Materials.m_new_box_material_mode == true)
        {
            GameObject game_object = GameObject.CreatePrimitive(PrimitiveType.Cube);

            List<Vector3> obj_vertices = null;
            if (usingAbsolutePosition == true)  // ���ֱ��ʹ�þ��Ե�����
                obj_vertices = this.GetVertices();
            else
                obj_vertices = this.GetVertices_WithoutRotation(); // �����ʹ�þ������꣬����Ӧ����ת��
            List<Vector3> vec = new List<Vector3>();
            List<Vector3> norm = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> indices = new List<int>();

            int[] triangles = {
                0, 2, 1, //face front
                0, 1, 2,
                1, 2, 3,
                1, 3, 2,
                2, 3, 7, //face top
                2, 7, 3,
                2, 6, 7,
                2, 7, 6,
                4, 6, 7, //face right
                4, 7, 6,
                4, 5, 7,
                4, 7, 5,
                1, 3, 7, //����
			    1, 7, 3,
                1, 5, 7,
                1, 7, 5,
                0, 6, 2, // ����
			    0, 2, 6,
                0, 4, 6,
                0, 6, 4
            };

            {
                Vector3 c = Vector3.zero;  // ��������
                if (usingAbsolutePosition == true)  // ���ʹ�þ�����������
                {
                    c = center;
                }
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    Vector3 v0 = obj_vertices[triangles[i + 0]];
                    Vector3 v1 = obj_vertices[triangles[i + 1]];
                    Vector3 v2 = obj_vertices[triangles[i + 2]];

                    Vector3 _n = Vector3.Cross(v2 - v1, v0 - v1).normalized;

                    if (Vector3.Dot(_n, v0 - c) < 0)
                        _n = -_n;

                    vec.Add(v0); vec.Add(v1); vec.Add(v2);
                    norm.Add(_n); norm.Add(_n); norm.Add(_n);
                    indices.Add(i + 0); indices.Add(i + 1); indices.Add(i + 2);
                }
            }


            game_object.name = "JsonNodeBox";
            game_object.GetComponent<Renderer>().material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            game_object.GetComponent<Renderer>().material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            game_object.GetComponent<Renderer>().material.SetInt("_ZWrite", 0);
            game_object.GetComponent<Renderer>().material.DisableKeyword("_ALPHATEST_ON");
            game_object.GetComponent<Renderer>().material.DisableKeyword("_ALPHABLEND_ON");
            game_object.GetComponent<Renderer>().material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            game_object.GetComponent<Renderer>().material.renderQueue = 3000;

            game_object.GetComponent<Transform>().position = Vector3.zero;

            if (usingAbsolutePosition == false) // �����Ӧ�þ�����������
            {
                game_object.GetComponent<Transform>().position = center;
                game_object.GetComponent<Transform>().rotation = Quaternion.LookRotation(dir2, dir1);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vec.ToArray();
            mesh.triangles = indices.ToArray();
            mesh.normals = norm.ToArray();
            mesh.RecalculateTangents();
            game_object.GetComponent<MeshFilter>().mesh = mesh;

            return game_object;
        }
        else
        {
            GameObject game_object = GameObject.CreatePrimitive(PrimitiveType.Cube);

            List<Vector3> vec = this.GetVertices();
            List<Vector3> norm = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            int[] triangles = {
                0, 2, 1, //face front
                0, 1, 2,
                1, 2, 3,
                1, 3, 2,
                2, 3, 7, //face top
                2, 7, 3,
                2, 6, 7,
                2, 7, 6,
                4, 6, 7, //face right
                4, 7, 6,
                4, 5, 7,
                4, 7, 5,
                1, 3, 7, // ����
			    1, 7, 3,
                1, 5, 7,
                1, 7, 5,
                0, 6, 2, // ����
			    0, 2, 6,
                0, 4, 6,
                0, 6, 4
            };

            Mesh mesh = new Mesh();
            mesh.vertices = vec.ToArray();
            mesh.triangles = triangles;
            //mesh.Optimize();
            //mesh.RecalculateNormals();
            game_object.GetComponent<MeshFilter>().mesh = mesh;
            game_object.name = "JsonNodeBox";
            game_object.GetComponent<Renderer>().material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            game_object.GetComponent<Renderer>().material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            game_object.GetComponent<Renderer>().material.SetInt("_ZWrite", 0);
            game_object.GetComponent<Renderer>().material.DisableKeyword("_ALPHATEST_ON");
            game_object.GetComponent<Renderer>().material.DisableKeyword("_ALPHABLEND_ON");
            game_object.GetComponent<Renderer>().material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            game_object.GetComponent<Renderer>().material.renderQueue = 3000;

            game_object.GetComponent<Transform>().position = Vector3.zero;

            return game_object;
        }
    }



    static public MinBoundingBox GetMinBBoxFromVertexList(Vector3 root, Vector3[] vertexArray)
    {
        float threshold_width = 0.06f;  // Ĭ��0.03f
        MinBoundingBox bbox = new MinBoundingBox();

        if (vertexArray.Length == 0) // bug: �Ե���0���⴦��
            return bbox;

        // [1] ���ȼ���һ��"�����Ȩ"��ƽ��������Ϊ������

        Vector3 mainDir = Vector3.zero;           // dir1
        for (int i = 0; i < vertexArray.Length; ++i)
            mainDir += (vertexArray[i] - root);

        mainDir.Normalize();

        float len1, len2, len3;   // ������������곤��
        len1 = len2 = len3 = 0.0f;

        // �������ˣ�(root��mainDir).
        // [2] ��ʼ������ÿ���������󵽵����ͶӰ��
        Vector3[] projected_points = new Vector3[vertexArray.Length];          // ͶӰ�������position
        float[] projected_lengths_to_root = new float[vertexArray.Length];     // ͶӰ�㵽root�ľ���
        Vector3[] normalized_radial_dir = new Vector3[vertexArray.Length];     // ͶӰ���ڵ����ϵľ�������ķ�������

        for (int i = 0; i < vertexArray.Length; ++i)
        {
            projected_points[i] = MinBoundingBox.PointToPlane(vertexArray[i], root, mainDir);
            projected_lengths_to_root[i] = Vector3.Distance(projected_points[i], root);
            normalized_radial_dir[i] = (projected_points[i] - root).normalized;
        }

        Vector3 secondary_dir = Vector3.zero;  // dir2

        // Ȼ���ÿ��ͶӰ�㣬���Ӧ��ֱ�����ȣ��ȼ���õ�ľ��룬���ڼн�>90��ĵ�������һ������ļ��㣩
        {

            Vector3 max_pt = root;
            Vector3 min_pt = root;
            double tmp_max_length = double.MinValue;  // �����ֱ��(������ֵ���dist)
            for (int i = 0; i < projected_points.Length; ++i)
            {
                double length = projected_lengths_to_root[i];  // һ��ĳ���

                // �������㣬����Ѱ�������չ����
                double max = 0;
                for (int k = 0; k < projected_points.Length; ++k)
                {
                    if (i == k) continue;

                    float dot_product = Vector3.Dot(
                        normalized_radial_dir[i],
                        projected_points[k] - root);

                    if (dot_product < 0 && max < (-dot_product))
                    {
                        max = -dot_product;
                    }
                }

                length += max;  // ������һ��ĳ���

                if (tmp_max_length < length)
                {
                    tmp_max_length = length;
                    max_pt = projected_points[i];

                    secondary_dir = normalized_radial_dir[i].normalized;
                }
            }

            Vector3 _c = max_pt - (float)(0.5f * tmp_max_length) * secondary_dir;

            root = _c;

            len2 = (float)tmp_max_length;

            if (len2 < threshold_width)   // �����������̫ϸ��
                len2 = threshold_width;

            // ���secondary_dir��������������������ֱ����
            if (Vector3.Distance(secondary_dir, Vector3.zero) < 0.0001f)
                secondary_dir = Geometry.GetOneNormalVectorFrom(mainDir);
        }

        Vector3 third_dir = Vector3.Cross(mainDir, secondary_dir).normalized;
        // dir3 
        {
            double min = double.MaxValue;
            double max = double.MinValue;

            Vector3 min_pt = root;
            Vector3 max_pt = root;

            for (int i = 0; i < projected_points.Length; ++i)
            {
                double dotproduct = Vector3.Dot(
                    third_dir,
                    projected_points[i] - root);

                if (dotproduct < min)
                {
                    min = dotproduct;
                    min_pt = root + third_dir * (float)min;
                }
                if (dotproduct > max)
                {
                    max = dotproduct;
                    max_pt = root + third_dir * (float)max;
                }
            }
            Vector3 _c = 0.5f * (max_pt + min_pt);
            root = _c;

            len3 = (max_pt - min_pt).magnitude;

            if (len3 < threshold_width)   // �����������̫ϸ��
                len3 = threshold_width;
        }

        // ��root�ƶ���box��y������
        {
            double min = double.MaxValue;
            double max = double.MinValue;

            Vector3 min_pt = root;
            Vector3 max_pt = root;

            for (int i = 0; i < vertexArray.Length; ++i)
            {
                double dotproduct = Vector3.Dot(
                    mainDir,
                    vertexArray[i] - root);

                if (dotproduct < min)
                {
                    min = dotproduct;
                    min_pt = root + mainDir * (float)min;
                }
                if (dotproduct > max)
                {
                    max = dotproduct;
                    max_pt = root + mainDir * (float)max;
                }
            }
            Vector3 _c = 0.5f * (max_pt + min_pt);
            root = _c;
            len1 = (max_pt - min_pt).magnitude;

            if (len1 < threshold_width)   // �����������̫ϸ��
                len1 = threshold_width;
        }


        //TCP_Client.SendMessage(string.Format("A len={0} {1} {2}", len1, len2, len3));

        // ���ֱ��������Ϊ�ڶ�����
        // ��������Ϊcross(�����򣬵ڶ�����)
        bbox.dir1 = mainDir;
        bbox.dir2 = secondary_dir;
        bbox.dir3 = third_dir;

        bbox.center = root;

        bbox.len1 = len1;
        bbox.len2 = len2;
        bbox.len3 = len3;

        return bbox;
    }

    static public MinBoundingBox Initilize_Flower_BBOX_in_YXZaxis(Vector3[] vertexArray)
    {
        MinBoundingBox bbox = new MinBoundingBox();

        bbox.dir1 = Vector3.up;      // (0,1,0) y��
        bbox.dir2 = Vector3.right;   // (1,0,0) x��
        bbox.dir3 = Vector3.forward; // (0,0,1) z��

        float maxx = float.MinValue, maxy = float.MinValue, maxz = float.MinValue;
        float minx = float.MaxValue, miny = float.MaxValue, minz = float.MaxValue;

        foreach (var pt in vertexArray)
        {
            float x = pt.x; float y = pt.y; float z = pt.z;
            if (minx > x) minx = x;
            if (miny > y) miny = y;
            if (minz > z) minz = z;

            if (maxx < x) maxx = x;
            if (maxy < y) maxy = y;
            if (maxz < z) maxz = z;
        }

        bbox.center = new Vector3((maxx + minx) / 2.0f, (maxy + miny) / 2.0f, (maxz + minz) / 2.0f);

        bbox.len1 = maxy - miny;
        bbox.len2 = maxx - minx;
        bbox.len3 = maxz - minz;

        return bbox;
    }

    // ��game object�л�����е�vertices���ڳ�ʼ����ģ��ʱ���õ��ˣ�
    static public List<Vector3> GetVertexList_FromGameObject(GameObject obj)
    {
        List<Vector3> globalVertices = new List<Vector3>();

        Transform[] myTransforms = obj.GetComponentsInChildren<Transform>();

        foreach (var transform in myTransforms)
        {
            GameObject go = transform.gameObject;

            if (go.GetComponent<MeshFilter>() != null)
            {
                Vector3[] vertices = go.GetComponent<MeshFilter>().mesh.vertices;

                Matrix4x4 localToWorld = go.GetComponent<Transform>().localToWorldMatrix;

                foreach (var ver in vertices)
                {
                    globalVertices.Add(localToWorld.MultiplyPoint3x4(ver));
                }
            }
        }

        return globalVertices;
    }

    // �����p��ƽ���ͶӰp'
    // Ȼ���ÿ��p'��һ�����ȣ��õ㵽ֱ�ߵ�ͶӰ) ѡȡ���

    static private Vector3 PointToPlane(Vector3 worldpos, Vector3 targetPlanePos, Vector3 planeNormal)
    {
        var localpos = worldpos - targetPlanePos;
        var dis = Vector3.Dot(localpos, planeNormal);
        var vecN = planeNormal * dis;
        return worldpos - vecN;
    }



    // ������Internode�й���BBOX
    static public MinBoundingBox GetMinBBoxFromSubTree(Internode rt,
        Dictionary<Kit, MinBoundingBox> flower_boxes = null,
        Dictionary<Kit, MinBoundingBox> leaf_boxes = null)
    {
        MinBoundingBox bbox = new MinBoundingBox();

        Queue<Internode> queue = new Queue<Internode>();
        queue.Enqueue(rt);

        List<Vector3> vertices = new List<Vector3>();
        while (queue.Count != 0)
        {
            var cur = queue.Dequeue();

            vertices.Add(cur.a);

            if (cur.childs.Count == 0)
                vertices.Add(cur.b);

            foreach (var kit in cur.kits)
            {

                if (typeof(Flower) == kit.GetType() && flower_boxes != null)
                    vertices.AddRange(flower_boxes[kit].GetVertices());
                if (typeof(Leaf) == kit.GetType() && leaf_boxes != null)
                    vertices.AddRange(leaf_boxes[kit].GetVertices());
            }

            foreach (var child in cur.childs)
                queue.Enqueue(child);
        }

        return MinBoundingBox.GetMinBBoxFromVertexList(rt.a, vertices.ToArray());
    }
    static public (Dictionary<MinBoundingBox, (int, bool)>, int) GetMinBBoxFromTreeRoot(Internode rt)
    {
        Dictionary<Kit, MinBoundingBox> flower_boxes = new Dictionary<Kit, MinBoundingBox>();
        Dictionary<Kit, MinBoundingBox> leaf_boxes = new Dictionary<Kit, MinBoundingBox>();

        {
            // ���ȴ���һ�»���
            Queue<Internode> queue = new Queue<Internode>();
            queue.Enqueue(rt);
            while (queue.Count != 0)
            {
                var cur = queue.Dequeue();

                foreach (var child in cur.childs)
                    queue.Enqueue(child);
                foreach (var kit in cur.kits)
                {
                    if (typeof(Flower) == kit.GetType())
                    {
                        Flower flower = (Flower)kit;
                        var bbox = MinBoundingBox.GetMinBBoxFromFlower((Flower)kit);
                        bbox.boxType = BoxType.flower;

                        if (flower.flowerName == "leaf_Բ��Ǯ��" ||
                            flower.flowerName == "flower_��׻�_Ҷ")
                            bbox.boxType = BoxType.leaf;
                        
                        flower_boxes.Add(kit, bbox);
                    }
                    if (typeof(Leaf) == kit.GetType())
                    {
                        var bbox = MinBoundingBox.GetMinBBoxFromLeaf((Leaf)kit);
                        bbox.boxType = BoxType.leaf;
                        leaf_boxes.Add(kit, bbox);
                    }
                }
            }
        }
        // Ȼ��ͳ��һ�� ÿ���ڵ��level��Ϣ (int-level, bool-�Ƿ����չ)
        Dictionary<Internode, int> internodes_levels = new Dictionary<Internode, int>();
        Dictionary<MinBoundingBox, (int, bool)> bbox_levels = new Dictionary<MinBoundingBox, (int, bool)>();
        {
            Queue<Internode> queue = new Queue<Internode>();

            queue.Enqueue(rt);
            internodes_levels.Add(rt, 0);
            bbox_levels.Add(MinBoundingBox.GetMinBBoxFromSubTree(rt, flower_boxes, leaf_boxes), (0, true));


            while (queue.Count != 0)
            {
                Internode cur = queue.Dequeue();
                int curLevel = internodes_levels[cur];

                // ����ֱ������������ײ�
                List<Vector3> curlevel_branch_vertices = new List<Vector3>();
                while (true)
                {
                    // ��cur�����л����ӽ���
                    foreach (var kit in cur.kits)
                    {
                        if (typeof(Flower) == kit.GetType())
                            bbox_levels.Add(flower_boxes[kit], (curLevel + 1, false));
                        if (typeof(Leaf) == kit.GetType())
                            bbox_levels.Add(leaf_boxes[kit], (curLevel + 1, false));
                    }

                    // ��cur�Ķ���a�ý���
                    curlevel_branch_vertices.Add(cur.a);

                    if (cur.childs.Count == 1)
                    {
                        cur = cur.childs[0];
                        continue;
                    }

                    if (cur.childs.Count != 1) // cur�ĺ��ӵ�������Ϊ1������β����
                    {
                        // ��������stem��bbox
                        curlevel_branch_vertices.Add(cur.b);

                        var bbox = MinBoundingBox.GetMinBBoxFromVertexList(curlevel_branch_vertices[0],
                                                                    curlevel_branch_vertices.ToArray());
                        bbox.boxType = BoxType.endBranch;
                        TCP_Client.SendMessage(string.Format("A endbranch {0}, {1}, {2}, {3}",
                            curlevel_branch_vertices.Count,
                            bbox.dir2.x, bbox.dir2.y, bbox.dir2.z));
                        bbox_levels.Add(bbox, (curLevel + 1, false));


                        for (int i = 0; i < cur.childs.Count; ++i) // ����cur�����еĺ���
                        {
                            // ���ȴ����ú��Ӷ�Ӧ������bbox
                            bbox_levels.Add(MinBoundingBox.GetMinBBoxFromSubTree(cur.childs[i], flower_boxes, leaf_boxes),
                                (curLevel + 1, true));

                            queue.Enqueue(cur.childs[i]); // ������ʶ���
                            internodes_levels.Add(cur.childs[i], curLevel + 1);
                        }

                        break;
                    }
                }
            }
        }
        int max_level = -1;
        foreach (var item in bbox_levels)
        {
            if (max_level < item.Value.Item1)
                max_level = item.Value.Item1;

        }
        return (bbox_levels, max_level);
    }


    static public MinBoundingBox GetMinBBoxFromFlower(Flower f)
    {
        Debug.Log("+++++++++++++++++++++++++++++");

        MinBoundingBox init_box = init_flower_bbox[f.flowerName];

        MinBoundingBox new_box = new MinBoundingBox();
        new_box.dir1 = f.rotation * init_box.dir1;// y��
        new_box.dir2 = f.rotation * init_box.dir2;// x��
        new_box.dir3 = f.rotation * init_box.dir3;// z��

        Debug.Log("+++++++++++++++++++++++++++++");
        new_box.center = f.a + f.rotation
            * new Vector3(init_box.center.x * f.local_scale.x,
            init_box.center.y * f.local_scale.y,
            init_box.center.z * f.local_scale.z);
        new_box.len1 = init_box.len1 * f.local_scale[1];
        new_box.len2 = init_box.len2 * f.local_scale[0];
        new_box.len3 = init_box.len3 * f.local_scale[2];

        new_box.boxType = BoxType.flower;
        Debug.Log(new_box.center.ToString());
        return new_box;
    }

    static public MinBoundingBox GetMinBBoxFromLeaf(Leaf leaf)
    {
        MinBoundingBox bbox = new MinBoundingBox();

        if (leaf.isCrossShape == true)
        {
            // ˫Ƭ����ҶƬ����״��
            Vector3 dir = leaf.dir;
            dir.Normalize();  // ������
            Vector3 left = Vector3.left;  // ����
            if (dir != Vector3.up)
                left = Vector3.Cross(Vector3.up, dir).normalized;
            float textureRatioX = 1.0f;
            if (MinBoundingBox.init_leaf_ratio.ContainsKey(leaf.textureName))
                textureRatioX = MinBoundingBox.init_leaf_ratio[leaf.textureName].Item1;

            // Ӧ����Ԫ����ת
            dir = leaf.rotation * dir;
            left = leaf.rotation * left;

            // first facelet
            bbox.dir1 = dir;
            bbox.dir2 = left;
            bbox.dir3 = Vector3.Cross(bbox.dir1, bbox.dir2).normalized;

            bbox.len1 = leaf.h;
            bbox.len2 = leaf.w * textureRatioX;
            bbox.len3 = leaf.w * textureRatioX;

            bbox.center = dir * leaf.h / 2.0f + leaf.a;
        }

        if (leaf.isCrossShape == false)
        {
            // ��Ƭ��Ҷ����״��
            // c f d
            //   e
            // a g b ����
            Vector3 a, b, c, d, e, f, g;
            a = b = c = d = e = f = g = Vector3.zero;

            int slidesHor = 4;
            int slidesVer = 8;   // �����������û�ж���ͷ��������΢����һ�¿�����box��ҶƬ��������һ��

            Vector3 dir = leaf.dir;
            dir.Normalize();  // ������
            Vector3 left = Vector3.left;  // ����
            if (dir != Vector3.up)
                left = Vector3.Cross(Vector3.up, dir).normalized;

            float textureRatioX = 1.0f;
            if (MinBoundingBox.init_leaf_ratio.ContainsKey(leaf.textureName))
                textureRatioX = MinBoundingBox.init_leaf_ratio[leaf.textureName].Item1;

            // Ӧ����Ԫ����ת
            dir = leaf.rotation * dir;
            left = leaf.rotation * left;

            // ���д洢������
            List<List<Vector3>> rows = new List<List<Vector3>>();  // ���涥��

            Vector3 curDir = dir;  // ��ǰ������
            Vector3 curPt = leaf.a;    // ��ǰ�е����꣨�S��"��row"�����׃����
            float xdelta = (leaf.w / slidesHor / 2f); // x�Ჽ��
            float ydelta = (leaf.h / slidesVer);      // y�Ჽ��

            for (int n = 0; n < slidesVer; ++n) // ��������
            {
                Vector3 curLeft = left;     // ���еľֲ�����
                Vector3 curRight = -left;   // ���еľֲ��ҷ���


                Vector3 curLeftPt = curPt;  // ��ǰ������
                Vector3 curRightPt = curPt; // ��ǰ���Ҳ��

                if (n == (slidesVer / 2))
                    e = curPt;

                if (n == 0) g = curPt;
                if (n == (slidesVer - 1)) f = curPt;

                for (int m = 1; m < slidesHor; ++m)  // ������ǰ��
                {
                    curLeft = Quaternion.AngleAxis(-leaf.horAng, curDir) * curLeft;  // �µľֲ�����
                    curRight = Quaternion.AngleAxis(leaf.horAng, curDir) * curRight; // �µľֲ��ҷ���
                    curLeftPt = curLeftPt + curLeft * xdelta;                   // ��ǰ������
                    curRightPt = curRightPt + curRight * xdelta;  // ��ǰ���Ҳ��

                    if (n == 0 && m == (slidesHor - 1)) // a b
                    {
                        a = curLeftPt;
                        b = curRightPt;
                    }
                    if (n == (slidesVer - 1) && m == (slidesHor - 1)) // c d
                    {
                        c = curLeftPt;
                        d = curRightPt;
                    }

                    // ��������Χ��⣺�Ƿ�û��Ҫ����X����:

                    float currentRatioX = m / (float)(slidesHor - 1);

                    //TCP_Client.SendMessage(string.Format("A {0},{1}", currentRatioX, textureRatioX));

                    if (currentRatioX > textureRatioX)
                    {
                        if (n == 0) // a b
                        {
                            a = curLeftPt;
                            b = curRightPt;
                        }
                        if (n == (slidesVer - 1)) // c d
                        {
                            c = curLeftPt;
                            d = curRightPt;
                        }
                        break;
                    }
                }

                curDir = Quaternion.AngleAxis(leaf.verAng, left) * curDir;  // ��һ�е�������
                curPt = curPt + curDir * ydelta;  // ��һ�е��е�����
            }
            Vector3 centerOfPlane = (a + b + c + d) / 4;


            // ����boundingbox
            bbox.dir1 = (c - a).normalized;
            bbox.dir2 = (b - a).normalized;
            bbox.dir3 = Vector3.Cross(bbox.dir1, bbox.dir2).normalized;

            if (Vector3.Dot(bbox.dir3, e - centerOfPlane) > 0)
                bbox.dir3 = -bbox.dir3;

            bbox.len1 = (f - g).magnitude;
            bbox.len2 = (b - a).magnitude;
            bbox.len3 = (e - centerOfPlane).magnitude;

            bbox.center = centerOfPlane - (bbox.len3 / 2.0f) * bbox.dir3;
        }

        bbox.boxType = BoxType.leaf;
        return bbox;
    }

    public string ToString()
    {
        return string.Format("center:({0}, {1}, {2})", center.x, center.y, center.z);
    }
}
