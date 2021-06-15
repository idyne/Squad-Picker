using FSG.MeshAnimator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FateGames;
using System.Linq;

public class NewHuman : MonoBehaviour, IPooledObject
{
    private Rigidbody rb = null;
    private static NewTroop troop = null;
    private HumanState state = HumanState.IDLE;
    private Stack<GameObject> humansOnShoulder = null;
    private int size = 0;
    private static SquadPickerLevel levelManager = null;
    [SerializeField] private int maxSize = 6;
    [SerializeField] private MeshAnimator meshAnimator = null;
    [SerializeField] private BoxCollider boxCollider = null;
    public float LastInteractionTime = 0;
    public NewTroop.PositionInTroop PositionInTroop = null;

    public int Size { get => size; }
    public Rigidbody Rb { get => rb; }
    public MeshAnimator MeshAnimator { get => meshAnimator; }
    public HumanState State { get => state; }

    private void Awake()
    {
        if (!troop)
            troop = FindObjectOfType<NewTroop>();
        if (!levelManager)
            levelManager = (SquadPickerLevel)LevelManager.Instance;
        rb = GetComponent<Rigidbody>();
        humansOnShoulder = new Stack<GameObject>();
    }



    public void OnObjectSpawn()
    {
        rb.isKinematic = true;
        while (humansOnShoulder.Count > 0)
        {
            humansOnShoulder.Pop().SetActive(false);
        }
        if (GameManager.Instance.State == GameManager.GameState.STARTED)
            ChangeState(HumanState.RUNNING);
    }

    public void Fall(Vector3 force)
    {
        rb.isKinematic = false;
        rb.AddForce(force, ForceMode.Impulse);
        LeanTween.delayedCall(gameObject, 5f, () => { gameObject.SetActive(false); });
    }

    public void ChangeState(HumanState newState)
    {
        state = newState;
        if (state == HumanState.IDLE)
            meshAnimator.Play(0);
        else if (state == HumanState.RUNNING)
            meshAnimator.Play(1);
        else if (state == HumanState.RUNNING_CARRYING)
            meshAnimator.Play(2);
        else if (state == HumanState.IDLE_CARRYING)
            meshAnimator.Play(3);
    }

    public void AddHumanOnShoulder()
    {
        if (state == HumanState.RUNNING)
            ChangeState(HumanState.RUNNING_CARRYING);
        GameObject sittingHuman;
        if (size < maxSize)
        {
            if (size > 0)
            {
                sittingHuman = humansOnShoulder.Pop();
                GameObject sittingCarryingHuman = ObjectPooler.Instance.SpawnFromPool("SittingCarryingHuman", Vector3.up * 100, Quaternion.identity);
                FixedJoint sittingHumanJoint = sittingHuman.GetComponent<FixedJoint>();
                FixedJoint sittingCarryingHumanJoint = sittingCarryingHuman.GetComponent<FixedJoint>();
                sittingCarryingHuman.transform.position = sittingHuman.transform.position;
                sittingCarryingHuman.transform.rotation = transform.rotation;
                sittingHuman.transform.position += Vector3.up * 0.6f;
                sittingCarryingHumanJoint.connectedBody = size > 1 ? humansOnShoulder.Peek().GetComponent<Rigidbody>() : rb;
                sittingHumanJoint.connectedBody = sittingCarryingHuman.GetComponent<Rigidbody>();
                humansOnShoulder.Push(sittingCarryingHuman);
                humansOnShoulder.Push(sittingHuman);
            }
            else
            {
                sittingHuman = ObjectPooler.Instance.SpawnFromPool("SittingHuman", Vector3.zero, Quaternion.identity);
                FixedJoint sittingHumanJoint = sittingHuman.GetComponent<FixedJoint>();
                sittingHuman.transform.position = transform.position + Vector3.up * (0.6f);
                sittingHuman.transform.rotation = transform.rotation;
                sittingHumanJoint.connectedBody = rb;
                humansOnShoulder.Push(sittingHuman);
            }
            ExpandCollider();
        }
        size++;

    }

    public bool RemoveHumanOnShoulder()
    {
        if (size > 0)
        {
            if (size <= maxSize)
            {
                if (size > 1)
                {
                    GameObject sittingHuman = humansOnShoulder.Pop();
                    humansOnShoulder.Pop().SetActive(false);
                    sittingHuman.transform.position -= Vector3.up * 0.6f;
                    humansOnShoulder.Push(sittingHuman);
                }
                else if (size == 1)
                {
                    humansOnShoulder.Pop().SetActive(false);
                    if (state == HumanState.RUNNING_CARRYING)
                        ChangeState(HumanState.RUNNING);
                }
                ShrinkCollider();
            }
            size--;
            return true;
        }
        return false;
    }

    private void ExpandCollider()
    {
        boxCollider.size += Vector3.up * 0.6f;
        boxCollider.center += Vector3.up * 0.3f;
    }

    private void ShrinkCollider()
    {
        boxCollider.size -= Vector3.up * 0.6f;
        boxCollider.center -= Vector3.up * 0.3f;
    }

    public void Move(float Speed)
    {
        if (state == HumanState.IDLE)
            ChangeState(HumanState.RUNNING);
        rb.MovePosition(transform.position + (troop.transform.position + PositionInTroop.Position - transform.position) * Time.fixedDeltaTime * Speed);
        Vector3 rot = Quaternion.LookRotation(troop.transform.position + PositionInTroop.Position - transform.position).eulerAngles;
        rot.x = 0;
        rb.MoveRotation(Quaternion.Euler(rot));
    }

    public bool CheckGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up, Vector3.down);
    }

    public enum HumanState { IDLE, RUNNING, RUNNING_CARRYING, IDLE_CARRYING }
}
