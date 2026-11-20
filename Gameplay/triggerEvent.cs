using UnityEngine;

public class triggerEvent : MonoBehaviour
{
    public GameObject object1;
    public GameObject object2;
    public GameObject object3;
    private bool discovered;

    private void Awake()
    {
        if (EscapeJournal.Current == null) gameObject.AddComponent<EscapeJournal>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (discovered || !other.CompareTag("Player")) return;
        discovered = true;
        object2.SetActive(true);
        object3.SetActive(false);
        // The journal carries the blocked-door information through the rest of the run.
        EscapeJournal.Current.Advance(EscapeJournal.Stage.Crank);
    }
}
