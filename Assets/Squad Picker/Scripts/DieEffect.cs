using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieEffect : MonoBehaviour, IPooledObject
{
    private ParticleSystem effect = null;

    private void Awake()
    {
        effect = GetComponent<ParticleSystem>();
    }
    public void OnObjectSpawn()
    {
        effect.Play();
    }
}
