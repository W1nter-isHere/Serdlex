using UnityEngine;

namespace Game
{
    public class WinScreen : GameOverScreen
    {
        [SerializeField] private Transform leftConfetti;
        [SerializeField] private Transform rightConfetti;

        public override void GameOver()
        {
            base.GameOver();
            
        }
    }
}