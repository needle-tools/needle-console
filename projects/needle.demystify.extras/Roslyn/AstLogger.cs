using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnityEngine;
using UnityEditor;

public class AstLogger : EditorWindow
{
    private string inputText;
    private string outputText;

    [MenuItem("Needle/AST Logger Window")]
    static void LoggerWindow() => EditorWindow.GetWindow<AstLogger>().Show();
    
    private void OnGUI()
    {
        inputText = EditorGUILayout.TextArea(inputText, GUILayout.Height(200));
        EditorGUILayout.Space();
        if (GUILayout.Button("Parse"))
        {
            outputText = Parse(inputText);
        }

        EditorGUILayout.TextArea(outputText, GUILayout.Height(200));
    }

    private string Parse(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input);
        var root = tree.GetRoot();
        var sb = new StringBuilder();
        var pre = "";
        
        void AddNode(SyntaxNode node, string pr)
        {
            pr += "  ";
            var tk = node.GetFirstToken();
            sb.AppendLine(pr + tk + " (" + tk.Kind() + ")");
            foreach (var child in node.ChildNodes())
            {
                AddNode(child, pr);
            }
        }
        
        AddNode(root, pre);

        return sb.ToString();
    }
}
