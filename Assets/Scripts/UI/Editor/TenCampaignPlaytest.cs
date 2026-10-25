using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad] public static class TenCampaignPlaytest {
    static TenCampaignPlaytest(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("TenCampaigns",false)){SessionState.SetBool("TenCampaigns",false);new UnityEngine.GameObject("Ten Campaign Probe").AddComponent<TenCampaignProbe>();}};}
    public static void Run(){ValidateStory.Run();SessionState.SetBool("TenCampaigns",true);EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");EditorApplication.EnterPlaymode();}
}
