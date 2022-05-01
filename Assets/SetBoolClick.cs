using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetBoolClick : MonoBehaviour
{
    public Animator anim;
    public string[] tru, fals;

    public void Click()
    {
        foreach (string name in tru) anim.SetBool(name, true);
        foreach (string name in fals) anim.SetBool(name, false);
    }
}
