using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;

public class Saw : MonoBehaviour
{
    [SerializeField] private Transform mesh;
    private static SquadPickerLevel levelManager = null;
    private Collider[] colliders = null;
    private Human human;
    private Vector3 desiredPos;
    private Vector3 initialMeshPosition;
    private float time = 0;
    private Vector3 pos;
    private void Awake()
    {
        if (!levelManager)
            levelManager = (SquadPickerLevel)LevelManager.Instance;
        initialMeshPosition = mesh.position;
    }
    void Update()
    {
        mesh.Rotate(Vector3.up * Time.deltaTime * 360);
        desiredPos = initialMeshPosition + transform.right * (Mathf.PingPong(Time.time * 2, 6) - 3);
        mesh.position = desiredPos;
        colliders = Physics.OverlapBox(mesh.transform.position, new Vector3(0.75f, 1, 0.25f), Quaternion.identity, levelManager.HumanLayerMask);
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(mesh.transform.position, new Vector3(1.5f, 2, 0.5f));
    }

}
