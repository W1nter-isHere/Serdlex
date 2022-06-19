using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    public static SceneTransitioner Instance;

    [SerializeField] private CanvasGroup cover;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        
        Destroy(gameObject);
    }

    public void TransitionToScene(int scene)
    {
        StartCoroutine(Transition(scene));
    }

    private IEnumerator Transition(int scene)
    {
        LeanTween.value(gameObject, f => cover.alpha = f, 0, 1, 0.2f);
        
        yield return new WaitForSeconds(0.2f);
        yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        yield return new WaitForSeconds(0.2f);
        
        LeanTween.value(gameObject, f => cover.alpha = f, 1, 0, 0.2f);
    }
}