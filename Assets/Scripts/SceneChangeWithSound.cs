using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChangeWithSound : MonoBehaviour
{
    public string sceneToLoad;
    public AudioClip clickSound;

    public void OnButtonClick()
    {
        if (ButtonClickManager.Instance != null)
        {
            ButtonClickManager.Instance.PlayClickSound();
        }

        // Delay scene transition until the sound finishes playing
        StartCoroutine(LoadSceneAfterSound());
    }

    IEnumerator LoadSceneAfterSound()
    {
        if (clickSound != null)
        {
            yield return new WaitForSeconds(clickSound.length); // Wait for sound to finish
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}
