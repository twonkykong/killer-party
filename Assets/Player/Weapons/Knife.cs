using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Knife : MonoBehaviour
{
    public Animator anim;
    public Player player;

    IEnumerator HitTimer()
    {
        anim.SetBool("hit", true);
        yield return new WaitForSeconds(0.3f);
        anim.SetBool("hit", false);
    }

    public void Click()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("knife_hit")) return;
        StartCoroutine(HitTimer());

        RaycastHit hit;
        if (Physics.BoxCast(transform.position, Vector3.one, transform.forward, out hit, transform.rotation, 3f))
        {
            if (hit.collider.tag == "Player")
            {
                hit.collider.GetComponent<Player>().photonView.RPC("Death", RpcTarget.All, hit.collider.gameObject.GetPhotonView().ViewID, player.gameObject.GetPhotonView().ViewID);
                GameObject.Find("gameManager").GetComponent<GameManager>().photonView.RPC("KillNotification", RpcTarget.All, hit.collider.gameObject.GetPhotonView().ViewID, -5, 0);
            }
        }
    }
}
