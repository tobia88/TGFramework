using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq;

public class TGBuildGame : MonoBehaviour
{
	public static void Build()
	{
		string format = "{0}\\{1}\\{1}.exe";
		// 从命令行中获取发布路径
		string buildPath = string.Format(format, Environment.GetCommandLineArgs().Last(), Application.productName);
		Debug.Log("Build Path: " + buildPath);

		// 编译
		BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath, BuildTarget.StandaloneWindows64, BuildOptions.None);
	}
}