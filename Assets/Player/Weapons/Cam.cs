using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Cam : MonoBehaviourPun
{
    public Player player;
    public GameObject steps;
    bool canUse = true;
    public Text text;

    IEnumerator useTimer()
    {
        canUse = false;
        for (int i = 5; i > 0; i--)
        {
            text.text = "photo\n" + i;
            yield return new WaitForSeconds(1);
        }
        text.text = "photo";
        canUse = true;
    }
    public void Click()
    {
        if (!canUse) return;
        RaycastHit hit;
        if (Physics.BoxCast(transform.position, Vector3.one, transform.forward, out hit, transform.rotation, 2f))
        {
            if (hit.collider.tag == "deadBody")
            {
                DeadBody db = hit.collider.gameObject.GetComponent<DeadBody>();
                if (db.killer == null) return;

                GameObject g = Instantiate(steps, db.firstPoint, Quaternion.identity);
                g.transform.LookAt(db.secondPoint);
                StartCoroutine(useTimer());
            }
        }
    }
}
