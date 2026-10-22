using System;
using UnityEngine;
using UnityEngine.UI;
public static class SceneParticleTests {
 public static void Run(){int checks=0;bool pref=SceneParticles.Enabled;var state=UnityEngine.Random.state;var root=new GameObject("Particle test",typeof(RectTransform));try{
 foreach(string key in new[]{"market-peace","market-war","walls-peace","walls-war","intelligence","assault-battlefield","ladder-wall"}){SceneParticles.Attach(root,key);if(root.transform.childCount!=24)throw new Exception("Pool must stay at 24");checks++;foreach(var image in root.GetComponentsInChildren<RawImage>()){if(image.raycastTarget)throw new Exception("Effect intercepts controls");checks++;}}
 var sample=UnityEngine.Random.value;UnityEngine.Random.state=state;if(UnityEngine.Random.value!=sample)throw new Exception("Decorative effects changed campaign randomness");checks++;
 SceneParticles.Enabled=false;root.GetComponent<SceneParticles>().SendMessage("Update");foreach(var image in root.GetComponentsInChildren<RawImage>()){if(image.enabled)throw new Exception("Disabled particle visible");checks++;}
 if(SceneParticles.KindFor("diplomacy")!=null||SceneParticles.KindFor("fleet-peace")!=null)throw new Exception("Unsupported scene got atmosphere");checks++;Debug.Log("SCENE_PARTICLE_CHECKS "+checks+" passed");
 }finally{SceneParticles.Enabled=pref;UnityEngine.Random.state=state;UnityEngine.Object.DestroyImmediate(root);}}
}
