using UnityEngine;

namespace Buttons
{
    public class PlayAgainButton : MonoBehaviour
    {
        public void PlayAgain()
        {
            SceneTransitioner.Instance.TransitionToScene(7);
        }
    }
}
