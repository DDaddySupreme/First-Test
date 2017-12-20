using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Dialogue// : MonoBehaviour
{
    public List<DialogueMain> dialogueList; 

    [System.Serializable]
    public class DialogueMain
    {
        [Multiline]
        public string text;
        public Sprite portrait;
        public float textDelay;

        [Space]
        public int exp;
        public Item item;
        public int itemAmount;

        [Space]
        public List<DialogueOption> options;
    }

    [System.Serializable]
    public class DialogueOption
    {

        public string text;
        public int destination;

        [Space]
        public bool requirement;
        public int levelRequirement;

        [Space]
        public List<Item> requiredItems;
        public List<int> requiredItemsAmount;

        [Space]
        public UnityEvent action;

    }
}

[System.Serializable]
public class Notification // A simpler version of dialogue used for switches and shops and such
{
    public string text;

    public string[] options;
    public UnityAction[] actions;

    public Notification (string t, string[] o, UnityAction[] a)
    {
        text = t;
        options = o;
        actions = a;
    }
}
