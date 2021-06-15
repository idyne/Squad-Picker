using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;

public class NewTroopController : MonoBehaviour
{
    private Swerve1D swerve = null;
    private NewTroop troop = null;
    private Vector3 anchor;
    private bool isTapCancelled = false;
    private float tapTime = 0;
    private int screenWidth = 0;
    [SerializeField] private bool turnRight = true;
    private void Awake()
    {
        swerve = InputManager.CreateSwerve1D(Vector2.right, Screen.width / 2f);
        troop = GetComponent<NewTroop>();
        screenWidth = Screen.width;
    }

    private void FixedUpdate()
    {
        troop.MoveHorizontally(swerve.Rate);
    }

    private void Update()
    {
        if (GameManager.Instance.State == GameManager.GameState.STARTED)
        {
            CheckInput();
        }
    }
    private void CheckInput()
    {

        if (Input.GetMouseButtonDown(0))
        {
            anchor = Input.mousePosition;
            isTapCancelled = false;
            tapTime = Time.time;
        }
        else if (Input.GetMouseButton(0))
        {
            if (Vector2.Distance(Input.mousePosition, anchor) >= screenWidth / 20)
            {
                isTapCancelled = true;
            }
        }
        else if (Input.GetMouseButtonUp(0) && !isTapCancelled && Time.time - tapTime < 0.5f)
        {

            troop.Turn(turnRight ? 90 : -90);
            turnRight = !turnRight;
        }
        
    }
}
