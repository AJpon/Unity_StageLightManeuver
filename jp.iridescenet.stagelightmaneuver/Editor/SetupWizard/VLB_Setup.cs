using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if VLB_URP || VLB_HDRP
#define VLB_INSTALLED
#endif

namespace StageLightManeuver
{
    public class IntegrationSetup : EditorWindow
    {
#if VLB_INSTALLED
        private const bool VLB_INSTALLED = true; 
#else
        private const bool VLB_INSTALLED = false;
#endif
        string const WINDOW_TITLE = "Setup SLM Integration";

        [MenuItem("Window/StageLightManeuver/" + WINDOW_TITLE)]
        private static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(IntegrationSetup), false, WINDOW_TITLE);
        }

        private void OnGUI()
        {
            // EditorApplication.projectChanged += () => UpdatePackageInfoFromManifest(); // プロジェクトが変更されたらパッケージ情報を更新
            minSize = new(440, 260);
            titleContent = new GUIContent(WINDOW_TITLE);
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.richText = true;

            EditorGUILayout.LabelField("Setup VLB Integration", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            // 各ステータスを表示 
            EditorGUILayout.LabelField("Integration Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // VLB Integration のインストールステータス
            // - VLB のインストールステータス
            // - VLB のシンボル定義ステータス
            // - VLB の API にアクセスできるか
            EditorGUILayout.LabelField("VLB Integration", VLB_INSTALLED ? "Installed" : "Not Installed");
        }


        private void InstallIntegration()
        {
            // install Editor/Resources/Package/slm_vlb_integration.unitypackage
            // guid: dc6aaec98c31a6144bc94065bb624bb7 
            var PACKAGE_GUID = "dc6aaec98c31a6144bc94065bb624bb7";
            var packagePath = AssetDatabase.GUIDToAssetPath(PACKAGE_GUID);
            AssetDatabase.ImportPackage(packagePath, true);
        }

        /// <summary>
        /// USE_VLB シンボルが定義済みであれば true を返します
        /// </summary>
        /// <returns></returns>
        private bool CheckSymbolDefine()
        {
#if USE_VLB
            return true
            
#else
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            // if (!symbols.Contains("USE_VLB"))
            // {
            //     symbols += ";USE_VLB";
            //     PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            // }
            return symbols.Contains("USE_VLB")
#endif
        }

        /// <summary>
        /// VLB の API にアクセスできるかチェックします
        /// </summary>
        /// <returns></returns>
        private bool CheckVLBAvailable()
        {
            try
            {
                var vlb = new VLB.VolumetricLightBeam();
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}
