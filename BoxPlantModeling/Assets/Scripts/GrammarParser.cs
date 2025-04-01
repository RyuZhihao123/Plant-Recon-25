using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using DataStructures.ViliWonka.KDTree;


/// <summary>
/// 小王八类：记录我们可爱的小鳖的状态(State)
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
    public List<float> sequences_probability = new List<float>();  // 每个sequence对应的概率
    public static void UpdateRules(Dictionary<string, Rule> rules, string str)
    {
        string[] __1 = str.Split("=");

        string left = __1[0];   // =的前半部分 A(l,w)
        string right = __1[1];  // =的后半部分 +F(l,w)\\A(l,w)

        if (left.Contains("("))
        {
            int lid = left.IndexOf("(");

            string label = UTIL.SubString(left, 0, lid - 1);  // rule的名字-label (A)
            string[] param_list = UTIL.SubString(left, lid + 1, left.Length - 2).Split(","); // rule的参数列表 (l,w)

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
            throw new System.Exception(string.Format("[错误] 该字符{0}没有对应的产生式", this.label));

        if (sequences.Count == 1)
            return sequences[0];

        int id = ((int)Random.Range(0, 300)) %sequences.Count;

        return sequences[id];

    }
}

public class GrammarParser
{
    string premise = ""; // 初始字符
    // 自定义的参数数值(e h)
    Dictionary<string, float> custom_values = new Dictionary<string, float>();
    // 规则 （A B C)
    Dictionary<string, Rule> rules = new Dictionary<string, Rule>();

    // 内置的关键字
    string keyword_internode = "F"; // 树干
    string keyword_leaf = "J";      // 树叶
    string keyword_flower = "K";      // 花瓣

    // Turtle状态栈
    List<Turtle> stack = new List<Turtle>();

    // 所有生成的线段
    public List<Vector3> lines = new List<Vector3>();
    // 所有生成的花朵/树叶/果实等等
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
        //Debug.Log("迭代次数: " + maxlevel.ToString());
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
        // 构建parameters
        // 补充：随机性挑选一个规则
        Rule currentRule = rules[label]; //当前的规则
        string sequence = currentRule.GetRandomGeneration();  // 产生式

        // 用来实现移动的命令
        List<string> cmds = new List<string> { "+", "-", "&", "^", "\\", "/", "!", "?", ";", "@" };

        if (level > maxlevel)
            return;

        for (int i = 0; i < sequence.Length;)
        {

            // 如果是单字符的-关键字cmds（角度控制和移动步长控制）
            if (cmds.Contains(sequence[i].ToString()))
            {
                string cur_cmd = sequence[i].ToString(); // + - \ / & ^

                List<string> cur_params = UTIL.GetFollowedParams(sequence, i + 1, out i);

                UTIL.PrintSubCommond(cur_cmd, cur_params);

                // 解析参数 +!(angle/factor)。// 唯一的参数：旋转角度/缩放倍数
                float cur_param_value = -1;
                if (cur_params.Count != 0) // 如果参数序列的个数不为0
                    cur_param_value = UTIL.ComputeMathExpression(cur_params[0], variables, level, maxlevel);

                // 解析旋转时的角度参数(angle
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
                if (cur_cmd == "!") // 移动步长-缩小
                    stack[stack.Count - 1].cur_branch_len *= local_len_factor;
                if (cur_cmd == "?") // 移动步长-放大
                    stack[stack.Count - 1].cur_branch_len /= local_len_factor;
                if (cur_cmd == ";") // 旋转角度-缩小
                {
                    stack[stack.Count - 1].cur_angX *= local_angX_factor;
                    stack[stack.Count - 1].cur_angY *= local_angY_factor;
                    stack[stack.Count - 1].cur_angZ *= local_angZ_factor;
                }
                if (cur_cmd == "@") // 旋转角度-放大
                {
                    stack[stack.Count - 1].cur_angX /= local_angX_factor;
                    stack[stack.Count - 1].cur_angY /= local_angY_factor;
                    stack[stack.Count - 1].cur_angZ /= local_angZ_factor;
                }
                continue;
            }

            // 如果是自定义关键字(A,B,C,D,E,... 除了(F,J,K)三兄弟之外)
            bool isCustomValue = false;
            foreach (var rule in rules)
            {
                if (i + rule.Key.Length <= sequence.Length &&  // 禁止越界
                    sequence.Substring(i, rule.Key.Length) == rule.Key)
                {
                    isCustomValue = true;
                    string cur_cmd = rule.Key;
                    List<string> cur_params = UTIL.GetFollowedParams(sequence, i + cur_cmd.Length, out i);

                    UTIL.PrintSubCommond(cur_cmd, cur_params);

                    // 更新参数  ABCDE...(x,y,z,....)
                    Dictionary<string, float> new_variables = new Dictionary<string, float>(variables);
                    UTIL.PrintDictionary(new_variables);
                    for (int g = 0; g < cur_params.Count; ++g)
                    {
                        string param_name = rule.Value.param_list[g]; // 第g个参数的label
                        new_variables[param_name] = UTIL.ComputeMathExpression(cur_params[g], variables, t:level, mt: maxlevel);  // 计算新的数值
                        //Debug.Log("计算" + param_name +":"+ cur_params[g] +"="+ new_variables[param_name].ToString());
                    }
                    UTIL.PrintDictionary(new_variables);

                    if(Random.Range(0.0f, 1.0f) <=GlobalParams.p_prob_branch)  // 随机性控制：决定是否进入产生式
                        Recursive(cur_cmd, new_variables, level + 1, maxlevel);
                }
            }
            if (isCustomValue)
                continue;

            /////////// 如果是F命令（绘制一段branch)
            if (i + keyword_internode.Length <= sequence.Length
                && sequence.Substring(i, keyword_internode.Length) == keyword_internode)
            {
                string cur_cmd = keyword_internode;
                List<string> cur_params = UTIL.GetFollowedParams(sequence, i + cur_cmd.Length, out i);
                string appendix = stack[stack.Count - 1].position.ToString() + "->";

                // 解析参数 F(step:步长)
                float cur_step_len = stack[stack.Count - 1].cur_branch_len; // 无显示指定step，为当前Turtle的步长
                if (cur_params.Count != 0)
                    cur_step_len = UTIL.ComputeMathExpression(cur_params[0], variables, level, maxlevel); // 计算得到新步长

                lines.Add(stack[stack.Count - 1].position);
                stack[stack.Count - 1].Forward(cur_step_len);  // 移动一格
                lines.Add(stack[stack.Count - 1].position);

                appendix += stack[stack.Count - 1].position.ToString();
                UTIL.PrintSubCommond(cur_cmd, cur_params, appendix);

                continue;
            }

            /////////// 如果是J命令则得到叶片，如果是K命令则得到果实
            {
                string cur_cmd = IsCommand(i, sequence, keyword_leaf); // 判断是不是"J"
                if (cur_cmd != keyword_leaf) cur_cmd = IsCommand(i, sequence, keyword_flower); // 判断是不是"K"

                if (cur_cmd == keyword_leaf || cur_cmd == keyword_flower) // 如果是"J"或者"K"
                {
                    List<string> cur_params = UTIL.GetFollowedParams(sequence, i + cur_cmd.Length, out i);
                    string appendix = stack[stack.Count - 1].position.ToString() + "->";

                    // 如果是树叶
                    if (cur_cmd == "J")
                    {
                        
                        // 解析参数 J(width,height,horBendAngle, verBendAngle);
                        float w = GlobalParams.p_leaf_witdth;
                        float h = GlobalParams.p_leaf_height;
                        float horBend_Ang = GlobalParams.p_leaf_horBend_Ang;
                        float verBend_Ang = GlobalParams.p_leaf_verBend_Ang;

                        if (cur_params.Count >= 1) w = UTIL.ComputeMathExpression(cur_params[0], variables, level, maxlevel);
                        if (cur_params.Count >= 2) h = UTIL.ComputeMathExpression(cur_params[1], variables, level, maxlevel);
                        if (cur_params.Count >= 3) horBend_Ang = UTIL.ComputeMathExpression(cur_params[2], variables, level, maxlevel);
                        if (cur_params.Count >= 4) verBend_Ang = UTIL.ComputeMathExpression(cur_params[3], variables, level, maxlevel);
                        //Debug.Log(string.Format("参数个数{0}={1}-{2}-{3}-{4}", cur_params.Count,w, h, horBend_Ang, verBend_Ang));
                        // 创建leaf
                        m_kits.Add(new Leaf(
                            stack[stack.Count - 1].position,
                            stack[stack.Count - 1].direction * Vector3.up,
                            w,h,horBend_Ang,verBend_Ang));
                    }
                    if (cur_cmd == "K")
                    {
                        // 解析参数 K(scale)
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
                // 入栈
                i++;
                //Debug.Log("入栈[");
                stack.Add(new Turtle(stack[stack.Count - 1]));
                //stack_internodes.Add(stack_internodes[stack_internodes.Count-1]); // 加入栈
                continue;
            }
            if (sequence[i] == '$')  // TODO: $有个方向向量的参数(x,y,z)
            {
                // 入栈
                i++;
                //Debug.Log("未定义符号: $");
                //stack[stack.Count - 1].position = stack[0].position;
                continue;
            }
            if (sequence[i] == ']')
            {
                // 出栈 i++
                i++;
                //Debug.Log("出栈]: " + stack.Count);
                stack.RemoveAt(stack.Count - 1);
                //stack_internodes.RemoveAt(stack_internodes.Count - 1);
                continue;
            }


            throw new System.Exception(string.Format("异常: 存在无法解析的字符{0}(层次{1})", sequence[i], level));
        }
    }

    private string IsCommand(int i, string sequence, string cmd)
    {
        if (i + cmd.Length <= sequence.Length && sequence.Substring(i, cmd.Length) == cmd)
            return cmd;
        else
            return "";
    }

    // 在进行递归完成后，根据lines生成tree strcture
    public void BuildStructure_with_kdtree(Internode root)
    {
        float tolerance = 0.001f;

        Vector3[] points_a = new Vector3[lines.Count / 2]; // 根据每段line的首端点a建立kd-tree
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

        // 根节点进行一个处理
        List<int> results = new List<int>();
        query.Radius(tree, root.b, tolerance, results);

        //Debug.Log("根节点的孩子:" + results.Count.ToString());
        for (int i = 0; i < results.Count; ++i)
        {
            int id = results[i]; // 在Points_a中的id
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
            //Debug.Log("当前的孩子:" + results.Count.ToString() + " " + len.ToString());
            for (int i = 0; i < results.Count; ++i)
            {
                int id = results[i]; // 在Points_a中的id
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

    // 在进行递归完成后，根据lines生成tree strcture
    public void Build_allkits_with_kdtree(Internode root)
    {
        // 首先提取所有的internodes
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
        //Debug.Log("仅仅叶片:" + m_kits.Count.ToString());
        // 如果只有一个根节点把叶子直接加进来就好嘛
        if(root.childs.Count == 0)
        {
            for (int i = 0; i < m_kits.Count; ++i)
                root.kits.Add(m_kits[i]);
            return;
        }

        Vector3[] points_b = new Vector3[branches.Count]; // 根据每段Internode的端点b建立kd-tree

        for (int i = 0; i < branches.Count; i++)
        {
            points_b[i] = branches[i].b;
        }

        int maxPointsPerLeafNode = 8;
        KDTree tree = new KDTree(points_b, maxPointsPerLeafNode);
        DataStructures.ViliWonka.KDTree.KDQuery query = new DataStructures.ViliWonka.KDTree.KDQuery();

        // 遍历每个kit
        for (int i = 0; i < m_kits.Count; ++i)
        {
            List<int> results = new List<int>();
            query.ClosestPoint(tree, m_kits[i].a, results); // 查询最近的

            branches[results[0]].kits.Add(m_kits[i]);
        }
    }
}


