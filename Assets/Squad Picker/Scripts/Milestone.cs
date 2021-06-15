using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FateGames;
using System.Linq;

public class Milestone : MonoBehaviour
{
    [SerializeField] private int numberOfNeededHumans = 50;
    [SerializeField] private Text text = null;
    [SerializeField] private GameObject[] confetties = null;
    [SerializeField] private GameObject staticDestruct = null;
    [SerializeField] private GameObject destruct = null;
    private int desiredNumberOfHumans = 0;
    private int currentNumberOfHumans = 0;
    private int initialNumberOfParts = 0;
    private Stack<Rigidbody> parts = null;
    private static SquadPickerLevel levelManager = null;
    private Queue<Human> humans = null;

    private bool reached = false;

    private void Awake()
    {
        parts = new Stack<Rigidbody>(GetComponentsInChildren<Rigidbody>().OrderBy(rb => -Vector3.Distance(rb.transform.position, transform.position - transform.forward * 5 + Vector3.up * 5)));
        initialNumberOfParts = parts.Count;
        if (!levelManager)
            levelManager = (SquadPickerLevel)LevelManager.Instance;
        text.text = (int)currentNumberOfHumans + "/" + numberOfNeededHumans;
        destruct.SetActive(false);
        humans = new Queue<Human>();
    }

    private void Update()
    {
        int difference = (int)Mathf.MoveTowards(currentNumberOfHumans, desiredNumberOfHumans, Time.deltaTime * 160) - currentNumberOfHumans;
        Human human;
        while (difference > 0 && humans.Count > 0)
        {
            human = humans.Dequeue();
            while (currentNumberOfHumans < numberOfNeededHumans && levelManager.Troop.DecrementHuman(human))
            {
                ObjectPooler.Instance.SpawnFromPool("PartExplosion", human.transform.position + Vector3.up * 0.5f, Quaternion.identity);
                currentNumberOfHumans++;
                difference--;
            }
        }
        if (!reached && currentNumberOfHumans >= numberOfNeededHumans)
        {
            reached = true;
            GetComponent<Collider>().enabled = false;
            text.enabled = false;
            levelManager.Troop.OnMilestone = false;
            for (int i = 0; i < confetties.Length; i++)
                confetties[i].SetActive(true);
            levelManager.ReachMilestone();
        }
        Rigidbody part;
        Vector3 force;
        while (parts.Count > 0 && ((float)initialNumberOfParts / numberOfNeededHumans) * currentNumberOfHumans > initialNumberOfParts - parts.Count)
        {
            part = parts.Pop();
            part.isKinematic = false;
            force = (part.transform.position - levelManager.Troop.transform.position).normalized;
            force *= Random.Range(10, 20f);
            force.y = Random.Range(3, 5f);
            GameObject effect = ObjectPooler.Instance.SpawnFromPool("PartTrail", part.transform.position, Quaternion.identity);
            effect.transform.parent = part.transform;
            effect.GetComponent<ParticleSystem>().Play();

            part.AddForce(force, ForceMode.Impulse);
            part.AddTorque(Vector3.one * Random.Range(10f, 20f) * (Random.value < 0.5f ? -1 : 1), ForceMode.Impulse);
            StartCoroutine(DeactivatePart(3, part.gameObject, effect));
        }
        text.text = currentNumberOfHumans + "/" + numberOfNeededHumans;
    }

    private void OnTriggerEnter(Collider other)
    {
        Human human = other.GetComponent<Human>();
        if (human)
        {
            humans.Enqueue(human);
            if (!levelManager.Troop.OnMilestone)
            {
                levelManager.Troop.OnMilestone = true;
                staticDestruct.SetActive(false);
                destruct.SetActive(true);
            }
            desiredNumberOfHumans += human.Size + 1;

        }
    }

    private IEnumerator DeactivatePart(float delay, GameObject part, GameObject effect)
    {
        yield return new WaitForSeconds(delay);
        part.SetActive(false);
        effect.transform.parent = null;
        effect.GetComponent<ParticleSystem>().Stop();
        effect.SetActive(false);
    }
}
