using UnityEngine;

public class Interaction : MonoBehaviour
{
    public AudioSource audioSource; // assign in inspector
    public GameObject obj;          // assign the clip you want to play

    // Call this method from UnityEvents or other scripts
    public void PlayAudio()
    {// play money
        ItemValue value = obj.GetComponent<ItemValue>();
        if (audioSource != null&& value!=null)
        {
            audioSource.PlayOneShot(value.GetAudioForPrice(value.cost));
            Debug.Log("Audio played!");
        }
        else
        {
            Debug.LogWarning("AudioSource or clip not assigned on " + gameObject.name);
        }
    }
}
