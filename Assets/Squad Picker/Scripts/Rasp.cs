using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;

public class Rasp : MonoBehaviour
{
    [SerializeField] private Transform mesh;
    private static SquadPickerLevel levelManager = null;
    private Human human;
    private float time = 0;
    private Vector3 pos;

    private void Awake()
    {
        if (!levelManager)
            levelManager = (SquadPickerLevel)LevelManager.Instance;
    }

    private void Start()
    {
    }
    void Update()
    {
        mesh.Rotate(-Vector3.up * Time.deltaTime * 720);
        Collider[] colliders = Physics.OverlapCapsule(mesh.transform.position + Vector3.up * 0.6f, mesh.transform.position + Vector3.up * 1.65f, 0.6f, levelManager.HumanLayerMask);
        time = Time.time;
        foreach (Collider collider in colliders)
        {
            human = collider.GetComponent<Human>();
            if (human.IsActive && human.LastInteractionTime + 0.1f < time)
            {
                pos = collider.ClosestPoint(transform.position);
                //ObjectPooler.Instance.SpawnFromPool("DieEffect", pos, Quaternion.identity);
                levelManager.Troop.DecrementHuman(human);
                human.LastInteractionTime = time;
            }

        }
    }
}
