using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.IO;
using System.Text;
using System;


public class UTIL
{
    public static void PrintStringList(string[] list)
    {
        string str = "�����б�: " + list.Length.ToString() + ": ";

        foreach (string item in list)
            str += item + " ||| ";

        Debug.Log(str);
    }

    public static void PrintDictionary(Dictionary<string, float> dict)
    {
        string str = "�ֵ�(" + dict.Count.ToString() + "): ";

        foreach (var item in dict)
            str += item.Key + "=" + item.Value.ToString() + " ||| ";

        Debug.Log(str);
    }

    public static void PrintRule(Rule rule)
    {
        string str = "������: " + rule.label;

        str += "\n�����б�";

        foreach (var param in rule.param_list)
            str += param + " ";

        str += "\n����ʽ: \n";
        foreach (var seq in rule.sequences)
            str += seq + "\n";
        Debug.Log(str);
    }

    public static string SubString(string str, int startID, int endID)
    {
        // ������ʼID����ֹID�������ַ�
        int len = endID - startID + 1;
        return str.Substring(startID, len);
    }

    public static float ComputeMathExpression(string expression, Dictionary<string, float> variables, float t, float mt)
    {
        org.matheval.Expression exp = new org.matheval.Expression(expression);
        exp.Bind("t", t);
        exp.Bind("mt", mt);

        foreach (var kv in variables)
            exp.Bind(kv.Key, kv.Value);

        object answer = exp.Eval();
        return float.Parse(answer.ToString());
    }

    public static List<string> GetFollowedParams(string str, int sid, out int eid)
    {
        // sid��Ӧ���������ŵ�id������ǰ�ؼ��ֽ��������һ���ַ���
        string[] parameters = { };
        eid = sid;

        if (sid >= str.Length - 1)  // �Ѿ�����ĩβ
            return new List<string>();

        if (str[sid] == '(') // �������ĸ��������
        {
            int count = 0; // ͳ��֮�������������Ÿ���
            for (int i = sid + 1; i < str.Length; ++i) // �ҵ���һ�������ŵ�ID
            {
                if(str[i] == '(')
                {
                    count++;
                    continue;
                }
                if (str[i] == ')' && count != 0) 
                {
                    count--;
                    continue;
                }
                if (str[i] == ')' && count==0)
                {
                    eid = i;
                    break;
                }
            }

            parameters = UTIL.SubString(str, sid + 1, eid - 1).Split(",");
            eid += 1;
        }
        return new List<string>(parameters);
    }

    public static void PrintSubCommond(string cmdName, List<string> param, string appendix = "")
    {
        string str = "����: " + cmdName + " ����: ";

        foreach (var item in param)
            str += item + " ";

        str += " ����: " + appendix;
        Debug.Log(str);
    }

    // ���ض�λ�û�������
    static public void drawText(string text, Vector3 worldPosition, Color textColor, Vector2 anchor, float textSize = 15f)
    {
#if UNITY_EDITOR
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        if (!view)
            return;
        Vector3 screenPosition = view.camera.WorldToScreenPoint(worldPosition);
        if (screenPosition.y < 0 || screenPosition.y > view.camera.pixelHeight || screenPosition.x < 0 || screenPosition.x > view.camera.pixelWidth || screenPosition.z < 0)
            return;
        var pixelRatio = UnityEditor.HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.right).x - UnityEditor.HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.zero).x;
        UnityEditor.Handles.BeginGUI();
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = (int)textSize,
            normal = new GUIStyleState() { textColor = textColor }
        };
        Vector2 size = style.CalcSize(new GUIContent(text)) * pixelRatio;
        var alignedPosition =
            ((Vector2)screenPosition +
            size * ((anchor + Vector2.left + Vector2.up) / 2f)) * (Vector2.right + Vector2.down) +
            Vector2.up * view.camera.pixelHeight;
        GUI.Label(new Rect(alignedPosition / pixelRatio, size / pixelRatio), text, style);
        UnityEditor.Handles.EndGUI();
#endif
    }

    static public (float, float) Vector2Polar(Vector3 dir)
    {
        if(dir == Vector3.up)
        {
            return (0.0f, 0.0f);
        }

        float angX, angY;

        angX = Vector3.Angle(Vector3.right, new Vector3(dir.x, 0.0f, dir.z));
        angY = Vector3.Angle(Vector3.up, dir);

        if (dir.z < 0)
        {
            angX = 180.0f + (180.0f-angX);
        }

        return (angX, angY);

    }

    /// <summary>
    /// ����Ϊplant�������ļ�txt��ʽ����/cacheĿ¼�£���
    /// </summary>
    /// <param name="plant"></param>
    static public void SavePlantStructure(Plant plant, string filename)
    {
        

        List<string> lines = new List<string>();
        Queue<Internode> queue = new Queue<Internode>();
        queue.Enqueue(plant.root);
        int count = 0; // 
        int countCur = 0;
        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();

            lines.Add("+ " + countCur.ToString());
            // ��һ�� x,y,z;
            lines.Add(string.Format("-node {0} {1} {2} {3} {4} {5}",
                cur.a.x, cur.a.y, cur.a.z,
                cur.b.x, cur.b.y, cur.b.z));

            // �ڶ��� ��Ҷ
            for(int i=0; i<cur.kits.Count; ++i)
            {
                if(cur.kits[i].GetType() == typeof(Leaf))
                {
                    Leaf lf = (Leaf)cur.kits[i];

                    lines.Add(string.Format("-leaf {0}", JsonUtility.ToJson(lf)));
                }

                if(cur.kits[i].GetType() == typeof(Flower))
                {
                    Flower lf = (Flower)cur.kits[i];

                    lines.Add(string.Format("-flower {0}", JsonUtility.ToJson(lf)));
                }
            }

            string childID = "-child";
            for(int i=0; i<cur.childs.Count; ++i)
            {
                queue.Enqueue(cur.childs[i]);
                count++;
                childID += " " + (count).ToString();
            }
            lines.Add(childID);
            countCur++;
        }


        File.WriteAllLinesAsync(GlobalPath.p_catch_path+ filename, lines);
    }

    static public void SavePlantMesh()
    {

    }
    /// <summary>
    /// ��ȡplant�������ļ�txt��ʽ����/cacheĿ¼�£���
    /// </summary>
    /// <param name="plant"></param>
    /// <param name="filepath"></param>
    static public void LoadPlant(Plant plant, string filepath)
    {
        plant.ClearTree(plant.root);
        plant.ClearTree(plant.root_interpolated);

        string[] lines = File.ReadAllLines(filepath);
        
        List<Internode> internodes = new List<Internode>();
        Internode curNode = null;

        for (int m = 0; m < lines.Length; m++)
        {
            string[] line = lines[m].Split(" ");

            if (line.Length == 0)
                continue;

            if(line[0] == "-BranchRadiusFactor")
            {
                GlobalParams.p_radius_factor = float.Parse(line[1]);
            }
            if (line[0] == "-BranchBaseRadius")
            {
                GlobalParams.p_base_radius = float.Parse(line[1]);
            }
            if (line[0] == "-LeafDensity")
            {
                GlobalSketchSetting.p_leaf_density = float.Parse(line[1]);
            }
            if (line[0] == "-LeafSize")
            {
                GlobalSketchSetting.p_leaf_size = float.Parse(line[1]);
            }
            if (line[0] == "-LeafGravityDir")
            {
                GlobalSketchSetting.p_leaf_gravity_factor = float.Parse(line[1]);
            }
            if (line[0] == "-LeafBaseDir")
            {
                GlobalSketchSetting.p_leaf_base_dir_factor = float.Parse(line[1]);
            }
            if (line[0] == "-TwigGravity")
            {
                GlobalSketchSetting.p_twig_gravity_factor = float.Parse(line[1]);
            }
            if (line[0] == "-TwigBaseDir")
            {
                GlobalSketchSetting.p_twig_base_dir_factor = float.Parse(line[1]);
            }
            if(line[0] == "-TwigBaseLen")
            {
                GlobalSketchSetting.p_twig_base_len = float.Parse(line[1]);
            }

            if (line[0] == "+")
            {
                Internode tmp = new Internode();
                internodes.Add(tmp);
                curNode = tmp;
                //Debug.Log("cur:" + line[1]);
                continue;
            }

            if(line[0] == "-node")
            {
                curNode.a.x = float.Parse(line[1]);
                curNode.a.y = float.Parse(line[2]);
                curNode.a.z = float.Parse(line[3]);

                curNode.b.x = float.Parse(line[4]);
                curNode.b.y = float.Parse(line[5]);
                curNode.b.z = float.Parse(line[6]);
                continue;
            }

            if (line[0] == "-leaf")
            {
                Leaf lf = JsonUtility.FromJson<Leaf>(line[1]);
                lf.isSelected = false;
                curNode.kits.Add(lf);
                continue;
            }

            if(line[0] == "-flower")
            {
                Flower fl = JsonUtility.FromJson<Flower>(line[1]);
                fl.isSelected = false;
                curNode.kits.Add(fl);
                continue;
            }
        }

        for (int m = 0; m < lines.Length; m++)
        {
            string[] line = lines[m].Split(" ");

            if (line.Length == 0)
                continue;

            if (line[0] == "+")
            {
                curNode = internodes[int.Parse(line[1])];
                continue;
            }

            if (line[0] == "-child")
            {
                for (int i = 1; i < line.Length; ++i) 
                {
                    if (line[i] == "" || line[i] == " ")
                        continue;

                    curNode.childs.Add(internodes[int.Parse(line[i])]);
                }
                continue;
            }
        }

        if (internodes.Count == 0)
            plant.root = null;
        else
            plant.root = internodes[0]; 
    }
    
    static public void SavePlantJason()
    {

    }
}



/// <summary>
/// Json ���ݼ��Ŀ����ࣨ����ѵ��StructureNet��
/// </summary>
public class JsonDatasetGenerator
{
    public class JsonNodeLabel
    {
        public static string Plant = "plant"; // ����Plant�ķ���������չ��Ψһ��
        public static string PlantPartBox = "subbox";  // ����Box������չ��
        public static string BranchNode = "branch";  // ֦�ɣ�Ҷ�ӽ�㣩
        public static string BranchLeafNode = "leaf"; // ��Ҷ��Ҷ�ӽ�㣩
    }


    [System.Serializable]
    public class JsonNode
    {
        public int id;
        public string label;  // ȡֵ��JsonNodeLabel��
        public float[] box;
        public List<JsonNode> children = new List<JsonNode>();

        public JsonNode() { }
        public JsonNode(string label) { this.label = label; }
    }


    public static void GenerateJsonDataset(int dataNum, int iter, GrammarParser parser, bool isConsiderLeaf = false)
    {
        List<string> train_lists = new List<string>();
        List<string> val_lists = new List<string>();
        List<string> test_lists = new List<string>();
        for (int fileId = 0; fileId < dataNum; ++fileId)
        {
            string filename = string.Format("{0}.txt", fileId);

            List<string> lines = new List<string>();

            // ����ֲ��
            UnityEngine.Random.InitState(fileId * 12312321);
            Plant plant = new Plant();
            plant.ClearTree(plant.root);
            plant.ClearTree(plant.root_interpolated);

            // ����ֲ����µ�graph�ṹ
            parser.ConstructePlantStructure(iter, plant.root);
            plant.RefreshTree();

            // ���浱ǰjson�ļ�
            JsonDatasetGenerator.SaveJson2(plant, 
                string.Format("{0}{1}.json", GlobalPath.p_json_dataset_path, fileId + 1), isConsiderLeaf);
            train_lists.Add((fileId + 1).ToString());
            val_lists.Add((fileId + 1).ToString());
            test_lists.Add((fileId + 1).ToString());
        }

        File.WriteAllLinesAsync(GlobalPath.p_json_dataset_path + "train.txt", train_lists);
        File.WriteAllLinesAsync(GlobalPath.p_json_dataset_path + "val.txt", val_lists);
        File.WriteAllLinesAsync(GlobalPath.p_json_dataset_path + "test.txt", test_lists);
    }


    private static float[] GetBoxFrom(Vector3 a, Vector3 b)
    {
        float[] box = new float[12];

        Vector3 center = (a + b) / 2;
        Vector3 lengths = new Vector3((b - a).magnitude, 0.001f, 0.001f);

        // ע�⣻�ĸ�������dir1
        Vector3 dir1 = (b - a).normalized;
        Vector3 dir2 = JsonDatasetGenerator.GetOneNormalVectorFrom(dir1);

        box[0] = center.x; box[1] = center.y; box[2] = center.z;
        box[3] = lengths.x; box[4] = lengths.y; box[5] = lengths.z;
        box[6] = dir1.x; box[7] = dir1.y; box[8] = dir1.z;
        box[9] = dir2.x; box[10] = dir2.y; box[11] = dir2.z;

        return box;
    }


    /// TODO:::::::::::::::::::::::::::::::::::::::::::::::::::::
    
    private static void UpdateBoxMainDirection(Vector3[] vertices, Vector3 originPt, out Vector3 dir)
    {
        // ������һ����������
        dir = Vector3.zero;  // y �������Ҫ����

        foreach (var cur in vertices)
        {
            Vector3 curDir = (cur - originPt).normalized;  // ? �Ƿ���Ҫ����distance-weighed
            dir += curDir;
        }

        dir = dir.normalized;
    }

    private static void UpdateBoxLength(Vector3[] vertices, Vector3 originPt,
        Vector3 dirx, Vector3 diry, Vector3 dirz,
        out float max_x, out float max_y, out float max_z,
        out float min_x, out float min_y, out float min_z)
    {
        max_x = float.MinValue; max_y = float.MinValue; max_z = float.MinValue;
        min_x = float.MaxValue; min_y = float.MaxValue; min_z = float.MaxValue;

        foreach (var cur in vertices)
        {
            Vector3 curDir = (cur - originPt);

            float x = Vector3.Dot(curDir, dirx);
            float y = Vector3.Dot(curDir, diry);
            float z = Vector3.Dot(curDir, dirz);

            max_x = Mathf.Max(x, max_x);
            max_y = Mathf.Max(y, max_y);
            max_z = Mathf.Max(z, max_z);

            min_x = Mathf.Min(x, min_x);
            min_y = Mathf.Min(y, min_y);
            min_z = Mathf.Min(z, min_z);
        }
    }

    private static float[] GetBoxFromLeaf(Leaf leaf)   // �õ�Ҷ�ӵ�box
    {
        float[] box = new float[12];

        Mesh leafMesh = Geometry.CreateLeaf(leaf);  // ��Ҷ�����еĶ�������
        var vertices = leafMesh.vertices;


        Vector3 dir = leaf.dir.normalized; // ������
        Vector3 left = Vector3.left;  // ����
        if (dir != Vector3.up)
            left = Vector3.Cross(Vector3.up, dir).normalized;

        // Ӧ����Ԫ����ת
        left = leaf.rotation * left;


        // ������һ����������
        // ���ݵ�ǰҶ�ӵ����ж��㣬���¼������귽��diry
        Vector3 diry;  // y �������Ҫ����
        JsonDatasetGenerator.UpdateBoxMainDirection(vertices, leaf.a, out diry);

        // ʣ����������
        Vector3 dirx = left.normalized;  // x left����
        Vector3 dirz = Vector3.Cross(diry, dirx).normalized;
        dirx = Vector3.Cross(diry, dirz).normalized;


        // ��һ�����������length
        float max_x, max_y, max_z;     // �������������������С��ֵ
        float min_x, min_y, min_z;
        JsonDatasetGenerator.UpdateBoxLength(vertices, leaf.a, dirx, diry, dirz,
            out max_x, out max_y, out max_z, out min_x, out min_y, out min_z);

        Vector3 center = leaf.a
            + diry * (min_y + max_y) / 2.0f; 
        Vector3 length = new Vector3(max_x - min_x, max_y - min_y, max_z - min_z);

        //Vector3 length = new Vector3(0.01f,max_y-min_y, 0.01f);

        float min_width = 0.001f;  // ����Box�ĳߴ�̫С
        length.x = 0.001f;
        //length.x = Mathf.Max(min_width, length.x);
        length.y = Mathf.Max(min_width, length.y);
        length.z = 0.001f;
        //length.z = Mathf.Max(min_width, length.z);

        box[0] = center.x; box[1] = center.y; box[2] = center.z;
        box[3] = length.y; box[4] = length.x; box[5] = length.z;    // ��diry����dirx
        box[6] = diry.x; box[7] = diry.y; box[8] = diry.z;          // dir1 = diry;
        box[9] = dirx.x; box[10] = dirx.y; box[11] = dirx.z;        // dir2 = dirx;


        return box;
    }


    private static float[] GetBoxFromFlower(Flower fl)
    {
        float[] box = new float[12];

        
        return box;
    }

    /// TODO:::::::::::::::::::::::::::::::::::::::::::::::::::::
    public static Vector3 GetOneNormalVectorFrom(Vector3 dir)
    {
        if (dir.x == 0)
            return new Vector3(0, 0, -1);
        else
            return new Vector3(-dir.z / dir.x, 0, 1).normalized;
    }

    public static float[] GetBoxFromPlantRoot(Internode root, bool isConsiderLeaf = false)
    {
        // ������ȡ��root�µ�����internodes
        List<Internode> all_nodes = new List<Internode>();

        Queue<Internode> queue = new Queue<Internode>();
        queue.Enqueue(root);

        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();

            all_nodes.Add(cur);

            foreach (var child in cur.childs)
                queue.Enqueue(child);
        }

        return JsonDatasetGenerator.GetBoxFromInternodeList(all_nodes, isConsiderLeaf);
    }

    public static float[] GetBoxFromInternodeList(List<Internode> nodes, bool isConsiderLeaf = false)
    {
        float[] box = new float[12];

        Internode root = nodes[0];

        // ��øô�internodes���еĶ���
        List<Vector3> vertices = new List<Vector3>();
        
        foreach(var cur in nodes)
            vertices.Add(cur.b);


        // ������һ����������
        Vector3 diry = Vector3.zero;  // y �������Ҫ����

        JsonDatasetGenerator.UpdateBoxMainDirection(vertices.ToArray(), root.a, out diry);

        Vector3 dirx = JsonDatasetGenerator.GetOneNormalVectorFrom(diry).normalized;
        Vector3 dirz = Vector3.Cross(diry, dirx).normalized;

        // ��ҶƬ�Ķ���������
        foreach(var cur in nodes)
        {
            foreach(var kit in cur.kits)
            {
                if(kit.GetType() == typeof(Leaf))
                {
                    Mesh leafMesh = Geometry.CreateLeaf((Leaf)kit);  // ��Ҷ�����еĶ�������
                    vertices.AddRange(leafMesh.vertices);
                }
            }
        }


        // ��һ�����������length
        float max_x, max_y, max_z;     // �������������������С��ֵ
        float min_x, min_y, min_z;
        JsonDatasetGenerator.UpdateBoxLength(vertices.ToArray(), root.a, dirx, diry, dirz,
            out max_x, out max_y, out max_z, out min_x, out min_y, out min_z);


        Vector3 center = root.a
            + diry * (min_y + max_y) / 2.0f;
        Vector3 length = new Vector3(max_x - min_x, max_y - min_y, max_z - min_z);

        float min_width = 0.8f;
        //length.x = Mathf.Max(min_width, length.x);
        length.y = Mathf.Max(min_width, length.y);
        //length.z = Mathf.Max(min_width, length.z);
        length.x = length.z = 0.8f;
        box[0] = center.x; box[1] = center.y; box[2] = center.z;
        box[3] = length.y; box[4] = length.x; box[5] = length.z;    // ��diry����dirx
        box[6] = diry.x; box[7] = diry.y; box[8] = diry.z;          // dir1 = diry;
        box[9] = dirx.x; box[10] = dirx.y; box[11] = dirx.z;        // dir2 = dirx;

        return box;
    }



    /// <summary>
    /// ���ݼ��ĸ�ʽ2�����е�һ���hierarychy
    /// </summary>
    /// <param name="plant"></param>
    /// <param name="filename"></param>
    /// <param name="isConsiderLeaf"></param>
    public static JsonNode SaveJson2(Plant plant, string filename = "", bool isConsiderLeaf = false)
    {
        // ֲ�ﱾ���box
        JsonNode plantJson = new JsonNode(JsonNodeLabel.Plant); // ���������plant������ֵlabel
        plantJson.box = JsonDatasetGenerator.GetBoxFromPlantRoot(plant.root, isConsiderLeaf);

        JsonNode mainTrunkJson;
        List<Internode> subInternodes;

        (mainTrunkJson, subInternodes) = JsonDatasetGenerator.GetSubHierarchyBoxesFrom(plant.root,isConsiderLeaf);

        // ����mainTrunk
        plantJson.children.Add(mainTrunkJson);
        
        foreach(var curnode in subInternodes)
        {
            JsonNode subBranchBox = new JsonNode(JsonNodeLabel.PlantPartBox);
            subBranchBox.box = JsonDatasetGenerator.GetBoxFromPlantRoot(curnode, isConsiderLeaf);
            plantJson.children.Add(subBranchBox);
        }

        JsonDatasetGenerator.ComputeJsonNodeID(plantJson); // �������е�json����

        if(filename != "")
        {
            // �������յ�json�ļ�
            string json = JsonUtility.ToJson(plantJson);
            json = json.Replace(",\"children\":[]", string.Empty);

            File.WriteAllText(filename, json);
        }
        return plantJson;
    }

    // ���أ��Ե�ǰroot��Ӧ��mainTrunk��Ӧ��JsonNode-Box + ��mainTrunk�����з�֧��Internode
    public static (JsonNode, List<Internode>) GetSubHierarchyBoxesFrom(Internode root, bool isConsiderLeaf = false)
    {
        // ������1: ��������main trunk�� Box jsonnode
        JsonNode mainTrunkJson = new JsonNode();
        mainTrunkJson.label = JsonNodeLabel.PlantPartBox;

        // ������2: �������еķֲ�node�б��ȴ���������JsonBox��
        List<Internode> subInternodes = new List<Internode>();
        
        // ����root
        Queue<Internode> queue = new Queue<Internode>();
        queue.Enqueue(root);
        List<Internode> curLevelNodeList = new List<Internode>();  // ��¼���ϵ�ǰlevel������internodes������

        while(queue.Count!=0)
        {
            Internode cur = queue.Dequeue();

            curLevelNodeList.Add(cur);

            foreach(var child in cur.childs)  // �������еĺ���
            {
                if ((root.level == 0) && (child.level != 0) && (child.level != 1)) // �������0����ǰ������0���Ҳ�����1
                {
                    subInternodes.Add(child);
                    continue;
                }
                if ((root.level != 0) && (child.level != root.level))  // ���������0�����ҵ�ǰlevel������root��level
                {
                    subInternodes.Add(child);
                    continue;
                }

                queue.Enqueue(child);
            }
        }

        // ����MainTrunk����jsonNode
        mainTrunkJson.box = JsonDatasetGenerator.GetBoxFromInternodeList(curLevelNodeList);

        // �õ���maintrunkjson֮�󣬻���Ҫ����curlevelNodelist�����ɺ���jsonbox����Ҫд�º�����

        foreach (Internode curNode in curLevelNodeList)
        {
            foreach(Kit kit in curNode.kits)
            {
                if(kit.GetType() == typeof(Leaf))
                {
                    JsonNode leafnode = new JsonNode(JsonNodeLabel.BranchLeafNode);
                    leafnode.box = JsonDatasetGenerator.GetBoxFromLeaf((Leaf)kit);
                    mainTrunkJson.children.Add(leafnode);
                }
            }
        }


        return (mainTrunkJson, subInternodes);
    }

    public static void ComputeJsonNodeID(JsonNode root)
    {
        Queue<JsonNode> queue = new Queue<JsonNode>();
        queue.Enqueue(root);

        int id = 0;
        while (queue.Count != 0)
        {
            JsonNode cur = queue.Dequeue();
            cur.id = id;
            id++;
            foreach(var child in cur.children)
            {
                queue.Enqueue(child);
            }
        }
    }
}


public class PythonHandler
{
    static public string result = ""; // python����ʱִ�н�������нű������й���

    static public void RunPythonScript(string python_exe, string script_path, string argvs)
    {
        var p = new System.Diagnostics.Process();

        string command = script_path + " " + argvs; // ָ��

        p.StartInfo.FileName = python_exe;
        p.StartInfo.Arguments = command;


        p.StartInfo.UseShellExecute = true;
        p.StartInfo.RedirectStandardOutput = false;
        p.StartInfo.RedirectStandardError = false;
        p.StartInfo.RedirectStandardInput = false;
        p.StartInfo.CreateNoWindow = false;

        p.Start();

        //p.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(Get_data);
        //p.WaitForExit();
        System.Threading.Thread.Sleep(1000);
    }

    private static void Get_data(object sender, System.Diagnostics.DataReceivedEventArgs eventArgs)
    {
        Debug.Log("Hi!!!!");
        if (!string.IsNullOrEmpty(eventArgs.Data))
        {
            result = eventArgs.Data;
        }
    }
}