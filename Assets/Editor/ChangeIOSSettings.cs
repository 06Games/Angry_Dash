using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using UnityEditor.iOS.Xcode;
using System.IO;

public class ChangeIOSSettings
{

    [PostProcessBuild]
    public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {

        if (buildTarget == BuildTarget.iOS)
        {

            // Get plist
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            PlistElementDict rootDict = plist.root;

            // Allow file sharing
            rootDict.SetBoolean("UIFileSharingEnabled", true);
            rootDict.SetBoolean("LSSupportsOpeningDocumentsInPlace", true);

            // Write to file
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}