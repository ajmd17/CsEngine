﻿using ApexEngine.Assets;
using ApexEngine.Math;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApexEngine.Rendering.Util
{
    public class ShaderUtil
    {
        public static string GetValueString(string varName, object val)
        {
            if (val is bool)
            {
                bool bval = (bool)val;
                return (bval ? "true" : "false");
            }
            else if (val is int)
            {
                int ival = (int)val;
                return ival.ToString();
            }
            else if (val is float)
            {
                float fval = (float)val;
                return fval.ToString();
            }
            else if (val is Vector2f)
            {
                Vector2f vval = (Vector2f)val;
                return "vec2(" + vval.x + ", " + vval.y + ")";
            }
            else if (val is Vector3f)
            {
                Vector3f vval = (Vector3f)val;
                return "vec3(" + vval.x + ", " + vval.y + ", " + vval.z + ")";
            }
            else if (val is Vector4f)
            {
                Vector4f vval = (Vector4f)val;
                return "vec4(" + vval.x + ", " + vval.y + ", " + vval.z + ", " + vval.w + ")";
            }
            return "";
        }

        public static bool CompareShader(ShaderProperties a, ShaderProperties b)
       { 

            /*string[] keys_a = a.values.Keys.ToArray();
            string[] keys_b = b.values.Keys.ToArray();
            object[] vals_a = a.values.Values.ToArray();
            object[] vals_b = b.values.Values.ToArray();

            for (int i = 0; i < keys_a.Length; i++)
            {
                if (!keys_a[i].Equals(keys_b[i]))
                {
                    return false;
                }
                else
                {
                    if (!vals_a[i].Equals(vals_b[i]))
                    {
                        return false;
                    }
                }
            }*/

            foreach (var pair in a.values)
            {
                object value;
                if (b.values.TryGetValue(pair.Key, out value))
                {
                    if (value is bool)
                    {
                        if (pair.Value is bool)
                            return ((bool)value).Equals((bool)pair.Value);
                        else
                            return false;
                    }
                    else if (value is int)
                    {
                        if (pair.Value is int)
                            return ((int)value).Equals((int)pair.Value);
                        else
                            return false;
                    }
                    else if (value is float)
                    {
                        if (pair.Value is float)
                            return ((float)value).Equals((float)pair.Value);
                        else
                            return false;
                    }
                    else if (value is string)
                    {
                        if (pair.Value is string)
                            return ((string)value).Equals((string)pair.Value);
                        else
                            return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public static string FormatShaderVersion(string origCode)
        {
            string res = "";
            string verString = "";
            string[] lines = origCode.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Trim().StartsWith("#version"))
                {
                    verString = line.Trim();
                    line = "";
                }
                if (lines[i] != "")
                    res += line + "\n";
            }
            return verString + "\n" + res;
        }

        public static string FormatShaderIncludes(string shaderPath, string origCode)
        {
            string res = "";
            string[] lines = origCode.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Trim().StartsWith("#include"))
                {
                    string path = line.Trim().Substring("#include ".Length);
                    if (path.Contains("<") || path.Contains(">")) // internal resource
                    {
                        path = path.Replace("<", "");
                        path = path.Replace(">", "");
                        line = Properties.Resources.ResourceManager.GetString(path);
                    }
                    else // external resource
                    {
                        path = path.Replace("\"", "");

                        string parentPath = System.IO.Directory.GetParent(shaderPath).ToString();
                        string incPath = parentPath + "\\" + path;

                        line = (string)AssetManager.Load(incPath, ShaderTextLoader.GetInstance());
                    }
                }
                if (lines[i] != "")
                    res += line + "\n";
            }
            return res;
        }

        public static string FormatShaderProperties(string origCode, ShaderProperties properties)
        {
            string res = "";
            string[] lines = origCode.Split('\n');
            bool inIfStatement = false;
            String ifStatementText = "";
            bool removing = false;
            List<string> ifdefs = new List<string>();
            List<string> ifndefs = new List<string>();
            string currentIfDef = "";

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Trim().StartsWith("#ifdef"))
                {
                    inIfStatement = true;
                    ifStatementText = lines[i].Trim().Substring(7);
                    ifdefs.Add("!" + ifStatementText);
                    currentIfDef = "!" + ifStatementText;

                    bool remove = !(properties.GetBool(ifStatementText));

                    int num_ifdefs = 0;
                    int num_endifs = 0;

                    for (int j = i; j < lines.Length; j++)
                    {

                        if (lines[j].Trim().StartsWith("#ifdef") || lines[j].Trim().StartsWith("#ifndef"))
                        {
                            num_ifdefs++;
                        }
                        else if (lines[j].Trim().StartsWith("#endif"))
                        {
                            num_endifs++;

                            if (num_endifs >= num_ifdefs)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (remove)
                            {
                                lines[j] = "";
                            }
                        }
                    }

                    lines[i] = "";
                }
                else if (lines[i].Trim().StartsWith("#ifndef"))
                {
                    inIfStatement = true;
                    ifStatementText = lines[i].Trim().Substring(8);
                    ifdefs.Add("!" + ifStatementText);
                    currentIfDef = "!" + ifStatementText;

                    bool remove = (properties.GetBool(ifStatementText));

                    int num_ifdefs = 0;
                    int num_endifs = 0;

                    for (int j = i; j < lines.Length; j++)
                    {

                        if (lines[j].Trim().StartsWith("#ifdef") || lines[j].Trim().StartsWith("#ifndef"))
                        {
                            num_ifdefs++;
                        }
                        else if (lines[j].Trim().StartsWith("#endif"))
                        {
                            num_endifs++;

                            if (num_endifs >= num_ifdefs)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (remove)
                            {
                                lines[j] = "";
                            }
                        }
                    }
                    lines[i] = "";
                }
                else if (lines[i].Trim().StartsWith("#endif"))
                {
                    lines[i] = "";
                }
                if (lines[i] != "")
                    res += lines[i] + "\n";
            }
            foreach (var val in properties.values)
            {
                res = res.Replace("$" + val.Key, GetValueString(val.Key, val.Value));
            }
         //   Console.WriteLine(res);
            return res;
        }

        public static string FormatShaderProperties_old(string origCode, ShaderProperties properties)
        {
            string res = "";
            string[] lines = origCode.Split('\n');
            bool inIfStatement = false;
            String ifStatementText = "";
            bool removing = false;
            List<string> ifdefs = new List<string>();
            List<string> ifndefs = new List<string>();
            string currentIfDef = "";
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Trim().StartsWith("#ifdef"))
                {
                    inIfStatement = true;
                    ifStatementText = lines[i].Trim().Substring(7);
                    ifdefs.Add(ifStatementText);
                    currentIfDef = ifStatementText;
                    if (properties.GetBool(ifStatementText) == true)
                        removing = false;
                    else
                        removing = true;
                    lines[i] = "";
                }
                else if (lines[i].Trim().StartsWith("#ifndef"))
                {
                    inIfStatement = true;
                    ifStatementText = lines[i].Trim().Substring(8);
                     if (properties.GetBool(ifStatementText) == false)
                          removing = false;
                      else
                          removing = true;
                    ifdefs.Add("!" + ifStatementText);
                    currentIfDef = "!" + ifStatementText;
                    lines[i] = "";
                }
                else if (lines[i].Trim().StartsWith("#endif"))
                {
                    if (inIfStatement)
                    {
                        inIfStatement = false;
                        removing = false;
                    }
                    lines[i] = "";
                }
                if (inIfStatement && removing)
                {
                    lines[i] = "";
                }
                if (lines[i] != "")
                    res += lines[i] + "\n";
            }
            foreach (var val in properties.values)
            {
                res = res.Replace("$" + val.Key, GetValueString(val.Key, val.Value));
            }
            return res;
        }
    }
}