using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
using System.Linq;
using PathCreation;

public class NewTroop : MonoBehaviour
{
    [SerializeField] private int initialNumberOfHumans = 1;
    [SerializeField] private float maxVerticalSpeed = 5;
    [SerializeField] private float speedAngle = 30;
    [SerializeField] private int maxNumberOfBaseHumans = 52;
    [SerializeField] private List<NewHuman> baseHumans = new List<NewHuman>();
    [SerializeField] private int size = 0;
    //private SquadPickerLevel levelManager = null;
    private float angle = 0;
    private float sinSpeedAngle = 1;
    private float verticalSpeed = 1;
    private float horizontalSpeed = 1;
    private ObjectPooler objectPooler;
    private Transform focus = null;
    private List<PositionInTroop> positions = null;
    private PathCreator pathCreator = null;
    public int Size { get => size; }
    public float Angle { get => angle; }
    public Vector3 Direction { get => Quaternion.Euler(0, angle, 0) * Vector3.forward; }
    public int MaxNumberOfHumans { get => maxNumberOfBaseHumans; }
    public float MaxVerticalSpeed { get => maxVerticalSpeed; }
    public Transform Focus { get => focus; }

    public class PositionInTroop
    {
        private Vector3 position;
        private NewHuman takenBy = null;
        private int order;
        public Vector3 Position { get => position; }
        public bool Taken { get => takenBy != null; }
        public int Order { get => order; }
        public NewHuman TakenBy { get => takenBy; }

        public PositionInTroop(Vector3 position, int order)
        {
            this.position = position;
            this.order = order;
        }

        public void BeTaken(NewHuman by)
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
        //levelManager = (SquadPickerLevel)LevelManager.Instance;
        objectPooler = ObjectPooler.Instance;
        positions = CalculatePositionsInCircle(2, 0.5f);
        pathCreator = FindObjectOfType<PathCreator>();
        sinSpeedAngle = Mathf.Sin(speedAngle * Mathf.Deg2Rad);
    }
    private void Start()
    {
        AddHumans(initialNumberOfHumans);
    }

    private void Update()
    {
        maxVerticalSpeed = Mathf.Clamp(size / 20f + 5, 5, 15);
        NewCameraFollow.Instance.Speed = maxVerticalSpeed;
        horizontalSpeed = maxVerticalSpeed * sinSpeedAngle;

    }

    private void FixedUpdate()
    {
        MoveForward();
        MoveHumans();
    }

    public void Turn(float angle)
    {
        this.angle += angle;
        transform.forward = Direction;
        NewCameraFollow.Instance.Turn(angle);
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
        NewHuman human;
        for (int i = 0; i < baseHumans.Count; i++)
        {
            human = baseHumans[i];
            human.Move(maxVerticalSpeed);
        }
    }

    public void AddHumans(int numberOfHumans)
    {
        List<PositionInTroop> emptyPositions = positions
            .Where(position => !position.Taken)
            .ToList()
            .OrderBy(o => o.Order)
            .ToList();
        int numberOfExtraHumans = Mathf.Clamp(numberOfHumans - emptyPositions.Count, 0, numberOfHumans);
        int i;
        for (i = 0; i < numberOfHumans - numberOfExtraHumans; i++)
            AddHuman(emptyPositions[i]);
        List<NewHuman> sortedHumans = baseHumans
            .OrderBy(o => o.Size)
            .ToList();
        i = 0;
        NewHuman human;
        NewHuman nextHuman;
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
        }
        SetFocus();
    }

    private NewHuman AddHuman(PositionInTroop positionInTroop)
    {
        NewHuman human = objectPooler.SpawnFromPool("NewHuman", transform.position + positionInTroop.Position, Quaternion.identity).GetComponent<NewHuman>();
        positionInTroop.BeTaken(human);
        baseHumans.Add(human);
        size++;
        return human;
    }

    public void RemoveHumans(int numberOfHumans)
    {
        if (size >= numberOfHumans)
        {
            // Humans sorted by number of humans on their shoulders in descending order
            List<NewHuman> sortedHumans = baseHumans
                .OrderBy(o => -o.Size)
                .ThenBy(o => -o.PositionInTroop.Order)
                .ToList();
            int i = 0;
            while (i < numberOfHumans)
            {
                DecrementHuman(sortedHumans[i % sortedHumans.Count]);
                i++;
            }
        }
    }

    public bool DecrementHuman(NewHuman human)
    {
        if (baseHumans.Contains(human))
        {
            if (human.Size > 0)
                human.RemoveHumanOnShoulder();
            else
                RemoveHuman(human);
            size--;
            if (size == 0) { }
            //levelManager.FinishLevel(false);
            else
                SetFocus();
            return true;
        }
        return false;
    }

    private void RemoveHuman(NewHuman human)
    {
        human.gameObject.SetActive(false);
        human.PositionInTroop.Clear();
        baseHumans.Remove(human);
    }

    private void SetFocus()
    {
        focus = baseHumans.OrderBy(o => o.PositionInTroop.Order).ToArray()[0].transform;
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
                position = new Vector3((i + 0.5f) * humanWidth, 0, (j + 0.5f) * humanWidth);
                if (Vector3.Distance(position, Vector3.zero) < radius)
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
