using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;

public class WreckingWall : MonoBehaviour
{
    [SerializeField] private Transform mesh;
    [SerializeField] private Transform ball;
    private static SquadPickerLevel levelManager = null;
    private Collider[] colliders = null;
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
        //mesh.LeanRotateZ(-50, 1.5f).setEaseOutCubic().setOnComplete(()=> { mesh.LeanRotateZ(50, 1.5f).setEaseOutCubic(); }) ;

    }
    void Update()
    {
        //   mesh.rotation = Quaternion.Euler(0, 0, easeOutCubic(Mathf.PingPong(Time.time, 1)) * 100 - 50);
        float MaxAngleDeflection = 50.0f;
        float SpeedOfPendulum = 1.0f;

        float angle = MaxAngleDeflection * Mathf.Sin(Time.time * SpeedOfPendulum);
        mesh.localRotation = Quaternion.Euler(0, 0, angle);
        colliders = Physics.OverlapSphere(ball.transform.position, 0.5f, levelManager.HumanLayerMask);
        time = Time.time;
        foreach (Collider collider in colliders)
        {
            human = collider.GetComponent<Human>();
            if (human.IsActive && human.LastInteractionTime + 0.1f < time)
            {
                pos = collider.ClosestPointOnBounds(ball.position);
                //ObjectPooler.Instance.SpawnFromPool("DieEffect", pos, Quaternion.identity);
                levelManager.Troop.DecrementHuman(human);
                human.LastInteractionTime = time;
            }

        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(ball.transform.position, 0.5f);
    }
}
