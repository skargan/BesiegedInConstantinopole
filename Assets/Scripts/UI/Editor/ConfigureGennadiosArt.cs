#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
public static class ConfigureGennadiosArt
{
    public static void Run(){
        const string path="Assets/Resources/UI/Identity/CourtFigures/203.png";
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Default;importer.isReadable=true;
        importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;
        importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
        importer.SaveAndReimport();
    }
}
#endif
