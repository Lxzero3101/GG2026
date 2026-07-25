using UnityEngine;

public class TugOfWarAudio : MonoBehaviour
{
    public AudioSource sfxSource;
    public AudioClip inZoneClip;
    public AudioClip outZoneClip;

    private bool wasInZone = false;

    public void UpdateZoneState(bool isInGreenZone)
    {
        if (isInGreenZone && !wasInZone)
            sfxSource.PlayOneShot(inZoneClip, 0.4f);
        else if (!isInGreenZone && wasInZone)
            sfxSource.PlayOneShot(outZoneClip, 0.5f);

        wasInZone = isInGreenZone;
    }
}