using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Publication assets only: reads the original PNGs without changing their import settings.
public static class ExportAchievementArt
{
    [MenuItem("Besieged/Publication/Export Steam achievement icons")]
    public static void Run()
    {
        string root=Directory.GetParent(Application.dataPath).FullName;
        string source=Path.Combine(Application.dataPath,"Resources/UI/MidnightMosaic/Achievements");
        string output=Path.Combine(root,"Publication/Steam/AchievementIcons");
        foreach(string id in AchievementPlatform.Ids)
            if(!File.Exists(Path.Combine(source,id+".png")))throw new FileNotFoundException("Achievement artwork is missing: "+id);
        Directory.CreateDirectory(output);
        foreach(string id in AchievementPlatform.Ids)
        {
            var original=new Texture2D(2,2,TextureFormat.RGBA32,false);
            var result=new Texture2D(256,256,TextureFormat.RGBA32,false);
            try
            {
                if(!original.LoadImage(File.ReadAllBytes(Path.Combine(source,id+".png"))))throw new InvalidDataException("Cannot read artwork: "+id);
                if(original.width!=original.height)throw new InvalidDataException("Achievement artwork must be square: "+id);
                var pixels=original.GetPixels32();var reduced=new Color32[256*256];
                // Area-weighted downsampling works in batch mode without a graphics device.
                double scale=(double)original.width/256;
                for(int y=0;y<256;y++)for(int x=0;x<256;x++)
                {
                    double left=x*scale,right=(x+1)*scale,bottom=y*scale,top=(y+1)*scale;
                    double red=0,green=0,blue=0,alpha=0,area=0;
                    for(int sy=(int)Math.Floor(bottom);sy<Math.Ceiling(top);sy++)
                        for(int sx=(int)Math.Floor(left);sx<Math.Ceiling(right);sx++)
                        {
                            double weight=(Math.Min(right,sx+1)-Math.Max(left,sx))*(Math.Min(top,sy+1)-Math.Max(bottom,sy));
                            Color32 c=pixels[sy*original.width+sx];
                            // Premultiplied colour avoids dark fringes on transparent source art.
                            double coverage=weight*c.a/255.0;
                            red+=c.r*coverage;green+=c.g*coverage;blue+=c.b*coverage;alpha+=coverage;area+=weight;
                        }
                    reduced[y*256+x]=alpha>0?new Color32((byte)Math.Round(red/alpha),(byte)Math.Round(green/alpha),(byte)Math.Round(blue/alpha),(byte)Math.Round(255*alpha/area)):new Color32(0,0,0,0);
                }
                result.SetPixels32(reduced);result.Apply(false,false);
                File.WriteAllBytes(Path.Combine(output,id+"_ACHIEVED.png"),result.EncodeToPNG());
            }
            finally{UnityEngine.Object.DestroyImmediate(original);UnityEngine.Object.DestroyImmediate(result);}
        }
        Debug.Log("STEAM_ACHIEVEMENT_ART: exported "+AchievementPlatform.Ids.Length+" achieved icons at 256 x 256 to "+output);
    }
}
