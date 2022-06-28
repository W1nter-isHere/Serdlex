using UnityEngine;

namespace UIElements
{
    public class PlayAgainButton : MonoBehaviour
    {
        public void PlayAgain()
        {
            SceneTransitioner.Instance.TransitionToScene(7);
        }
    }
}
