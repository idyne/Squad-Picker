using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
using UnityEngine.AI;

public class HumanSpawner : MonoBehaviour
{
    [SerializeField] private GameObject humanPrefab = null;
    private Transform troop = null;
    private List<NavMeshAgent> agents = new List<NavMeshAgent>();
    [SerializeField] private bool clk = false;
    ObjectPooler objectPooler;

    private void Awake()
    {
        objectPooler = ObjectPooler.Instance;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.J))
            SpawnHuman(5);
        if (Input.GetMouseButtonDown(0))
            clk = !clk;
        Move(clk ? Vector3.forward : Vector3.right);*/

    }
    private void SpawnHuman(int count)
    {
        if (!troop)
        {
            troop = new GameObject("Troop").transform;
        }
        for (int i = 0; i < count; i++)
        {
            GameObject humanObj = objectPooler.SpawnFromPool("Human", troop.position + new Vector3(Random.value, 0, Random.value) * 0.5f, Quaternion.identity);
            humanObj.transform.parent = troop;
            agents.Add(humanObj.GetComponent<NavMeshAgent>());
        }
    }

    private void Move(Vector3 direction)
    {
        troop.position = Vector3.MoveTowards(troop.position, troop.position + direction, Time.deltaTime * 5);
    }
}
