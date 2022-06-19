using System.Collections.Generic;
using UnityEngine;

public class GlobalRandom : MonoBehaviour
{
    public static GlobalRandom Instance;

    public List<int> chosen;

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

    public int RandUniqueInt(int min, int max)
    {
        int r;
        
        do
        {
            r = Random.Range(min, max + 1);
        } while (chosen.Contains(r));

        chosen.Add(r);
        
        return r;
    }
}