using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    public Animator anim;
    [SerializeField] private Text txt;

    public void OnEnable()
    {
        AnimatorClipInfo[] clipInfo = anim.GetCurrentAnimatorClipInfo(0);
        Destroy(gameObject, clipInfo[0].clip.length - Time.deltaTime);
        txt = anim.GetComponent<Text>();
    }

    public void Set(string text)
    {

        txt.text = text;
    }
}
