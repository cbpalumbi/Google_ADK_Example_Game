using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq; // Required for .Where() and .Select()

public class BuildScript
{
    // This is the method that will be called from the command line.
    // It takes the target platform (Windows/Linux) and output path.
    public static void PerformBuild()
    {
        Debug.Log("BuildScript: Starting PerformBuild method.");

        // Get command line arguments
        string[] args = System.Environment.GetCommandLineArgs();
        Debug.Log($"BuildScript: Command-line arguments received: {string.Join(" ", args)}");

        // --- 1. Determine Target Platform ---
        BuildTarget target = BuildTarget.StandaloneWindows64; // Default to Windows 64-bit
        Debug.Log("BuildScript: Default build target set to Windows 64-bit.");

        // Check for specific build target arguments
        if (System.Array.IndexOf(args, "-buildWindowsPlayer") != -1)
        {
            target = BuildTarget.StandaloneWindows64;
            Debug.Log("BuildScript: Build target set to Windows 64-bit based on arguments.");
        }
        else if (System.Array.IndexOf(args, "-buildLinuxPlayer") != -1)
        {
            target = BuildTarget.StandaloneLinux64;
            Debug.Log("BuildScript: Build target set to Linux 64-bit based on arguments.");
        }
        else if (System.Array.IndexOf(args, "-buildOSXPlayer") != -1)
        {
            target = BuildTarget.StandaloneOSX; // Allow building for macOS if explicitly requested
            Debug.Log("BuildScript: Build target set to macOS based on arguments.");
        }
        else
        {
            Debug.Log("BuildScript: No specific build target argument found. Retaining default Windows 64-bit.");
        }

        // --- 2. Determine Output Path ---
        string outputPath = "";
        for (int i = 0; i < args.Length; i++)
        {
            // Prefer the path specified directly after the build player argument
            if ((args[i] == "-buildOSXPlayer" || args[i] == "-buildWindowsPlayer" || args[i] == "-buildLinuxPlayer") && (i + 1 < args.Length))
            {
                outputPath = args[i + 1];
                // Ensure the output path has a file extension for the executable
                if (target == BuildTarget.StandaloneOSX)
                {
                    if (!outputPath.EndsWith(".app"))
                    {
                        outputPath += ".app";
                    }
                }
                else if (target == BuildTarget.StandaloneWindows64)
                {
                    if (!outputPath.EndsWith(".exe"))
                    {
                        outputPath += ".exe";
                    }
                }
                // Important: Ensure this path is absolute on the build machine (e.g., C:\Builds\MyGame\MyGame.exe)
                Debug.Log($"BuildScript: Output path detected from build player argument: {outputPath}");
                break; // Found the path, exit loop
            }
            // Fallback for -outputPath argument (less common for Unity's -buildPlayer methods, but good to keep)
            if (args[i] == "-outputPath" && i + 1 < args.Length)
            {
                outputPath = args[i + 1];
                Debug.Log($"BuildScript: Output path detected via -outputPath argument: {outputPath}");
                break; // Found the path, exit loop
            }
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            Debug.LogError("BuildScript: ERROR! Output path not specified. Exiting build process.");
            EditorApplication.Exit(1); // Exit with an error code
            return;
        }

        // --- 3. Determine Scenes To Build (ALL ENABLED SCENES) ---
        EditorBuildSettingsScene[] editorBuildSettingsScenes = EditorBuildSettings.scenes;
        Debug.Log($"BuildScript: Found {editorBuildSettingsScenes.Length} scenes in Editor Build Settings.");
        foreach (var scene in editorBuildSettingsScenes)
        {
            Debug.Log($"BuildScript: Scene in Build Settings: {scene.path} (Enabled: {scene.enabled})");
        }

        // Now, simply collect all enabled scenes from the build settings
        string[] scenesToBuild = editorBuildSettingsScenes.Where(s => s.enabled).Select(s => s.path).ToArray();

        if (scenesToBuild.Length == 0)
        {
            Debug.LogError("BuildScript: ERROR! No scenes are enabled in your Build Settings. Please add and enable at least one scene. Exiting build.");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"BuildScript: Scenes selected for build ({scenesToBuild.Length} total):");
        foreach (var scenePath in scenesToBuild)
        {
            Debug.Log($"BuildScript: - {scenePath}");
        }

        // --- 4. Perform the Build ---
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenesToBuild;
        buildPlayerOptions.locationPathName = outputPath;
        buildPlayerOptions.target = target;
        buildPlayerOptions.options = BuildOptions.None; // Add BuildOptions.Development for development builds to get full logs

        Debug.Log($"BuildScript: Invoking BuildPipeline.BuildPlayer for target '{target}' to path '{outputPath}'...");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"BuildScript: SUCCESS! Build completed in {report.summary.totalTime}. Output to: {outputPath}");
            EditorApplication.Exit(0); // Exit with success code
        }
        else if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
        {
            Debug.LogError($"BuildScript: FAILED! Build errors: {report.summary.totalErrors}, warnings: {report.summary.totalWarnings}. Check build report for details.");
            EditorApplication.Exit(1); // Exit with error code
        }
        else if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Cancelled)
        {
            Debug.LogWarning("BuildScript: WARNING! Build was cancelled.");
            EditorApplication.Exit(1); // Exit with error code
        }
        else
        {
            Debug.LogError($"BuildScript: UNKNOWN BUILD RESULT: {report.summary.result}.");
            EditorApplication.Exit(1); // Exit with error code
        }
    }
}