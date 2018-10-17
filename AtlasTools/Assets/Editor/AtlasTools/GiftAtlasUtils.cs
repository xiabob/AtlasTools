using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.IO;
using System.Linq;
using UnityEngine.U2D;
using UnityEditor.U2D;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor.PackageManager;


namespace GiftAtlasTools
{

    public class GiftAtlasConfigWindow : EditorWindow
    {
        private static string resourcesRootPath;
        private string resourcesButton = "选择原图根目录";

        private static string atlasRootPath;
        private string atlasButton = "选择图集根目录";

        private static string ignoreResourcesFolders;

        private static string atlasRelativeResourcesPath;


        [MenuItem("AtlasTools/设置Atlas配置")]
        public static void ConfigAtlas()
        {
            GiftAtlasConfig originConfig = GetConfig();
            if (originConfig != null)
            {
                resourcesRootPath = originConfig.ResourcesRootPath;
                atlasRootPath = originConfig.AtlasRootPath;
                atlasRelativeResourcesPath = originConfig.AtlasPathInResources;
                ignoreResourcesFolders = originConfig.IgnoreResourcesFolders;
            }
            EditorWindow.GetWindow(typeof(GiftAtlasConfigWindow)).Show();
        }

        private static void CreateConfigAsset(GiftAtlasConfig so)
        {
            if (so == null)
            {
                Debug.Log("该对象无效，无法将对象实例化");
                return;
            }

            //自定义配置资源路径
            string path = Application.dataPath + "/Editor/AtlasTools/Resources";

            //判断该路径是否存在，不存在的话创建一个
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            //配置文件以.asset结尾
            //将对象名配置成与类名相同
            path = string.Format("Assets/Editor/AtlasTools/Resources/{0}.asset", "AtlasConfig");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            //按指定路径生成配置文件
            AssetDatabase.CreateAsset(so, path);
            AssetDatabase.Refresh();
        }

        public static bool HasConfig()
        {
            GiftAtlasConfig config = GetConfig();
            return config != null && !string.IsNullOrEmpty(config.ResourcesRootPath) && !string.IsNullOrEmpty(config.AtlasRootPath);
        }

        public static GiftAtlasConfig GetConfig()
        {
            GiftAtlasConfig config = Resources.Load<GiftAtlasConfig>("AtlasConfig");
            return config;
        }

        void OnGUI()
        {
            GUILayout.Label("原图根目录", EditorStyles.boldLabel);
            if (GUILayout.Button(resourcesButton))
            {
                resourcesRootPath = EditorUtility.OpenFolderPanel("", Application.dataPath + resourcesRootPath, "").Replace(Application.dataPath, "");
            }
            resourcesRootPath = EditorGUILayout.TextField("点击按钮选取路径", resourcesRootPath);

            GUILayout.Space(8);

            GUILayout.Label("图集根目录", EditorStyles.boldLabel);
            if (GUILayout.Button(atlasButton))
            {
                atlasRootPath = EditorUtility.OpenFolderPanel("", Application.dataPath + atlasRootPath, "").Replace(Application.dataPath, "");
            }
            atlasRootPath = EditorGUILayout.TextField("点击按钮选取路径", atlasRootPath);

            GUILayout.Space(8);

            GUILayout.Label("忽略目录列表", EditorStyles.boldLabel);
            ignoreResourcesFolders = EditorGUILayout.TextField("填写名称，以”，“分隔", ignoreResourcesFolders);

            GUILayout.Space(8);

            GUILayout.Label("图集相对Resources路径", EditorStyles.boldLabel);
            atlasRelativeResourcesPath = EditorGUILayout.TextField("填写路径", atlasRelativeResourcesPath);

            if (GUILayout.Button("保存设置"))
            {
                GiftAtlasConfig config = HasConfig() ? GetConfig() : ScriptableObject.CreateInstance<GiftAtlasConfig>();
                config.ResourcesRootPath = resourcesRootPath;
                config.AtlasRootPath = atlasRootPath;
                config.AtlasPathInResources = string.IsNullOrEmpty(atlasRelativeResourcesPath) ? "" : atlasRelativeResourcesPath;
                config.IgnoreResourcesFolders = string.IsNullOrEmpty(ignoreResourcesFolders) ? "" : ignoreResourcesFolders;
                if (!HasConfig()) CreateConfigAsset(config);

                EditorWindow.GetWindow(typeof(GiftAtlasConfigWindow)).Close();
            }

        }
    }


    // https://forum.unity.com/threads/creating-a-spriteatlas-from-code.511400/
    public class GiftAtlasMaker
    {

        private static void UpdateAtlasSprite()
        {
            LoopSetTexture();
        }

        // private 

        /// <summary>
        /// 循环设置选择的贴图
        /// </summary>
        private static void LoopSetTexture()
        {
            List<Texture2D> textures = GetResourcesTextures();
            Selection.objects = new Object[0];
            foreach (Texture2D texture in textures)
            {
                string path = AssetDatabase.GetAssetPath(texture);
                TextureImporter texImporter = GetTextureSettings(path);
                TextureImporterSettings tis = new TextureImporterSettings();
                texImporter.ReadTextureSettings(tis);
                texImporter.SetTextureSettings(tis);
                //更改设置后需要重新导入，否则会更改失效
                AssetDatabase.ImportAsset(path);
            }
        }

        /// <summary>
        /// 获取贴图设置
        /// </summary>
        private static TextureImporter GetTextureSettings(string path)
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            return textureImporter;
        }

        /// <summary>
        /// 获取选择的贴图
        /// </summary>
        /// <returns></returns>
        private static List<Texture2D> GetResourcesTextures()
        {
            // return Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
            string dirPath = sptSrcDir;
            List<Texture2D> spts = new List<Texture2D>();
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            foreach (FileInfo pngFile in dirInfo.GetFiles("*.png", SearchOption.AllDirectories))
            {
                string allPath = pngFile.FullName;
                string assetPath = allPath.Substring(allPath.IndexOf("Assets"));
                Texture2D sprite = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

                spts.Add(sprite);
            }

            return spts;
        }

        private static string sptDesDir = Application.dataPath + "/Resources";
        private static string sptSrcDir = Application.dataPath + "/Art";
        private static string atlasRelativePath = "";
        private static string[] ignoreFolders = new string[] { };

        [MenuItem("AtlasTools/按目录打包图集")]
        public static void CreateAtlasByFolders()
        {
            if (!CheckConfig()) return;

            UpdateAtlasSprite();

            DirectoryInfo rootDirInfo = new DirectoryInfo(sptSrcDir);
            //add folders
            List<Object> folders = new List<Object>();
            foreach (DirectoryInfo dirInfo in rootDirInfo.GetDirectories())
            {
                if (dirInfo == null || ignoreFolders.Contains(dirInfo.Name)) continue;

                folders.Clear();
                string assetPath = dirInfo.FullName.Substring(dirInfo.FullName.IndexOf("Assets"));
                var o = AssetDatabase.LoadAssetAtPath<DefaultAsset>(assetPath);
                if (IsPackable(o)) folders.Add(o);

                string atlasName = dirInfo.Name + ".spriteatlas";
                if (IsAtlasExists(atlasName))
                {
                    SpriteAtlas atlas = Resources.Load<SpriteAtlas>(atlasRelativePath + dirInfo.Name);
                    RefreshAtlas(atlas);
                    AssetDatabase.Refresh();

                    continue;
                }
                CreateAtlas(atlasName);
                SpriteAtlas sptAtlas = Resources.Load<SpriteAtlas>(atlasRelativePath + dirInfo.Name);
                Debug.Log(sptAtlas.tag);
                AddPackAtlas(sptAtlas, folders.ToArray());
            }

            //add texture by your self
        }

        [MenuItem("AtlasTools/按目录打包图集（不更新图片导入格式）")]
        public static void CreateAtlasByFoldersWithoutUpdateSprite()
        {
            if (!CheckConfig()) return;

            DirectoryInfo rootDirInfo = new DirectoryInfo(sptSrcDir);
            //add folders
            List<Object> folders = new List<Object>();
            foreach (DirectoryInfo dirInfo in rootDirInfo.GetDirectories())
            {
                if (dirInfo == null || ignoreFolders.Contains(dirInfo.Name)) continue;

                folders.Clear();
                string assetPath = dirInfo.FullName.Substring(dirInfo.FullName.IndexOf("Assets"));
                var o = AssetDatabase.LoadAssetAtPath<DefaultAsset>(assetPath);
                if (IsPackable(o)) folders.Add(o);

                string atlasName = dirInfo.Name + ".spriteatlas";
                if (IsAtlasExists(atlasName))
                {
                    SpriteAtlas atlas = Resources.Load<SpriteAtlas>(atlasRelativePath + dirInfo.Name);
                    RefreshAtlas(atlas);
                    AssetDatabase.Refresh();

                    continue;
                }
                CreateAtlas(atlasName);
                SpriteAtlas sptAtlas = Resources.Load<SpriteAtlas>(atlasRelativePath + dirInfo.Name);
                Debug.Log(sptAtlas.tag);
                AddPackAtlas(sptAtlas, folders.ToArray());
            }

            //add texture by your self
        }

        // [MenuItem("AtlasTools/AtlasMaker By Sprite")]
        public static void CreateAtlasBySprite()
        {
            if (!CheckConfig()) return;

            UpdateAtlasSprite();

            DirectoryInfo rootDirInfo = new DirectoryInfo(sptSrcDir);

            //add sprite

            List<Sprite> spts = new List<Sprite>();
            foreach (DirectoryInfo dirInfo in rootDirInfo.GetDirectories())
            {
                spts.Clear();
                foreach (FileInfo pngFile in dirInfo.GetFiles("*.png", SearchOption.AllDirectories))
                {
                    string allPath = pngFile.FullName;
                    string assetPath = allPath.Substring(allPath.IndexOf("Assets"));
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    if (IsPackable(sprite))
                        spts.Add(sprite);
                }
                string atlasName = dirInfo.Name + ".spriteatlas";
                if (IsAtlasExists(atlasName))
                {
                    SpriteAtlas atlas = Resources.Load<SpriteAtlas>(atlasRelativePath + dirInfo.Name);
                    RefreshAtlas(atlas);
                    AssetDatabase.Refresh();

                    continue;
                }
                CreateAtlas(atlasName);
                SpriteAtlas sptAtlas = Resources.Load<SpriteAtlas>(atlasRelativePath + dirInfo.Name);
                Debug.Log(sptAtlas.tag);
                AddPackAtlas(sptAtlas, spts.ToArray());
            }


            //add texture by your self
        }

        private static bool CheckConfig()
        {
            if (!GiftAtlasConfigWindow.HasConfig())
            {
                GiftAtlasConfigWindow.ConfigAtlas();
                return false;
            }
            else
            {
                GiftAtlasConfig config = GiftAtlasConfigWindow.GetConfig();
                sptDesDir = Application.dataPath + config.AtlasRootPath;
                sptSrcDir = Application.dataPath + config.ResourcesRootPath;
                atlasRelativePath = config.AtlasPathInResources;

                if (!string.IsNullOrEmpty(config.IgnoreResourcesFolders))
                {
                    string[] ignores = config.IgnoreResourcesFolders.Split(new string[] { "," }, System.StringSplitOptions.None);
                    if (ignores != null) ignoreFolders = ignores;
                }

                return true;
            }
        }

        static bool IsPackable(Object o)
        {
            return o != null && (o.GetType() == typeof(Sprite) || o.GetType() == typeof(Texture2D) || (o.GetType() == typeof(DefaultAsset) && ProjectWindowUtil.IsFolder(o.GetInstanceID())));
        }

        static void AddPackAtlas(SpriteAtlas atlas, Object[] spt)
        {
            MethodInfo methodInfo = System.Type
                 .GetType("UnityEditor.U2D.SpriteAtlasExtensions, UnityEditor")
                 .GetMethod("Add", BindingFlags.Public | BindingFlags.Static);
            if (methodInfo != null)
                methodInfo.Invoke(null, new object[] { atlas, spt });
            else
                Debug.Log("methodInfo is null");
            PackAtlas(atlas);
        }

        static void PackAtlas(SpriteAtlas atlas)
        {
            System.Type
                .GetType("UnityEditor.U2D.SpriteAtlasUtility, UnityEditor")
                .GetMethod("PackAtlases", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { new[] { atlas }, EditorUserBuildSettings.activeBuildTarget });
        }

        private static void RefreshAtlas(SpriteAtlas atlas)
        {
            PackAtlas(atlas);
        }

        private static bool IsAtlasExists(string atlasName)
        {
            string filePath = sptDesDir + "/" + atlasName;
            return File.Exists(filePath);
        }

        public static void CreateAtlas(string atlasName)
        {
            string yaml = @"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!687078895 &4343727234628468602
SpriteAtlas:
  m_ObjectHideFlags: 0
  m_PrefabParentObject: {fileID: 0}
  m_PrefabInternal: {fileID: 0}
  m_Name: New Sprite Atlas
  m_EditorData:
    textureSettings:
      serializedVersion: 2
      anisoLevel: 1
      compressionQuality: 50
      maxTextureSize: 2048
      textureCompression: 0
      filterMode: 1
      generateMipMaps: 0
      readable: 0
      crunchedCompression: 0
      sRGB: 1
    platformSettings: []
    packingParameters:
      serializedVersion: 2
      padding: 4
      blockOffset: 1
      allowAlphaSplitting: 0
      enableRotation: 0
      enableTightPacking: 0
    variantMultiplier: 1
    packables: []
    bindAsDefault: 1
  m_MasterAtlas: {fileID: 0}
  m_PackedSprites: []
  m_PackedSpriteNamesToIndex: []
  m_Tag: New Sprite Atlas
  m_IsVariant: 0
";
            AssetDatabase.Refresh();

            if (!Directory.Exists(sptDesDir))
            {
                Directory.CreateDirectory(sptDesDir);
                AssetDatabase.Refresh();
            }
            string filePath = sptDesDir + "/" + atlasName;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                AssetDatabase.Refresh();
            }
            FileStream fs = new FileStream(filePath, FileMode.CreateNew);
            byte[] bytes = new UTF8Encoding().GetBytes(yaml);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
            AssetDatabase.Refresh();
        }

    }

}