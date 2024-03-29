﻿using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class AndroidBuild : MonoBehaviour
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.Android)
        {
            FileInfo build = new FileInfo(pathToBuiltProject);
            File.Delete(build.DirectoryName + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(build.FullName) + "-" + Application.version + "-v" + PlayerSettings.Android.bundleVersionCode + ".symbols.zip");
            File.Delete(build.DirectoryName + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(build.FullName) + "_mapping.txt");
        }
    }
}