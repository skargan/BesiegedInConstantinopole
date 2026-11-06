#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using TMPro;
using UnityEngine;

// Called by an isolated exported-player capture; no campaign values are changed.
public static class NativeNameChecks
{
    public static int Run(Transform surface){
        int count=0;Action<bool,string> check=(ok,message)=>{if(!ok)throw new Exception(message);count++;};
        var prior=DiplomaticNames.Mode;
        try {
            DiplomaticNames.Mode=DiplomaticNames.Style.Original;
            var original=DiplomaticNames.OriginalTypeface;
            check(original!=null&&original.sourceFontFile.name=="NotoSerif-Regular","Original names use Noto Serif primary");
            check(Mathf.Approximately(original.faceInfo.scale,1),"Native family has normal point-size scale");
            check(Mathf.Approximately(original.atlasPadding/(float)original.faceInfo.pointSize,.1f),"Consistent native SDF padding ratio");
            check(ImperialTheme.Font.fallbackFontAssetTable[0]==original,"Serif fallback precedes generic default");
            check(original.fallbackFontAssetTable.Any(f=>f.sourceFontFile.name=="NotoSerifGeorgian-Regular"),"Georgian serif included");
            check(original.fallbackFontAssetTable.Any(f=>f.sourceFontFile.name=="NotoNaskhArabic-Regular"),"Arabic Naskh included");
            foreach(var entry in DiplomaticNames.All){uint[] missing;check(original.HasCharacters(entry.Display,out missing,true,true),"Native coverage "+entry.Id);}
            foreach(string id in new[]{"serbia","wallachia","moldavia","georgia","karaman"}){
                var label=CampaignHUD.Label(surface,"",Vector2.zero,Vector2.one,28);DiplomaticNames.Bind(label,id);Canvas.ForceUpdateCanvases();label.ForceMeshUpdate();
                check(label.font==original&&label.text==DiplomaticNames.Name(id),"Bound Original family/text "+id);
                foreach(var glyph in label.textInfo.characterInfo.Take(label.textInfo.characterCount).Where(c=>c.character>=0x400&&c.character<=0x52f))
                    check(glyph.fontAsset==original,"Cyrillic uses serif rather than generic sans "+id);
                check(label.GetComponents<RealmNameHeading>().Length==1,"One heading binding "+id);DiplomaticNames.Bind(label,id);check(label.GetComponents<RealmNameHeading>().Length==1,"Rebind reuses component "+id);
                DiplomaticNames.Mode=DiplomaticNames.Style.Full;label.GetComponent<RealmNameHeading>().Refresh();check(label.font==ImperialTheme.Font&&label.text==DiplomaticNames.Name(id),"English family restored "+id);
                DiplomaticNames.Mode=DiplomaticNames.Style.Original;label.GetComponent<RealmNameHeading>().Refresh();UnityEngine.Object.Destroy(label.gameObject);
            }
            return count;
        }finally{DiplomaticNames.Mode=prior;}
    }
}
#endif
