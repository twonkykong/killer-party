using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Menu : MonoBehaviourPunCallbacks
{
    public GameObject menu;
    public Text rooms;
    public InputField nickname;

    private void Start()
    {
        PhotonNetwork.NickName = "Player " + Random.Range(0, 101);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.ConnectUsingSettings();
    }

    private void Update()
    {
        rooms.text = "rooms: " + PhotonNetwork.CountOfRooms;
    }

    public override void OnConnectedToMaster()
    {
        menu.SetActive(true);
    }

    public void Connect()
    {
        if (PhotonNetwork.CountOfRooms > 0)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.CreateRoom("Room1");
            Application.LoadLevel("SampleScene");
        }
    }
}
