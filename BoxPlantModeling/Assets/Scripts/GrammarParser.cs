using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using DataStructures.ViliWonka.KDTree;


/// <summary>
/// С�����ࣺ��¼���ǿɰ���С���״̬(State)
/// </summary>
public class Turtle
{
    public Quaternion direction = Quaternion.identity;
    public Vector3 position = Vector3.zero;

    public float cur_angX = GlobalParams.p_angX;
    public float cur_angY = GlobalParams.p_angY;
    public float cur_angZ = GlobalParams.p_angZ;
    public float cur_branch_len = GlobalParams.p_branch_len;

    public Turtle() { }

    public Turtle(Turtle other)
    {
        this.direction = other.direction;
        this.position = other.position;
        this.cur_angX = other.cur_angX;
        this.cur_angY = other.cur_angY;
        this.cur_angZ = other.cur_angZ;
        this.cur_branch_len = other.cur_branch_len;
    }

    public void Forward(float step)
    {
        position += direction * new Vector3(0, step, 0);
    }


    public void RotateX(float angX, int sign = 1)
    {
        direction *= Quaternion.Euler(sign * angX, 0, 0);
    }

    public void RotateY(float angY, int sign = 1)
    {
        direction *= Quaternion.Euler(0, sign * angY, 0);
    }

    public void RotateZ(float angZ, int sign = 1)
    {
        direction *= Quaternion.Euler(0, 0, sign * angZ);
    }
}

public class Rule
{
    public string label;  // label
    public List<string> param_list = new List<string>(); // parameter names
    public List<string> sequences = new List<string>();  // sequences
    public List<float> sequences_probability = new List<float>();  // ÿ��sequence��Ӧ�ĸ���
    public static void UpdateRules(Dictionary<string, Rule> rules, string str)
    {
        string[] __1 = str.Split("=");

        string left = __1[0];   // =��ǰ�벿�� A(l,w)
        string right = __1[1];  // =�ĺ�벿�� +F(l,w)\\A(l,w)

        if (left.Contains("("))
        {
            int lid = left.IndexOf("(");

            string label = UTIL.SubString(left, 0, lid - 1);  // rule������-label (A)
            string[] param_list = UTIL.SubString(left, lid + 1, left.Length - 2).Split(","); // rule�Ĳ����б� (l,w)

            if (rules[label].param_list.Count == 0)
                rules[label].param_list = new List<string>(param_list);
            rules[label].sequences.Add(right);
        }
        else
        {
            rules[left].sequences.Add(right);
        }
    }

    public string GetRandomGeneration()
    {
        if (sequences.Count == 0)
            throw new System.Exception(string.Format("[����] ���ַ�{0}û�ж�Ӧ�Ĳ���ʽ", this.label));

        if (sequences.Count == 1)
            return sequences[0];

        int id = ((int)Random.Range(0, 300)) %sequences.Count;

        return sequences[id];

    }
}

public class GrammarParser
{
    string premise = ""; // ��ʼ�ַ�
    // �Զ���Ĳ�����ֵ(e h)
    Dictionary<string, float> custom_values = new Dictionary<string, float>();
    // ���� ��A B C)
    Dictionary<string, Rule> rules = new Dictionary<string, Rule>();

    // ���õĹؼ���
    string keyword_internode = "F"; // ����
    string keyword_leaf = "J";      // ��Ҷ
    string keyword_flower = "K";      // ����

    // Turtle״̬ջ
    List<Turtle> stack = new List<Turtle>();

    // �������ɵ��߶�
    public List<Vector3> lines = new List<Vector3>();
    // �������ɵĻ���/��Ҷ/��ʵ�ȵ�
    public List<Kit> m_kits = new List<Kit>();




    public void ParseFile(string filename)
    {
        string[] allLines = File.ReadAllLines(filename, Encoding.UTF8);

        custom_values.Clear();
        rules.Clear();

        for (int m = 0; m < allLines.Length; m++)
        {
            string[] line = allLines[m].Split(" ");

            if (line.Length <= 1)
                continue;

            if (line[0] == "Value")
            {
                //UTIL.PrintStringList(line);
                custom_values.Add(line[1], float.Parse(line[2]));
            }

            if (line[0] == "Premise")
            {
                premise = line[1];
            }

            if (line[0] == "Keywords")
            {
                for (int i = 1; i < line.Length; ++i)
                {
                    Rule rule = new Rule();
                    rule.label = line[i];
                    rules.Add(rule.label, rule);

                }
            }

            if (line[0] == "Rule")
            {
                Rule.UpdateRules(rules, line[1]);
            }
        }

        UTIL.PrintDictionary(custom_values);

        foreach (var item in rules)
        {
            UTIL.PrintRule(item.Value);
        }
    }

    public void ConstructePlantStructure(int maxlevel, Internode root)
    {
        //Debug.Log("��������: " + maxlevel.ToString());
        this.stack.Clear();
        this.lines.Clear();
        this.m_kits.Clear();
        //this.stack_internodes.Clear();

        this.stack.Add(new Turtle());
        //this.stack_internodes.Add(root);

        Recursive(premise, custom_values, 1, maxlevel);

        BuildStructure_with_kdtree(root);
        Build_allkits_with_kdtree(root);
    }

    public void Recursive(string label, Dictionary<string, float> variables,  int level, int maxlevel)
    {
        // ����parameters
        // ���䣺�������ѡһ������
        Rule currentRule = rules[label]; //��ǰ�Ĺ���
        string sequence = currentRule.GetRandomGeneration();  // ����ʽ

        // ����ʵ���ƶ�������
        List<string> cmds = new List<string> { "+", "-", "&", "^", "\\", "/", "!", "?", ";", "@" };

        if (level > maxlevel)
            return;

        for (int i = 0; i < sequence.Length;)
        {

            // ����ǵ��ַ���-�ؼ���cmds���Ƕȿ��ƺ��ƶ��������ƣ�
            if (cmds.Contains(sequence[i].ToString()))
            {
                string cur_cmd = sequence[i].ToString(); // + - \ / & ^

                List<string> cur_params = UTIL.GetFollowedParams(sequence, i + 1, out i);

                UTIL.PrintSubCommond(cur_cmd, cur_params);

                // �������� +!(angle/factor)��// Ψһ�Ĳ�������ת�Ƕ�/���ű���
                float cur_param_value = -1;
                if (cur_params.Count != 0) // ����������еĸ�����Ϊ0
                    cur_param_value = UTIL.ComputeMathExpression(cur_params[0], variables, level, maxlevel);

                // ������תʱ�ĽǶȲ���(angle
                float angX = stack[stack.Count - 1].cur_angX;
                float angY = stack[stack.Count - 1].cur_angY;
                float angZ = stack[stack.Count - 1].cur_angZ;

                if (cur_param_value != -1)
                    angX = angY = angZ = cur_param_value;

                // Position & Rotation
                if (cur_cmd == "+")
                    stack[stack.Count - 1].RotateZ(angZ, +1);
                if (cur_cmd == "-")
                    stack[stack.Count - 1].RotateZ(angZ, -1);
                if (cur_cmd == "\\")
                    stack[stack.Count - 1].RotateY(angY, +1);
                if (cur_cmd == "/")
                    stack[stack.Count - 1].RotateY(angY, -1);
                if (cur_cmd == "&")
                    stack[stack.Count - 1].RotateX(angX, +1);
                if (cur_cmd == "^")
                    stack[stack.Count - 1].RotateX(angX, -1);

                float local_len_factor = GlobalParams.p_branch_len_factor;
                float local_angX_factor = GlobalParams.p_angX_factor;
                float local_angY_factor = GlobalParams.p_angY_factor;
                float local_angZ_factor = GlobalParams.p_angZ_factor;
                if (cur_param_value != -1)
                    local_len_factor = local_angX_factor = local_angY_factor = local_angZ_factor = cur_param_value;

                // Incremental changes
                if (cur_cmd == "!") // �ƶ�����-��С
                    stack[stack.Count - 1].cur_branch_len *= local_len_factor;
                if (cur_cmd == "?") // �ƶ�����-�Ŵ�
                    stack[stack.Count - 1].cur_branch_len /= local_len_factor;
                if (cur_cmd == ";") // ��ת�Ƕ�-��С
                {
                    stack[stack.Count - 1].cur_angX *= local_angX_factor;
                    stack[stack.Count - 1].cur_angY *= local_angY_factor;
                    stack[stack.Count - 1].cur_angZ *= local_angZ_factor;
                }
                if (cur_cmd == "@") // ��ת�Ƕ�-�Ŵ�
                {
                    stack[stack.Count - 1].cur_angX /= local_angX_factor;
                    stack[stack.Count - 1].cur_angY /= local_angY_factor;
                    stack[stack.Count - 1].cur_angZ /= local_angZ_factor;
                }
                continue;
            }

            // ������Զ���ؼ���(A,B,C,D,E,... ����(F,J,K)���ֵ�֮��)
            bool isCustomValue = false;
            foreach (var rule in rules)
            {
                if (i + rule.Key.Length <= sequence.Length &&  // ��ֹԽ��
                    sequence.Substring(i, rule.Key.Length) == rule.Key)
                {
                    isCustomValue = true;
                    string cur_cmd = rule.Key;
                    List<string> cur_params = UTIL.GetFollowedParams(sequence, i + cur_cmd.Length, out i);

                    UTIL.PrintSubCommond(cur_cmd, cur_params);

                    // ���²���  ABCDE...(x,y,z,....)
                    Dictionary<string, float> new_variables = new Dictionary<string, float>(variables);
                    UTIL.PrintDictionary(new_variables);
                    for (int g = 0; g < cur_params.Count; ++g)
                    {
                        string param_name = rule.Value.param_list[g]; // ��g��������label
                        new_variables[param_name] = UTIL.ComputeMathExpression(cur_params[g], variables, t:level, mt: maxlevel);  // �����µ���ֵ
                        //Debug.Log("����" + param_name +":"+ cur_params[g] +"="+ new_variables[param_name].ToString());
                    }
                    UTIL.PrintDictionary(new_variables);

                    if(Random.Range(0.0f, 1.0f) <=GlobalParams.p_prob_branch)  // ����Կ��ƣ������Ƿ�������ʽ
                        Recursive(cur_cmd, new_variables, level + 1, maxlevel);
                }
            }
            if (isCustomValue)
                continue;

            /////////// �����F�������һ��branch)
            if (i + keyword_internode.Length <= sequence.Length
                && sequence.Substring(i, keyword_internode.Length) == keyword_internode)
            {
                string cur_cmd = keyword_internode;
                List<string> cur_params = UTIL.GetFollowedParams(sequence, i + cur_cmd.Length, out i);
                string appendix = stack[stack.Count - 1].position.ToString() + "->";

                // �������� F(step:����)
                float cur_step_len = stack[stack.Count - 1].cur_branch_len; // ����ʾָ��step��Ϊ��ǰTurtle�Ĳ���
                if (cur_params.Count != 0)
                    cur_step_len = UTIL.ComputeMathExpression(cur_params[0], variables, level, maxlevel); // ����õ��²���

                lines.Add(stack[stack.Count - 1].position);
                stack[stack.Count - 1].Forward(cur_step_len);  // �ƶ�һ��
                lines.Add(stack[stack.Count - 1].position);

                appendix += stack[stack.Count - 1].position.ToString();
                UTIL.PrintSubCommond(cur_cmd, cur_params, appendix);

                continue;
            }

            /////////// �����J������õ�ҶƬ�������K������õ���ʵ
            {
                string cur_cmd = IsCommand(i, sequence, keyword_leaf); // �ж��ǲ���"J"
                if (cur_cmd != keyword_leaf) cur_cmd = IsCommand(i, sequence, keyword_flower); // �ж��ǲ���"K"

                if (cur_cmd == keyword_leaf || cur_cmd == keyword_flower) // �����"J"����"K"
                {
                    List<string> cur_params = UTIL.GetFollowedParams(sequence, i + cur_cmd.Length, out i);
                    string appendix = stack[stack.Count - 1].position.ToString() + "->";

                    // �������Ҷ
                    if (cur_cmd == "J")
                    {
                        
                        // �������� J(width,height,horBendAngle, verBendAngle);
                        float w = GlobalParams.p_leaf_witdth;
                        float h = GlobalParams.p_leaf_height;
                        float horBend_Ang = GlobalParams.p_leaf_horBend_Ang;
                        float verBend_Ang = GlobalParams.p_leaf_verBend_Ang;

                        if (cur_params.Count >= 1) w = UTIL.ComputeMathExpression(cur_params[0], variables, level, maxlevel);
                        if (cur_params.Count >= 2) h = UTIL.ComputeMathExpression(cur_params[1], variables, level, maxlevel);
                        if (cur_params.Count >= 3) horBend_Ang = UTIL.ComputeMathExpression(cur_params[2], variables, level, maxlevel);
                        if (cur_params.Count >= 4) verBend_Ang = UTIL.ComputeMathExpression(cur_params[3], variables, level, maxlevel);
                        //Debug.Log(string.Format("��������{0}={1}-{2}-{3}-{4}", cur_params.Count,w, h, horBend_Ang, verBend_Ang));
                        // ����leaf
                        m_kits.Add(new Leaf(
                            stack[stack.Count - 1].position,
                            stack[stack.Count - 1].direction * Vector3.up,
                            w,h,horBend_Ang,verBend_Ang));
                    }
                    if (cur_cmd == "K")
                    {
                        // �������� K(scale)
                        float flower_scale = GlobalParams.p_flower_scale;
                        if (cur_params.Count != 0)
                            flower_scale = UTIL.ComputeMathExpression(cur_params[0], variables, level, maxlevel);

                        m_kits.Add(new Flower(
                            stack[stack.Count - 1].position,
                            stack[stack.Count - 1].direction,
                            flower_scale));
                    }

                    appendix += stack[stack.Count - 1].position.ToString();
                    UTIL.PrintSubCommond(cur_cmd, cur_params, appendix);

                    continue;
                }
            }

            if (sequence[i] == '[')
            {
                // ��ջ
                i++;
                //Debug.Log("��ջ[");
                stack.Add(new Turtle(stack[stack.Count - 1]));
                //stack_internodes.Add(stack_internodes[stack_internodes.Count-1]); // ����ջ
                continue;
            }
            if (sequence[i] == '$')  // TODO: $�и����������Ĳ���(x,y,z)
            {
                // ��ջ
                i++;
                //Debug.Log("δ�������: $");
                //stack[stack.Count - 1].position = stack[0].position;
                continue;
            }
            if (sequence[i] == ']')
            {
                // ��ջ i++
                i++;
                //Debug.Log("��ջ]: " + stack.Count);
                stack.RemoveAt(stack.Count - 1);
                //stack_internodes.RemoveAt(stack_internodes.Count - 1);
                continue;
            }


            throw new System.Exception(string.Format("�쳣: �����޷��������ַ�{0}(���{1})", sequence[i], level));
        }
    }

    private string IsCommand(int i, string sequence, string cmd)
    {
        if (i + cmd.Length <= sequence.Length && sequence.Substring(i, cmd.Length) == cmd)
            return cmd;
        else
            return "";
    }

    // �ڽ��еݹ���ɺ󣬸���lines����tree strcture
    public void BuildStructure_with_kdtree(Internode root)
    {
        float tolerance = 0.001f;

        Vector3[] points_a = new Vector3[lines.Count / 2]; // ����ÿ��line���׶˵�a����kd-tree
        bool[] visited = new bool[points_a.Length];
        for (int i = 0; i < lines.Count; i += 2)
        {
            points_a[i / 2] = lines[i];
            visited[i / 2] = false;
        }

        int maxPointsPerLeafNode = 8;
        KDTree tree = new KDTree(points_a, maxPointsPerLeafNode);
        DataStructures.ViliWonka.KDTree.KDQuery query = new DataStructures.ViliWonka.KDTree.KDQuery();

        Queue<Internode> queue = new Queue<Internode>();

        // ���ڵ����һ������
        List<int> results = new List<int>();
        query.Radius(tree, root.b, tolerance, results);

        //Debug.Log("���ڵ�ĺ���:" + results.Count.ToString());
        for (int i = 0; i < results.Count; ++i)
        {
            int id = results[i]; // ��Points_a�е�id
            Internode newnode = new Internode();
            newnode.a = lines[id];
            newnode.b = lines[id + 1];

            root.childs.Add(newnode);

            queue.Enqueue(newnode);

            visited[id] = true;
        }

        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();
            results.Clear();
            float len = (cur.b - cur.a).magnitude * tolerance;

            query.Radius(tree, cur.b, len, results);
            //Debug.Log("��ǰ�ĺ���:" + results.Count.ToString() + " " + len.ToString());
            for (int i = 0; i < results.Count; ++i)
            {
                int id = results[i]; // ��Points_a�е�id
                //Debug.Log(visited[id] + " " +id.ToString() + " | cur=" + cur.a +" "+ cur.b + " - child=" + lines[id+0] + lines[id+1]);
                if (visited[id] == true)
                    continue;

                Internode newnode = new Internode();
                newnode.a = lines[2 * id + 0];
                newnode.b = lines[2 * id + 1];


                cur.childs.Add(newnode);

                queue.Enqueue(newnode);

                visited[id] = true;
            }
        }
    }

    // �ڽ��еݹ���ɺ󣬸���lines����tree strcture
    public void Build_allkits_with_kdtree(Internode root)
    {
        // ������ȡ���е�internodes
        List<Internode> branches = new List<Internode>();

        Queue<Internode> queue = new Queue<Internode>();
        queue.Enqueue(root);
        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();
            branches.Add(cur);
            foreach (Internode child in cur.childs)
                queue.Enqueue(child);
        }
        //Debug.Log("����ҶƬ:" + m_kits.Count.ToString());
        // ���ֻ��һ�����ڵ��Ҷ��ֱ�Ӽӽ����ͺ���
        if(root.childs.Count == 0)
        {
            for (int i = 0; i < m_kits.Count; ++i)
                root.kits.Add(m_kits[i]);
            return;
        }

        Vector3[] points_b = new Vector3[branches.Count]; // ����ÿ��Internode�Ķ˵�b����kd-tree

        for (int i = 0; i < branches.Count; i++)
        {
            points_b[i] = branches[i].b;
        }

        int maxPointsPerLeafNode = 8;
        KDTree tree = new KDTree(points_b, maxPointsPerLeafNode);
        DataStructures.ViliWonka.KDTree.KDQuery query = new DataStructures.ViliWonka.KDTree.KDQuery();

        // ����ÿ��kit
        for (int i = 0; i < m_kits.Count; ++i)
        {
            List<int> results = new List<int>();
            query.ClosestPoint(tree, m_kits[i].a, results); // ��ѯ�����

            branches[results[0]].kits.Add(m_kits[i]);
        }
    }
}


