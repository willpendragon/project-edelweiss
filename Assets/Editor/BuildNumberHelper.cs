using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildNumberHelper : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    private const string initialVersion = "0.9";

    public void OnPreprocessBuild(BuildReport report)

    {
        string currentversion = FindCurrentVersion();
        UpdateVersion(currentversion);
    }

    private string FindCurrentVersion()
    {
        string[] currentVersion = PlayerSettings.bundleVersion.Split('[', ']');
        return currentVersion.Length == 1 ? initialVersion : currentVersion[1];
    }

    private void UpdateVersion(string version)
    {
        if (float.TryParse(version, out float versionNumber))
        {
            float newVersion = versionNumber + 0.01f;
            string date = DateTime.Now.ToString("d");

            PlayerSettings.bundleVersion = $"[{newVersion}] {date}"; ;
        }
    }

}
