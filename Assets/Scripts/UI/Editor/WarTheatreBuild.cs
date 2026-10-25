using UnityEditor;
using UnityEngine;
public static class WarTheatreBuild {
 public static void Build(){
  foreach(string name in new[]{"WarTheatreGeography","WarTheatreSymbols"}){
   var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/UI/Maps/"+name+".png");
   importer.textureType=TextureImporterType.Default;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=4096;importer.mipmapEnabled=true;importer.filterMode=FilterMode.Trilinear;importer.alphaIsTransparency=true;importer.SaveAndReimport();
  }
  WarTheatre.Validate(WarTheatre.Data);ConfigurePlaytest.Build();
 }
}
