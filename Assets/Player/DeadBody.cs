using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBody : MonoBehaviour
{
    public GameObject[] masks, bags;
    public GameObject killer, steps;
    public Vector3 firstPoint, secondPoint;
    public SkinnedMeshRenderer tie1, tie2, gloves, skin;
    public Player player;

    public void Configure()
    {
        for (int i = 0; i < masks.Length; i++)
        {
            if (player.headmasks[i].activeInHierarchy)
            {
                masks[i].SetActive(true);
                return;
            }
        }

        for (int i = 0; i < bags.Length; i++)
        {
            if (player.skins[i].activeInHierarchy)
            {
                bags[i].SetActive(true);
                return;
            }
        }

        if (killer != null)
        {
            StartCoroutine(StepsConfigure());
        }

        gloves.material.color = player.gloves.GetComponent<SkinnedMeshRenderer>().material.color;
    }

    IEnumerator StepsConfigure()
    {
        yield return new WaitForSeconds(0.2f);
        firstPoint = killer.transform.position;
        yield return new WaitForSeconds(1);
        secondPoint = killer.transform.position;
    }
}
