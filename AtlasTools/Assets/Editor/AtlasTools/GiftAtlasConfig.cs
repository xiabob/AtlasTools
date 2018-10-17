using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GiftAtlasTools
{
    [System.Serializable]
    public class GiftAtlasConfig : ScriptableObject
    {
        public string ResourcesRootPath; //离散图片所在根目录路径
        public string AtlasRootPath; //图集文件根目录路径
        public string IgnoreResourcesFolders; //忽略目录与，以“,”分隔
        public string AtlasPathInResources; //图集根目录在Resources文件夹中的相对路径
    }
}


