using System.IO;
using UnityEngine;

public static class ContentPath
{
    public static string Root
    {
        get
        {
#if UNITY_EDITOR
            // In Editor: Assets/../Content → ProjectRoot/Content
            return Path.GetFullPath(Path.Combine(Application.dataPath, "../Content"));
#else
            // In Build: AppFolder/Content
            return Path.GetFullPath(Path.Combine(Application.dataPath, "../Content"));
#endif
        }
    }

    public static string RootFolder(string filename)
    {
        return Path.Combine(Root, filename);
    }
    
    public static string Audio(string languageCode, string filename)
    {
        return Path.Combine(Root, "Audio", languageCode, filename);
    }
    
    public static string Config(string filename)
    {
        return Path.Combine(Root, "Config", filename);
    }
    
    public static string Image(string filename)
    {
        return Path.Combine(Root, "Image", filename);
    }

    public static string Font(string filename)
    {
        return Path.Combine(Root, "Font", filename);
    }
    
    public static string Video(string filename)
    {
        return Path.Combine(Root, "Video", filename);
    }

    public static string Translation(string languageCode)
    {
        return Path.Combine(Root, "Translation", $"{languageCode}.json");
    }
}