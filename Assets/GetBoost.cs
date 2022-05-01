using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetBoost : MonoBehaviour
{
    //0 - speed (5s for 3 coins), 1 - invisibility(8s for 6 coins), 2 - mask (20s for 10 coins), 3 - pistol (1 shot 10 coins), 4 - door key (1 use 6 coins);
    public int type;
    public int[] cost = new int[5] { 3, 6, 9, 10, 6 };

    public void Interact(Player player)
    {
        player.money -= cost[type];
        if (type == 0)
        {
            player.StartCoroutine(player.SpeedEffect());
        }
        else if (type == 1)
        {
            player.StartCoroutine(player.HideEffect());
        }
        else if (type == 2)
        {
            player.StartCoroutine(player.MaskEffect());
        }
        else if (type == 3)
        {

        }
        else if (type == 4)
        {

        }
    }
}
