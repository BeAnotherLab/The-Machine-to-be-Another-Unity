using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEngine;

public class CopyContentOnBuild : IPostprocessBuildWithReport
{
    // Lower number = earlier execution
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        // Folder inside your project that you want to copy
        string sourceFolder = Path.Combine(Application.dataPath, "../MyFolderToCopy");

        if (!Directory.Exists(sourceFolder))
        {
            UnityEngine.Debug.LogWarning($"Source folder not found: {sourceFolder}");
            return;
        }

        // Build output location
        string buildPath = report.summary.outputPath;

        // Directory where the executable lives
        string buildDirectory = Path.GetDirectoryName(buildPath);

        // Destination folder next to the executable
        string destinationFolder = Path.Combine(buildDirectory, "MyFolderToCopy");

        CopyDirectory(sourceFolder, destinationFolder);

        UnityEngine.Debug.Log("Folder copied successfully!");
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string targetFilePath = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, targetFilePath, true);
        }

        foreach (string directory in Directory.GetDirectories(sourceDir))
        {
            string targetSubDir = Path.Combine(targetDir, Path.GetFileName(directory));
            CopyDirectory(directory, targetSubDir);
        }
    }
}