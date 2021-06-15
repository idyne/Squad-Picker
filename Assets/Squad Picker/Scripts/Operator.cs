using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FateGames;

public abstract class Operator : MonoBehaviour
{
    protected static SquadPickerLevel levelManager = null;
    [SerializeField] protected Text text = null;
    [SerializeField] private GameObject effect = null;
    [SerializeField] private GameObject panel = null;
    private Collider coll = null;

    private void Awake()
    {
        if (!levelManager)
            levelManager = (SquadPickerLevel)LevelManager.Instance;
        coll = GetComponent<Collider>();
        SetText();
    }



    protected abstract void Operate();
    private void Deactivate()
    {
        coll.enabled = false;
        panel.SetActive(false);
        effect.SetActive(true);
        text.enabled = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        Human human = other.GetComponent<Human>();
        if (human && human.IsActive)
        {
            Deactivate();
            Operate();
        }
    }

    protected abstract void SetText();
}
