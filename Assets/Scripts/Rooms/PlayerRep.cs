using UnityEngine;

namespace Rooms
{
    public class PlayerRep : MonoBehaviour
    {
        [SerializeField] private GameObject checkmark;

        private void Start()
        {
            checkmark.SetActive(false);
        }

        public void Ready()
        {
            checkmark.SetActive(!checkmark.activeSelf);
        }
    }
}
