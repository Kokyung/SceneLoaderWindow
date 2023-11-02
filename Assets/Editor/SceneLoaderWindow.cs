using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BKK.EditorSceneManagement
{
    public class SceneLoaderWindow : EditorWindow
    {
        private SceneLoaderData sceneLoaderData;

        private static readonly string windowTitle = "Scene Loader";
        private readonly string pathLabel = "Path: ";
        private readonly string chooseSceneFolderLabel = "Choose Scene Folder";
        private readonly string autoLoadOnPlayLabel = "Auto Load On Play";
        private readonly string targetSceneLabel = "Target Scene";
        private readonly string loadSceneModeLabel = "Load Scene Mode";
        private readonly string noSceneInPathLabel = "No Scene in Path.";

        private GUIStyle warningTextStyle;
        
        /// <summary>
        /// Scene Loader Window 메뉴 클릭시 윈도우를 연다.
        /// </summary>
        [MenuItem("BKK/Editor Scene Management/Scene Loader")]
        private static void Init()
        {
            SceneLoaderWindow window = GetWindow<SceneLoaderWindow>();
            window.titleContent = new GUIContent(windowTitle);
        }

        /// <summary>
        /// InitializeOnLoadMethod에 의해 유니티 에디터 로드 중에 호출된다.
        /// 플레이 모드 변경시 호출할 LoadDefaultScene 메서드를 등록한다.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void InitAutoLoadScene()
        {
            EditorApplication.playModeStateChanged += LoadDefaultScene;
        }

        /// <summary>
        /// Edit 
        /// </summary>
        /// <param name="state"></param>
        private static void LoadDefaultScene(PlayModeStateChange state)
        {
            // Edit 모드 나갈때 현재 수정 상태를 저장한다.
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            // 플레이 모드에 진입할때 autoLoadSceneOnPlay가 True이면 목표 Scene을 로드한다. 
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                SceneLoaderData sceneLoaderData = SceneLoaderData.GetData();
                
                if(!sceneLoaderData.autoLoadSceneOnPlay) return;

                LoadSceneParameters loadSceneParameters = new LoadSceneParameters(sceneLoaderData.loadSceneMode);
                
                EditorSceneManager.LoadSceneAsyncInPlayMode(sceneLoaderData.GetAutoLoadTargetScenePath(), loadSceneParameters);
            }
        }

        private void OnEnable()
        {
            // Scene Loader Data의 존재 여부 확인. 없으면 생성.
            sceneLoaderData = Resources.Load<SceneLoaderData>(SceneLoaderPath.defaultResourcesFilePath);

            if (sceneLoaderData == null)
            {
                sceneLoaderData = SceneLoaderData.CreateData();
            }
            
            sceneLoaderData.GetScenePathInFolder();// 폴더 내 Scene 에셋들의 경로를 가져온다.
            
            // 폴더에 Scene 에셋이 없을때 출력할 안내문 스타일.
            if (warningTextStyle == null)
                warningTextStyle = new GUIStyle
                {
                    normal =
                    {
                        textColor = Color.yellow,
                    },
                };
        }

        private void OnGUI()
        {
            Draw();
        }

        private void Draw()
        {
            sceneLoaderData.GetScenePathInFolder();
            
            if(GUILayout.Button(chooseSceneFolderLabel))
            {
                string newPath = GetFolderPath();

                if (!string.IsNullOrEmpty(newPath))
                    sceneLoaderData.relativeFolderPath = newPath;
            }
            
            GUILayout.Label(pathLabel + sceneLoaderData.relativeFolderPath);
            
            if (sceneLoaderData.sceneDataList.Count == 0)
            {
                EditorGUILayout.LabelField(noSceneInPathLabel, warningTextStyle);
                return;
            }
            
            DrawSceneList();
            DrawPlayModeMenu();
        }

        private void DrawSceneList()
        {
            DrawLine();

            for (int i = 0; i < sceneLoaderData.sceneDataList.Count; i++)
            {
                string sceneName = sceneLoaderData.sceneDataList[i].sceneName;

                if (GUILayout.Button(sceneName))
                {
                    EditorSceneManager.OpenScene(sceneLoaderData.sceneDataList[i].scenePath);
                }
            }
            DrawLine();
        }

        private void DrawPlayModeMenu()
        {
            sceneLoaderData.autoLoadSceneOnPlay = EditorGUILayout.Toggle(autoLoadOnPlayLabel, sceneLoaderData.autoLoadSceneOnPlay);

            if (sceneLoaderData.autoLoadSceneOnPlay)
            {
                string[] optionList = new string[sceneLoaderData.sceneDataList.Count];

                for (int i = 0; i < sceneLoaderData.sceneDataList.Count; i++)
                {
                    optionList.SetValue(sceneLoaderData.sceneDataList[i].sceneName, i);
                }

                sceneLoaderData.targetSceneEnumIndex = EditorGUILayout.Popup(targetSceneLabel, sceneLoaderData.targetSceneEnumIndex, optionList);

                sceneLoaderData.loadSceneMode = (LoadSceneMode)EditorGUILayout.EnumPopup(loadSceneModeLabel, sceneLoaderData.loadSceneMode);
            }
        }

        private void DrawLine()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        /// <summary>
        /// 폴더 경로 지정 패널을 열고 지정한 경로를 리턴한다.
        /// </summary>
        /// <returns></returns>
        private string GetFolderPath()
        {
            string absolutePath = EditorUtility.OpenFolderPanel(chooseSceneFolderLabel, Application.dataPath, "");

            if (string.IsNullOrEmpty(absolutePath)) return string.Empty;
            
            return absolutePath.Substring(absolutePath.IndexOf("Assets", StringComparison.Ordinal));
        }
    }
}