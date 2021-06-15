using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FateGames;

public class SquadPickerLevel : LevelManager
{

    private Troop troop = null;
    private Camera mainCamera = null;
    private CameraFollow cameraFollow = null;
    [SerializeField] private LayerMask humanLayerMask = 128;
    [SerializeField] private GameObject[] milestoneUI = null;
    [SerializeField] private Text[] levelTexts = null;
    [SerializeField] private Color humanColor;
    [SerializeField] private Text gemText = null;
    [SerializeField] private GameObject[] confetties = null;
    public int NumberOfGems = 0;
    private int numberOfReachedMilestones = 0;


    public Troop Troop { get => troop; }
    public Camera MainCamera { get => mainCamera; }
    public LayerMask HumanLayerMask { get => humanLayerMask; }
    public CameraFollow CameraFollow { get => cameraFollow; }
    public Color HumanColor { get => humanColor; }

    private new void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
        troop = FindObjectOfType<Troop>();
        cameraFollow = FindObjectOfType<CameraFollow>();
        levelTexts[0].text = GameManager.Instance.CurrentLevel.ToString();
        levelTexts[1].text = (GameManager.Instance.CurrentLevel + 1).ToString();
        gemText.text = GameManager.GEM.ToString();
    }
    public override void StartLevel()
    {
        cameraFollow.Target = troop;
        gemText.text = GameManager.GEM.ToString();
    }
    public override void FinishLevel(bool success)
    {
        GameManager.Instance.State = GameManager.GameState.FINISHED;
        cameraFollow.Target = null;
        if (success)
        {
            GameManager.GEM += NumberOfGems;
            foreach (GameObject confetti in confetties)
            {
                confetti.SetActive(true);
            }
        }
        GameManager.Instance.FinishLevel(success);
    }

    private static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle)
    {
        Vector3 dir = point - pivot;
        dir = Quaternion.Euler(0, angle, 0) * dir;
        point = dir + pivot;
        return point;
    }

    public void ReachMilestone()
    {
        milestoneUI[numberOfReachedMilestones].SetActive(true);
        numberOfReachedMilestones++;
        if (numberOfReachedMilestones == 3)
        {
            GameManager.Instance.State = GameManager.GameState.FINISHED;
            StartCoroutine(troop.TurnToGem());

        }
    }

}
