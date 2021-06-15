using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;

public class Gem : MonoBehaviour
{
    private static SquadPickerLevel levelManager = null;

    private void Awake()
    {
        levelManager = (SquadPickerLevel)LevelManager.Instance;
    }
    private void OnTriggerEnter(Collider other)
    {
        Human human = other.GetComponent<Human>();
        if (human)
        {
            GetComponent<Collider>().enabled = false;
            levelManager.NumberOfGems++;
            ObjectPooler.Instance.SpawnFromPool("GemEffect", transform.position + Vector3.up * 0.5f, Quaternion.identity);
            GetComponentInChildren<MeshRenderer>().transform.LeanScale(Vector3.zero, 0.2f).setOnComplete(() => Destroy(gameObject));
        }
    }
}
