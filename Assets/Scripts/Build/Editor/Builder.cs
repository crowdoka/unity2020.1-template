namespace Crowdoka.ProjectName.Build.Editor
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    public static class Builder
    {
        private const string AppName = "ProjectName";
        private const string TargetDir = "builds";

        private static readonly string[] Scenes =
            (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();

        [MenuItem("Custom/CI/Build Windows")]
        private static void PerformWindowsBuild()
        {
            const string platform = "win64";
            const string extension = "exe";
            PerformBuild(platform, extension, BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64,
                BuildOptions.None);
        }

        [MenuItem("Custom/CI/Build OSX")]
        private static void PerformOSXBuild()
        {
            const string platform = "osx";
            const string extension = "app";
            PerformBuild(platform, extension, BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX,
                BuildOptions.None);
        }

        private static void PerformBuild(string platform, string extension, BuildTargetGroup buildTargetGroup,
            BuildTarget buildTarget, BuildOptions buildOptions)
        {
            var path = Path.Combine(TargetDir, platform);
            GenericBuild(Scenes, Path.Combine(path, $"{AppName}-{platform}.{extension}"), buildTargetGroup, buildTarget,
                buildOptions);
            ZipFile.CreateFromDirectory(path, Path.Combine(TargetDir, $"{AppName}-{platform}.zip"));
        }

        private static void GenericBuild(string[] scenes, string targetDir, BuildTargetGroup buildTargetGroup,
            BuildTarget buildTarget, BuildOptions buildOptions)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = targetDir,
                targetGroup = buildTargetGroup,
                target = buildTarget,
                options = buildOptions
            });

            if (report.summary.result != BuildResult.Failed) return;

            var error =
                (from step in report.steps
                    from message in step.messages
                    where message.type == LogType.Error
                    select message).Aggregate("", (current, message) => current + "\n" + message.content);
            throw new Exception($"BuildPlayer failure\n{error}");
        }
    }
}