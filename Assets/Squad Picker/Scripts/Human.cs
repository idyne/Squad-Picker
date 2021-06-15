using FSG.MeshAnimator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FateGames;
using System.Linq;

public class Human : MonoBehaviour, IPooledObject
{
    private Rigidbody rb = null;
    private static Troop troop = null;
    private HumanState state = HumanState.IDLE;
    private Stack<GameObject> humansOnShoulder = null;
    private int size = 0;
    private static SquadPickerLevel levelManager = null;
    [SerializeField] private int maxSize = 11;
    [SerializeField] private MeshAnimator meshAnimator = null;
    [SerializeField] private BoxCollider boxCollider = null;
    public float LastInteractionTime = 0;
    public Troop.PositionInTroop PositionInTroop = null;
    public string History = "";
    public int Size { get => size; }
    public Rigidbody Rb { get => rb; }
    public MeshAnimator MeshAnimator { get => meshAnimator; }
    public HumanState State { get => state; }
    public bool IsActive { get; set; }
    public BoxCollider BoxCollider { get => boxCollider; }

    private void Awake()
    {
        if (!troop)
            troop = FindObjectOfType<Troop>();
        if (!levelManager)
            levelManager = (SquadPickerLevel)LevelManager.Instance;
        rb = GetComponent<Rigidbody>();
        humansOnShoulder = new Stack<GameObject>();
    }



    public void OnObjectSpawn()
    {
        boxCollider.size = new Vector3(0.5f, 1.8f, 0.5f);
        boxCollider.center = new Vector3(0, 0.65f, 0);
        boxCollider.enabled = true;
        rb.isKinematic = true;
        IsActive = true;
        if (PositionInTroop != null)
            print(History);
        while (humansOnShoulder.Count > 0)
            humansOnShoulder.Pop().SetActive(false);
        size = 0;
        if (GameManager.Instance.State == GameManager.GameState.STARTED)
            ChangeState(HumanState.RUNNING);
    }

    public void Fall()
    {
        rb.isKinematic = false;
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
        else if (state == HumanState.WALKING)
            meshAnimator.Play(4);
        else if (state == HumanState.WALKING_CARRYING)
            meshAnimator.Play(5);
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
                if (size > 1)
                {
                    sittingCarryingHuman.transform.position = humansOnShoulder.Peek().transform.position + humansOnShoulder.Peek().transform.up * 0.6f;
                    sittingCarryingHuman.transform.rotation = humansOnShoulder.Peek().transform.rotation;
                }
                else
                {
                    sittingCarryingHuman.transform.position = transform.position + transform.up * 0.6f;
                    sittingCarryingHuman.transform.rotation = transform.rotation;
                }
                sittingHuman.transform.position = sittingCarryingHuman.transform.position + sittingCarryingHuman.transform.up * 0.6f;
                sittingHuman.transform.rotation = sittingCarryingHuman.transform.rotation;
                sittingCarryingHumanJoint.connectedBody = size > 1 ? humansOnShoulder.Peek().GetComponent<Rigidbody>() : rb;
                sittingHumanJoint.connectedBody = sittingCarryingHuman.GetComponent<Rigidbody>();
                humansOnShoulder.Push(sittingCarryingHuman);
                humansOnShoulder.Push(sittingHuman);
                Renderer sittingCarryingHumanRenderer = sittingCarryingHuman.GetComponentInChildren<Renderer>();
                sittingCarryingHumanRenderer.material.color = Color.black;
                sittingCarryingHumanRenderer.gameObject.LeanColor(levelManager.HumanColor, 0.5f);
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
            ObjectPooler.Instance.SpawnFromPool("AddEffect", sittingHuman.transform.position + Vector3.up * 0.5f, Quaternion.identity).transform.parent = sittingHuman.transform;
            Renderer sittingHumanRenderer = sittingHuman.GetComponentInChildren<Renderer>();
            sittingHumanRenderer.material.color = Color.black;
            sittingHumanRenderer.gameObject.LeanColor(levelManager.HumanColor, 0.5f);
            ExpandCollider();
            foreach (GameObject human in humansOnShoulder)
                human.GetComponent<FixedJoint>().massScale = 1.5f*(size + 1) / (float)maxSize;
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
                    FixedJoint sittingHumanJoint = sittingHuman.GetComponent<FixedJoint>();
                    humansOnShoulder.Pop().SetActive(false);
                    sittingHumanJoint.connectedBody = null;
                    if (size > 2)
                    {
                        sittingHuman.transform.position = humansOnShoulder.Peek().transform.position + humansOnShoulder.Peek().transform.up * 0.6f;
                        sittingHuman.transform.rotation = humansOnShoulder.Peek().transform.rotation;
                    }
                    else
                    {
                        sittingHuman.transform.position = transform.position + transform.up * 0.6f;
                        sittingHuman.transform.rotation = transform.rotation;
                    }
                    sittingHumanJoint.connectedBody = size > 2 ? humansOnShoulder.Peek().GetComponent<Rigidbody>() : rb;
                    humansOnShoulder.Push(sittingHuman);
                }
                else if (size == 1)
                {
                    humansOnShoulder.Pop().SetActive(false);
                    if (state == HumanState.RUNNING_CARRYING)
                        ChangeState(HumanState.RUNNING);
                }
                ShrinkCollider();
                foreach (GameObject human in humansOnShoulder)
                    human.GetComponent<FixedJoint>().massScale = 1.5f*(size - 1) / (float)maxSize;
            }
            size--;
            return true;
        }
        return false;
    }

    private void ExpandCollider()
    {
        BoxCollider.size += Vector3.up * 0.6f;
        BoxCollider.center += Vector3.up * 0.3f;
    }

    private void ShrinkCollider()
    {
        BoxCollider.size -= Vector3.up * 0.6f;
        BoxCollider.center -= Vector3.up * 0.3f;
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

    public Vector3 TopHumanPosition()
    {
        if (humansOnShoulder.Count > 0)
            return humansOnShoulder.Peek().transform.position;
        return transform.position;
    }

    public bool CheckGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up, Vector3.down);
    }

    public enum HumanState { IDLE, RUNNING, RUNNING_CARRYING, IDLE_CARRYING, WALKING, WALKING_CARRYING }
}
