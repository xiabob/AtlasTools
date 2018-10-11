using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using System.Collections;
using System.Collections.Generic;

public class GiftSpriteUtils
{

    // https://unity3d.com/cn/learn/tutorials/topics/interface-essentials/unity-editor-extensions-menu-items
    [MenuItem("AtlasTools/设置选中图片导入格式为Sprite(2D and UI)")]
    private static void UpdateAtlasSprite()
    {
        GiftSpriteUtils utils = new GiftSpriteUtils();
        utils.LoopSetTexture();
    }

    // private 

    /// <summary>
    /// 循环设置选择的贴图
    /// </summary>
    private void LoopSetTexture()
    {
        Object[] textures = GetSelectedTextures();
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
    public TextureImporter GetTextureSettings(string path)
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        textureImporter.textureType = TextureImporterType.Sprite;
        return textureImporter;
    }

    /// <summary>
    /// 获取选择的贴图
    /// </summary>
    /// <returns></returns>
    private Object[] GetSelectedTextures()
    {
        return Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
    }
}