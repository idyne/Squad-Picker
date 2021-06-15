using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FateGames;

public class NewSquadPickerLevel : LevelManager
{

    private NewTroop troop = null;
    private Camera mainCamera = null;
    private CameraFollow cameraFollow = null;
    [SerializeField] private LayerMask humanLayerMask = 128;
    [SerializeField] private GameObject[] milestoneUI = null;
    [SerializeField] private Text[] levelTexts = null;
    [SerializeField] private Gradient humanPowerPalette = null;
    [SerializeField] private Text gemText = null;
    [SerializeField] private GameObject[] confetties = null;
    public int NumberOfGems = 0;
    private int numberOfReachedMilestones = 0;


    public NewTroop NewTroop { get => troop; }
    public Camera MainCamera { get => mainCamera; }
    public LayerMask HumanLayerMask { get => humanLayerMask; }
    public CameraFollow CameraFollow { get => cameraFollow; }
    public Gradient HumanPowerPalette { get => humanPowerPalette; }

    private new void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
        troop = FindObjectOfType<NewTroop>();
        cameraFollow = FindObjectOfType<CameraFollow>();
        levelTexts[0].text = GameManager.Instance.CurrentLevel.ToString();
        levelTexts[1].text = (GameManager.Instance.CurrentLevel + 1).ToString();
        gemText.text = GameManager.GEM.ToString();
    }
    public override void StartLevel()
    {
        //cameraFollow.Target = troop;
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
                confetti.GetComponent<ParticleSystem>().Play();
            }
        }
        GameManager.Instance.FinishLevel(success);
    }



}
