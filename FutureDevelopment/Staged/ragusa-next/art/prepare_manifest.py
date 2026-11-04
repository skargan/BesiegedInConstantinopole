from pathlib import Path
import json, hashlib
from PIL import Image

root=Path(__file__).resolve().parent
polygons={
 'gazulus_foreground':[[.228,.209],[.266,.236],[.285,.290],[.287,.373],[.279,.427],[.289,.467],[.318,.493],[.334,.545],[.368,.604],[.387,.677],[.398,.745],[.348,.770],[.258,.760],[.210,.794],[.090,.840],[.000,.880],[.000,.758],[.030,.625],[.066,.510],[.105,.446],[.157,.405],[.156,.328],[.154,.281],[.169,.247],[.191,.221]],
 'cotrugli_rear':[[.363,.081],[.410,.078],[.438,.111],[.439,.181],[.427,.232],[.448,.274],[.461,.324],[.474,.390],[.501,.437],[.528,.486],[.535,.531],[.523,.575],[.503,.560],[.498,.523],[.493,.649],[.495,.779],[.420,.791],[.391,.708],[.365,.608],[.337,.530],[.295,.472],[.275,.402],[.292,.327],[.327,.263],[.346,.220],[.345,.153]],
 'cotrugli_foreground':[[.195,.201],[.245,.211],[.275,.258],[.277,.311],[.263,.374],[.259,.417],[.283,.471],[.316,.512],[.334,.567],[.367,.616],[.393,.684],[.408,.777],[.333,.786],[.246,.784],[.131,.833],[.000,.871],[.000,.744],[.035,.615],[.061,.524],[.092,.468],[.145,.421],[.139,.346],[.140,.280],[.148,.239]],
 'gazulus_rear':[[.370,.088],[.407,.094],[.430,.127],[.437,.173],[.425,.223],[.412,.246],[.437,.276],[.457,.329],[.476,.395],[.504,.446],[.525,.491],[.531,.544],[.516,.591],[.498,.566],[.494,.528],[.486,.653],[.474,.801],[.416,.799],[.386,.715],[.360,.605],[.331,.522],[.288,.453],[.268,.400],[.281,.330],[.314,.276],[.355,.235],[.354,.172]],
}
assets=[]
for filename,front,rear in [('ragusa-gazulus-foreground.png','gazulus','cotrugli'),('ragusa-cotrugli-foreground.png','cotrugli','gazulus')]:
 p=root/filename;im=Image.open(p)
 characters=[]
 for person,plane,priority in [(front,'foreground',20),(rear,'rear',10)]:
  polygon=polygons[person+'_'+plane]
  characters.append({'id':person,'displayName':'Johannes Gazulus' if person=='gazulus' else 'Benedetto Cotrugli','depth':plane,'hitPriority':priority,'polygonTopLeft':polygon,'polygonUnityBottomLeft':[[x,round(1-y,4)] for x,y in polygon],'hoverAnchorTopLeft':[.24,.25] if plane=='foreground' else [.39,.12]})
 assets.append({'file':filename,'width':im.width,'height':im.height,'mode':im.mode,'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'bytes':p.stat().st_size,'suggestedResourcePath':'UI/Diplomacy/Ragusa/'+('GazulusForeground' if front=='gazulus' else 'CotrugliForeground'),'foreground':front,'rear':rear,'characters':characters})
result={'targetVersion':38,'generatedDate':'2026-11-04','coordinateConvention':'Normalized over the actual fitted painting; top-left x right/y down and Unity bottom-left equivalents provided. Do not stretch image.','uiSafeRegionTopLeft':{'x':.62,'y':.04,'width':.36,'height':.92},'hoverNotes':'Approximate silhouettes measured from the actual generated images. Foreground receives hit priority where their robes overlap. Characters are painted into the scene in this version; no independent sprites are implied. Fit the painting first, then transform polygons with the same rectangle.','assets':assets,'visualReview':{'bothMenPresent':True,'identityAndCostumeMaintainedAcrossSwap':True,'noTextOrUiBakedIn':True,'rightmost38PercentClearOfPeople':True,'chessTableInLeftHalf':True,'noMajorHandAnatomyDefectObserved':True,'matchingArchitectureAndHarbourComposition':True,'unityRuntimeCheck':'not performed; artwork-only task'}}
(root/'art-manifest.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'images':len(assets),'dimensions':[(x['width'],x['height']) for x in assets],'uiStartsAt':.62},indent=2))
