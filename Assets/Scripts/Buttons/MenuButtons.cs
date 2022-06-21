using UnityEngine;

namespace Buttons
{
    public class MenuButtons : MonoBehaviour
    {
        public void Play()
        {
            SceneTransitioner.Instance.TransitionToScene(3);
        }

        public void Credits()
        {
            SceneTransitioner.Instance.TransitionToScene(1);
        }

        public void Options()
        {
            SceneTransitioner.Instance.TransitionToScene(9);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}