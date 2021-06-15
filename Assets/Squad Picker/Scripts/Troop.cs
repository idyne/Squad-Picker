using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
using System.Linq;
using PathCreation;

public class Troop : MonoBehaviour
{
    [SerializeField] private int initialNumberOfHumans = 1;
    [SerializeField] private float maxVerticalSpeed = 5;
    [SerializeField] private float speedAngle = 30;
    [SerializeField] private int maxNumberOfBaseHumans = 52;
    [SerializeField] private List<Human> baseHumans = new List<Human>();
    [SerializeField] private int size = 0;
    private int desiredSize = 0;
    private SquadPickerLevel levelManager = null;
    private float angle = 0;
    private float sinSpeedAngle = 1;
    private float verticalSpeed = 1;
    private float horizontalSpeed = 1;
    private ObjectPooler objectPooler;
    private Transform focus = null;
    private List<PositionInTroop> positions = null;
    private PathCreator pathCreator = null;
    public bool OnMilestone = false;
    public int Size { get => size; }
    public float Angle { get => angle; }
    public Vector3 Direction { get => Quaternion.Euler(0, angle, 0) * Vector3.forward; }
    public int MaxNumberOfHumans { get => maxNumberOfBaseHumans; }
    public float MaxVerticalSpeed { get => maxVerticalSpeed; }
    public Transform Focus { get => focus; }

    public class PositionInTroop
    {
        private Vector3 position;
        private Human takenBy = null;
        private int order;
        public Vector3 Position { get => position; }
        public bool Taken { get => takenBy != null; }
        public int Order { get => order; }
        public Human TakenBy { get => takenBy; }

        public PositionInTroop(Vector3 position, int order)
        {
            this.position = position;
            this.order = order;
        }

        public void BeTaken(Human by)
        {
            takenBy = by;
            by.PositionInTroop = this;
        }

        public void Clear()
        {
            takenBy.PositionInTroop = null;
            takenBy = null;
        }
    }

    private void Awake()
    {
        levelManager = (SquadPickerLevel)LevelManager.Instance;
        objectPooler = ObjectPooler.Instance;
        positions = CalculatePositionsInCircle(2.5f, 0.5f);
        pathCreator = FindObjectOfType<PathCreator>();
        sinSpeedAngle = Mathf.Sin(speedAngle * Mathf.Deg2Rad);
    }
    private void Start()
    {
        AddHumans(initialNumberOfHumans);
    }

    private void Update()
    {
        maxVerticalSpeed = OnMilestone ? 5 : Mathf.Clamp(size / 104f + 8, 8, 12);
        CameraFollow.Instance.DesiredSpeed = maxVerticalSpeed;
        horizontalSpeed = maxVerticalSpeed * sinSpeedAngle;
        //print(baseHumans.Where(human => human.PositionInTroop == null).Count());

        int difference = (int)Mathf.MoveTowards(size, desiredSize, Time.deltaTime * 160) - size;

        if (difference > 0)
            _AddHumans(difference);
        else if (difference < 0)
            _RemoveHumans(-difference);
    }

    private void LateUpdate()
    {
        int i = 0;
        while (i < baseHumans.Count)
        {
            if (!baseHumans[i].CheckGrounded())
                ReleaseHuman(baseHumans[i]);
            else
                i++;
        }
        if (size <= 0 && GameManager.Instance.State == GameManager.GameState.STARTED)
            levelManager.FinishLevel(false);
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.State == GameManager.GameState.STARTED)
        {
            MoveForward();
            MoveHumans();
        }
    }

    public void Turn(float angle)
    {
        this.angle += angle;
        transform.forward = Direction;
        CameraFollow.Instance.Turn(angle);
    }



    private void MoveForward()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, Time.fixedDeltaTime * verticalSpeed);
    }

    public void MoveHorizontally(float rate)
    {
        verticalSpeed = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(maxVerticalSpeed, 2) - Mathf.Pow(rate * horizontalSpeed, 2)));
        Vector3 desiredPos = transform.position + transform.right * rate * Time.fixedDeltaTime * horizontalSpeed;
        transform.position = desiredPos;
    }



    public void MoveHumans()
    {
        Human human;
        for (int i = 0; i < baseHumans.Count; i++)
        {
            human = baseHumans[i];
            human.Move(maxVerticalSpeed);
        }
    }

    private void _AddHumans(int numberOfHumans)
    {
        List<PositionInTroop> emptyPositions = positions
           .Where(position => !position.Taken)
           .ToList()
           .OrderBy(o => o.Order)
           .ToList();
        int numberOfExtraHumans = Mathf.Clamp(numberOfHumans - emptyPositions.Count, 0, numberOfHumans);
        int i;
        for (i = 0; i < numberOfHumans - numberOfExtraHumans; i++)
        {
            AddHuman(emptyPositions[i]);
            SetFocus();
        }
        List<Human> sortedHumans = baseHumans
            .OrderBy(o => o.Size)
            .ToList();
        i = 0;
        Human human;
        Human nextHuman;
        while (numberOfExtraHumans > 0)
        {
            human = sortedHumans[i % sortedHumans.Count];
            nextHuman = sortedHumans[(i + 1) % sortedHumans.Count];
            human.AddHumanOnShoulder();
            if (human.Size > nextHuman.Size)
                i++;
            else if (human.Size == nextHuman.Size)
                i = 0;
            numberOfExtraHumans--;
            size++;
            SetFocus();
        }
    }

    public void AddHumans(int numberOfHumans)
    {

        desiredSize += numberOfHumans;
    }

    private Human AddHuman(PositionInTroop positionInTroop)
    {
        Human human = objectPooler.SpawnFromPool("Human", transform.position + positionInTroop.Position, Quaternion.identity).GetComponent<Human>();
        positionInTroop.BeTaken(human);
        baseHumans.Add(human);
        ObjectPooler.Instance.SpawnFromPool("AddEffect", human.transform.position + Vector3.up * 0.5f, Quaternion.identity).transform.parent = human.transform;
        Renderer renderer = human.GetComponentInChildren<Renderer>();
        renderer.material.color = Color.black;
        renderer.gameObject.LeanColor(levelManager.HumanColor, 0.5f);
        size++;
        return human;
    }

    public void _RemoveHumans(int numberOfHumans)
    {
        numberOfHumans = Mathf.Clamp(numberOfHumans, 0, size);
        if (size >= numberOfHumans)
        {
            // Humans sorted by number of humans on their shoulders in descending order
            List<Human> sortedHumans = baseHumans
                .OrderBy(o => -o.Size)
                .ThenBy(o => -o.PositionInTroop.Order)
                .ToList();
            int i = 0;
            while (i < numberOfHumans)
            {
                DecrementHuman(sortedHumans[i % sortedHumans.Count], false);
                i++;
            }
        }
    }
    public void RemoveHumans(int numberOfHumans)
    {
        desiredSize -= numberOfHumans;
    }

    public bool DecrementHuman(Human human, bool b = true)
    {
        if (baseHumans.Contains(human))
        {
            ObjectPooler.Instance.SpawnFromPool("DieEffect", human.TopHumanPosition() + Vector3.up * 0.5f, Quaternion.identity);
            if (human.Size > 0)
                human.RemoveHumanOnShoulder();
            else
                RemoveHuman(human);
            size--;
            if (b)
                desiredSize--;
            if (size != 0)
                SetFocus();
            return true;
        }
        return false;
    }

    private void RemoveHuman(Human human)
    {
        if (baseHumans.Contains(human))
        {
            human.History += "RemoveHuman\n";
            human.gameObject.SetActive(false);
            baseHumans.Remove(human);
            human.IsActive = false;
            human.PositionInTroop.Clear();
        }
    }

    private void ReleaseHuman(Human human)
    {
        if (baseHumans.Contains(human))
        {
            human.History += "ReleaseHuman\n";
            baseHumans.Remove(human);
            human.PositionInTroop.Clear();
            human.IsActive = false;
            size -= human.Size + 1;
            desiredSize -= human.Size + 1;
            human.Rb.isKinematic = false;
            human.BoxCollider.enabled = false;
            SetFocus();
        }
    }

    private void SetFocus()
    {
        if (size > 0)
            focus = baseHumans.OrderBy(o => o.PositionInTroop.Order).ToArray()[0].transform;
    }

    public IEnumerator TurnToGem()
    {
        yield return new WaitForSeconds(0.5f);
        Human[] humans = baseHumans.OrderBy(human => -human.Size).ThenBy(human => -human.PositionInTroop.Order).ToArray();

        int humanSize = humans[0].Size;
        Vector3 effectPos;
        for (int i = 0; i < humans.Length; i++)
        {
            if (humans[i].Size != humanSize || size == 1)
                break;
            if (humans[i].Size == 0)
            {
                effectPos = humans[i].transform.position + Vector3.up * 0.5f;
                RemoveHuman(humans[i]);
            }
            else
            {
                effectPos = humans[i].TopHumanPosition() + Vector3.up * 0.5f;
                humans[i].RemoveHumanOnShoulder();
            }
            objectPooler.SpawnFromPool("GemEffect", effectPos, Quaternion.identity);
            size--;
            desiredSize--;
            levelManager.NumberOfGems++;
        }
        for (int i = 0; i < humans.Length; i++)
            humans[i].ChangeState(humans[i].Size > 0 ? Human.HumanState.IDLE_CARRYING : Human.HumanState.IDLE);
        if (size > 1)
            StartCoroutine(TurnToGem());
        else
            levelManager.FinishLevel(true);
    }


    private List<PositionInTroop> CalculatePositionsInCircle(float radius, float humanWidth)
    {
        List<Vector3> positions = new List<Vector3>();
        List<PositionInTroop> positionsInTroop = new List<PositionInTroop>();
        int maxCount = (int)(radius / humanWidth);
        Vector3 position;
        for (int i = -maxCount; i < maxCount; i++)
        {
            for (int j = -maxCount; j < maxCount; j++)
            {
                position = new Vector3(i + 0.5f, 0, j + 0.5f) * humanWidth;
                if (Vector3.Distance(position, Vector3.zero) + humanWidth * Mathf.Sqrt(2) < radius)
                    positions.Add(position);
            }
        }
        List<Vector3> sortedPositions = positions.OrderBy(o => Vector3.Distance(Vector3.zero, o)).ToList();
        for (int i = 0; i < sortedPositions.Count; i++)
        {
            positionsInTroop.Add(new PositionInTroop(sortedPositions[i], i));
        }
        return positionsInTroop;
    }

}
