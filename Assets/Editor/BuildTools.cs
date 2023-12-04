using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public enum BuildPlatforms
{
    WIN,
    Linux,
    OSX,
    Andorid
}

public class BuildTools
{
    const string k_RelativeBuildFolder = "Builds";
    const string k_WinClientFolder = "Matchplay-WIN";
    const string k_OSXClientFolder = "Matchplay-OSX";
    const string k_AndroidClientFolder = "Matchplay-Android";
    const string k_LinuxServerFolder = "Matchplay-LinuxServer";

    [MenuItem("BuildTools/All")]
    public static void BuildAll()
    {
        BuildWINClient();
        BuildLinuxServer();
        BuildOSXClient();
        BuildAndroidClient();
    }

    [MenuItem("BuildTools/WIN Client")]
    public static void BuildWINClient()
    {
        BuildForPlatform(BuildPlatforms.WIN);
    }

    [MenuItem("BuildTools/Linux Server")]
    public static void BuildLinuxServer()
    {
        BuildForPlatform(BuildPlatforms.Linux, true);
    }

    [MenuItem("BuildTools/OSX Client")]
    public static void BuildOSXClient()
    {
        BuildForPlatform(BuildPlatforms.OSX);
    }

    [MenuItem("BuildTools/Android Client")]
    public static void BuildAndroidClient()
    {
        BuildForPlatform(BuildPlatforms.Andorid);
    }

    static void BuildForPlatform(BuildPlatforms platform, bool server = false)
    {
        var startBuildTarget = EditorUserBuildSettings.activeBuildTarget;
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = AllBuildScenePaths();
        var locationPath = k_RelativeBuildFolder;
        var timeStamp = DateTime.Now.ToFileTimeUtc();
        switch (platform)
        {
            case BuildPlatforms.WIN:
                {
                    locationPath = Path.Combine(locationPath, k_WinClientFolder + $"_{timeStamp}", "Matchplay.exe");
                    buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
                    break;
                }
            case BuildPlatforms.Linux:
                {
                    locationPath = Path.Combine(locationPath, k_LinuxServerFolder + $"_{timeStamp}", "Matchplay.x86_64");
                    buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
                    break;
                }
            case BuildPlatforms.OSX:
                locationPath = Path.Combine(locationPath, k_OSXClientFolder + $"_{timeStamp}", "Matchplay.app");
                buildPlayerOptions.target = BuildTarget.StandaloneOSX;
                break;
            case BuildPlatforms.Andorid:
                locationPath = Path.Combine(locationPath, k_AndroidClientFolder + $"_{timeStamp}", "Matchplay.apk");
                buildPlayerOptions.target = BuildTarget.Android;
                break;
        }

        buildPlayerOptions.locationPathName = locationPath;
        buildPlayerOptions.options = BuildOptions.Development;

        if (server)
            buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;
        switch (summary.result)
        {
            case BuildResult.Succeeded:
                Debug.Log($"{platform} succeeded after {summary.totalTime} seconds @ {summary.outputPath}");
                break;
            case BuildResult.Failed:
                Debug.Log($"{platform} failed after {summary.totalTime} seconds");
                break;
            case BuildResult.Unknown:
                Debug.Log($"{platform} hit an unknown after {summary.totalTime} seconds.");
                break;
            case BuildResult.Cancelled:
                Debug.Log($"{platform} cancelled after {summary.totalTime} seconds.");
                break;
        }
        //Switch back to the build target we were at (if applicable)
        if (EditorUserBuildSettings.activeBuildTarget != startBuildTarget)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, startBuildTarget);

    }

    public static string[] AllBuildScenePaths()
    {
        List<string> scenePaths = new List<string>();

        foreach (var scene in EditorBuildSettings.scenes)
        {
            Debug.Log($"Adding Scene: {scene.path} ");
            scenePaths.Add(scene.path);
        }

        return scenePaths.ToArray();
    }
}