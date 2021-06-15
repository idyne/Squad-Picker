using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemEffect : MonoBehaviour, IPooledObject
{
    [SerializeField] private ParticleSystem effect;

    public void OnObjectSpawn()
    {
        effect.Play();
    }
}
