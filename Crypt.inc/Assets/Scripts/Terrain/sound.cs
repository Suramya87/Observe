using UnityEngine;

public class AnimSfx : MonoBehaviour
{
    public AudioSource source;
    public AudioClip clip;

    public void PlaySfx()
    {
        if (source && clip)
            source.PlayOneShot(clip);
    }
}
