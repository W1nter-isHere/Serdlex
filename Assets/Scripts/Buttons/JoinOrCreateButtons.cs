using UnityEngine;

namespace Buttons
{
    public class JoinOrCreateButtons : MonoBehaviour
    {
        public void Join()
        {
            SceneTransitioner.Instance.TransitionToScene(5);
        }

        public void Create()
        {
            SceneTransitioner.Instance.TransitionToScene(6);
        }
    }
}