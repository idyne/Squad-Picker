using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FateGames;
public class TroopSize : MonoBehaviour
{
    private SquadPickerLevel levelManager = null;
    private Troop troop = null;
    private Camera mainCamera = null;
    private float size = 0;
    [SerializeField] private Text text = null;

    private void Awake()
    {
        levelManager = (SquadPickerLevel)LevelManager.Instance;
        troop = levelManager.Troop;
        mainCamera = Camera.main;
        size = troop.Size;
    }
    private void LateUpdate()
    {
        if (GameManager.Instance.State == GameManager.GameState.FINISHED && troop.Size <= 0)
        {
            Destroy(gameObject);
            return;
        }
        size = Mathf.MoveTowards(size, troop.Size, Time.deltaTime * 160);
        if (troop.Focus)
            transform.position = mainCamera.WorldToScreenPoint(troop.Focus.position + troop.transform.forward * 3);
        text.text = ((int)size).ToString();
    }
}
