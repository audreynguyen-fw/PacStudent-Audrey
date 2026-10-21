using UnityEngine;
using UnityEngine.SceneManagement; 

public class StartMenuManager : MonoBehaviour
{
    [Header("Audio References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    
    public void LoadLevel1()
    {
        PlaySound(clickSound);
        SceneManager.LoadScene("Level1Scene"); 
    }

    public void LoadLevel2()
    {
        PlaySound(clickSound);
        SceneManager.LoadScene("InnovationScene"); 
    }
    
    public void PlayHoverSound()
    {
        PlaySound(hoverSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}