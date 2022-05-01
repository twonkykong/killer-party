using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DeathTrigger : MonoBehaviourPun
{
    public bool tpToSpawn;
    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (other.tag == "Player")
        {
            if (!GameObject.Find("gameManager").GetComponent<GameManager>().endedGame)
            {
                other.GetComponent<Player>().photonView.RPC("Death", RpcTarget.All, other.gameObject.GetPhotonView().ViewID);
                GameObject.Find("gameManager").GetComponent<GameManager>().photonView.RPC("KillNotification", RpcTarget.All, other.gameObject.GetPhotonView().ViewID, -5, 1);
                this.photonView.RPC("ChangeTag", RpcTarget.All, other.gameObject.GetPhotonView().ViewID);
                
            }
            if (tpToSpawn) other.transform.position = GameObject.Find("gameManager").GetComponent<GameManager>().spawns[Random.Range(0, GameObject.Find("gameManager").GetComponent<GameManager>().spawns.Length)].position;
        }
        else if (other.gameObject.layer == 9)
        {
            if (tpToSpawn) other.transform.position = GameObject.Find("gameManager").GetComponent<GameManager>().spawns[Random.Range(0, GameObject.Find("gameManager").GetComponent<GameManager>().spawns.Length)].position;
        }
    }

    [PunRPC]
    public void ChangeTag(int ViewID)
    {
        PhotonView.Find(ViewID).gameObject.tag = "Untagged";
    }
}
