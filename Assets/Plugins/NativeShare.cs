#if UNITY_IOS
using System.Runtime.InteropServices;
using System;
#else
using UnityEngine;
using System.IO;
#endif
using Crosstales.FB;
using System;

/// <summary>
/// https://github.com/ChrisMaire/unity-native-sharing
/// </summary>
public static class NativeShare
{
    /// <summary>
    /// Shares on file maximum
    /// </summary>
    /// <param name="body">The default message of sms, mails, ...</param>
    /// <param name="filePath">The path to the attached file</param>
    /// <param name="url">The url to share</param>
    /// <param name="subject"></param>
    /// <param name="mimeType">The mime type of the file</param>
    /// <param name="chooser"></param>
    /// <param name="chooserText"></param>
    public static void Share(string body, string filePath = null, string url = null, string subject = "", string mimeType = "text/html", bool chooser = false, string chooserText = "Select sharing app")
    {
        ShareMultiple(body, new string[] { filePath }, url, subject, mimeType, chooser, chooserText);
    }

    /// <summary>
    /// Shares multiple files at once
    /// </summary>
    /// <param name="body">The default message of sms, mails, ...</param>
    /// <param name="filePath">The path to the attached file</param>
    /// <param name="url">The url to share</param>
    /// <param name="subject"></param>
    /// <param name="mimeType">The mime type of the file</param>
    /// <param name="chooser"></param>
    /// <param name="chooserText"></param>
    public static void ShareMultiple(string body, string[] filePaths = null, string url = null, string subject = "", string mimeType = "text/html", bool chooser = false, string chooserText = "Select sharing app")
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        SharePC(filePaths, subject, "", null, null);
#elif UNITY_ANDROID
        ShareAndroid(body, subject, url, filePaths, mimeType, chooser, chooserText);
#elif UNITY_IOS
		ShareIOS(body, subject, url, filePaths);
#else
        
#endif
    }

#if UNITY_ANDROID
    public static void ShareAndroid(string body, string subject, string url, string[] filePaths, string mimeType, bool chooser, string chooserText)
    {
        using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
        using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
        using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent"))
        {
            using (intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND")))
            { }
            using (intentObject.Call<AndroidJavaObject>("setType", mimeType))
            { }
            using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject))
            { }
            using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), body))
            { }

            if (!string.IsNullOrEmpty(url))
            {
                // attach url
                using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
                using (AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", url))
                using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject))
                { }
            }
            else if (filePaths != null & filePaths.Length > 0 & !string.IsNullOrEmpty(filePaths[0]))
            {
                // attach extra files (pictures, pdf, etc.)
                using (AndroidJavaClass fileProviderClass = new AndroidJavaClass("android.support.v4.content.FileProvider"))
                using (AndroidJavaObject unityContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
                using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
                using (AndroidJavaObject uris = new AndroidJavaObject("java.util.ArrayList"))
                {
                    string packageName = unityContext.Call<string>("getPackageName");
                    string authority = packageName + ".provider";

                    AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", filePaths[0]);
                    AndroidJavaObject uriObj = fileProviderClass.CallStatic<AndroidJavaObject>("getUriForFile", unityContext, authority, fileObj);

                    int FLAG_GRANT_READ_URI_PERMISSION = intentObject.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION");
                    intentObject.Call<AndroidJavaObject>("addFlags", FLAG_GRANT_READ_URI_PERMISSION);

                    using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObj))
                    { }
                }
            }

            // finally start application
            if (chooser)
            {
                AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, chooserText);
                currentActivity.Call("startActivity", jChooser);
            }
            else
            {
                currentActivity.Call("startActivity", intentObject);
            }
        }
    }
#endif

#if UNITY_IOS
	public struct ConfigStruct
	{
		public string title;
		public string message;
	}

	[DllImport ("__Internal")] private static extern void showAlertMessage(ref ConfigStruct conf);

	public struct SocialSharingStruct
	{
		public string text;
		public string subject;
		public string filePaths;
	}

	[DllImport ("__Internal")] private static extern void showSocialSharing(ref SocialSharingStruct conf);

	public static void ShareIOS(string title, string message)
	{
		ConfigStruct conf = new ConfigStruct();
		conf.title  = title;
		conf.message = message;
		showAlertMessage(ref conf);
	}

	public static void ShareIOS(string body, string subject, string url, string[] filePaths)
	{
		SocialSharingStruct conf = new SocialSharingStruct();
		conf.text = body;
		string paths = string.Join(";", filePaths);
		if (string.IsNullOrEmpty(paths))
			paths = url;
		else if (!string.IsNullOrEmpty(url))
			paths += ";" + url;
		conf.filePaths = paths;
		conf.subject = subject;

		showSocialSharing(ref conf);
	}
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
    /// <param name="filePath">The path to the file</param>
    /// <param name="title">Dialog title</param>
    /// <param name="directory">Root directory</param>
    /// <param name="fileName">Default file name</param>
    /// <param name="filters">Supported export formats</param>
    public static void SharePC(string filePath, string title = "", string directory = "", string fileName = "", ExtensionFilter[] filters = null)
    {
        SharePC(new string[] { filePath }, title, directory, new string[] { fileName }, filters);
    }

    /// <param name="filePath">The path to files</param>
    /// <param name="title">Dialog title</param>
    /// <param name="directory">Root directory</param>
    /// <param name="fileName">Default files name</param>
    /// <param name="filters">Supported export formats</param>
    public static void SharePC(string[] filePath, string title, string directory, string[] fileName, ExtensionFilter[] filters)
    {
        if (filePath.Length == 1)
        {
            string path = FileBrowser.SaveFile(title, directory, fileName[0], filters);
            if (!string.IsNullOrEmpty(path))
                File.Copy(filePath[0], path);
        }
        else if (filePath.Length > 1)
        {
            string path = FileBrowser.OpenSingleFolder(title, directory);
            if (!string.IsNullOrEmpty(path))
            {
                for (int i = 0; i < filePath.Length; i++)
                    File.Copy(filePath[i], path + fileName[i]);
            }
        }
    }
#endif
}
