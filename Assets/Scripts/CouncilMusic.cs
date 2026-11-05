using UnityEngine;

// Starts the council theme on the existing music source; navigation does not stop it.
public sealed class CouncilMusic : MonoBehaviour
{
    private void LateUpdate()
    {
        var source = Settings.Instance.audioSource;
        if (source == null) return;
        if(SoundtrackLibrary.Enabled&&SoundtrackLibrary.Instance.Count>0){enabled=false;return;}
        var track = Resources.Load<AudioClip>("Audio/Music/The Last Ison");
        if (track == null) { enabled = false; return; }
        if (source.clip != track)
        {
            source.Stop();
            source.clip = track;
            source.loop = true;
            source.Play();
        }
        // Music mute/volume remain owned by Settings. No exit handler: keep playing.
        enabled = false;
    }
}
