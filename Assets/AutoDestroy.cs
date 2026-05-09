using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float destroyTime = 2f;

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
