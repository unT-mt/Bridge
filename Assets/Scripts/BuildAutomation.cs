// using UnityEditor;
// using UnityEngine;

// public class BuildAutomation
// {
//     [MenuItem("Build/Build Table and Wall Scenes")]
//     public static void BuildTableAndWallScenes()
//     {
//         // Tableシーンの設定
//         string tableScene = "Assets/Scenes/TableScene.unity";
//         string tablePath = "Desktop/wwo_table/Build";
//         BuildPlayerOptions tableBuildOptions = new BuildPlayerOptions
//         {
//             scenes = new[] { tableScene },
//             locationPathName = tablePath + "/TableBuild.exe",
//             target = BuildTarget.StandaloneWindows64,
//             options = BuildOptions.None
//         };
        
//         // ビルド前に解像度を設定
//         Screen.SetResolution(1920, 1200, false);
//         BuildPipeline.BuildPlayer(tableBuildOptions);

//         // Wallシーンの設定
//         string wallScene = "Assets/Scenes/WallScene.unity";
//         string wallPath = "Desktop/wwo_wall/Build";
//         BuildPlayerOptions wallBuildOptions = new BuildPlayerOptions
//         {
//             scenes = new[] { wallScene },
//             locationPathName = wallPath + "/WallBuild.exe",
//             target = BuildTarget.StandaloneWindows64,
//             options = BuildOptions.None
//         };
        
//         // 解像度を変更
//         Screen.SetResolution(1920, 1080, false);
//         BuildPipeline.BuildPlayer(wallBuildOptions);
//     }
// }
