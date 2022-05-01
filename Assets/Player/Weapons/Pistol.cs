using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon.StructWrapping;

public class Pistol : MonoBehaviourPun
{
    public Animator anim;
    public GameObject player;
    public ParticleSystem dust;

    IEnumerator HitTimer()
    {
        anim.SetBool("shot", true);
        yield return new WaitForSeconds(0.1f);
        anim.SetBool("shot", false);
    }

    public void Click()
    {
        if (anim.GetBool("shot")) return;

        StartCoroutine(HitTimer());

        RaycastHit hit;
        if (Physics.BoxCast(transform.position, Vector3.one * 0.5f, transform.forward, out hit, transform.rotation, 10f))
        {
            if (hit.collider.tag == "Player")
            {
                if (hit.collider.GetComponent<Player>().role != 1)
                {
                    player.GetComponent<Player>().photonView.RPC("Death", RpcTarget.All, player.GetPhotonView().ViewID, -5);
                }

                hit.collider.GetComponent<Player>().photonView.RPC("Death", RpcTarget.All, hit.collider.gameObject.GetPhotonView().ViewID, -5);
                GameObject.Find("gameManager").GetComponent<GameManager>().photonView.RPC("KillNotification", RpcTarget.All, hit.collider.gameObject.GetPhotonView().ViewID, player.GetPhotonView().ViewID, 0);
            }
        }

        if (player.GetComponent<Player>().role == 0 && !player.GetComponent<Player>().hasPolicemanPistol)
        {
            player.GetComponent<Player>()._ShowObj(player.GetComponent<Player>().pistol);
            player.GetComponent<Animator>().SetBool("pistol", false);
            player.GetComponent<Player>().policemanButtons.SetActive(false);
        }
    }

    [PunRPC]
    public void DustAnim(int ViewID)
    {
        PhotonView.Find(ViewID).GetComponent<ParticleSystem>().Stop();
        PhotonView.Find(ViewID).GetComponent<ParticleSystem>().Play();
    }
}
