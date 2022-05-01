using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Interactive : MonoBehaviourPun
{
    public int coins;
    public Animation anim;
    public bool working, on;

    public void Interact(Player player)
    {
        player.money -= coins;
        this.photonView.RPC("InteractPun", RpcTarget.All);
    }

    [PunRPC]
    public void InteractPun()
    {
        if (anim.GetClipCount() == 2)
        {
            if (on) anim.Play("off");
            else anim.Play("on");
            on = !on;
        }
        else if (!anim.isPlaying) anim.Play("work");
    }
}
