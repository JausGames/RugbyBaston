using UnityEngine;

[System.Serializable]
public class PlayerSoundManager
{
    [SerializeField] private AudioClip[] walkClips;
    [SerializeField] private AudioClip[] runClips;

    [SerializeField] private AudioSource audioSource;


    internal void PlayFootstep()
    {
        audioSource.clip = walkClips[UnityEngine.Random.Range(0, walkClips.Length)];
        audioSource.Play();

        /*audioSource.Stop();
        audioSource.PlayOneShot(walkClips[UnityEngine.Random.Range(0, walkClips.Length)]);*/
    }
}
