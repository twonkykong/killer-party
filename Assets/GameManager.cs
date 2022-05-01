using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;
using UnityEngine.Assertions.Must;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab, moneyPrefab;
    public Player player;
    Text playersText;
    public Transform[] spawns, coinsSpawns;
    public List<GameObject> playersAlive;
    public bool endedGame;

    private void Start()
    {
        endedGame = true;
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0, 5, 0), Quaternion.identity);
        StartCoroutine(PlayersList());
    }


    IEnumerator PlayersList()
    {
        while (player == null)
        {
            yield return new WaitForEndOfFrame();
        }

        playersText = player.playersText;
        while (true)
        {
            playersText.text = "players (" + PhotonNetwork.CurrentRoom.PlayerCount + "/10):";
            foreach (var player in PhotonNetwork.CurrentRoom.Players)
            {
                playersText.text += "\n" + player.Value.NickName;
                if (player.Value.NickName == PhotonNetwork.NickName) playersText.text += " (you)";
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void StartMatch()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 3)
        {
            player.warningNotification.GetComponentInChildren<Text>().text = "Need at least 3 players in a lobby!";
            player.warningNotification.Stop();
            player.warningNotification.Play();
        }
        else
        {
            List<int> nums = new int[11] { 0, 1, 2, 3, 4, 5, 6, 7, 7, 9, 10 }.ToList();
            int num = nums[Random.Range(0, nums.Count)];

            StopCoroutine(PlayersList());
            //0 - pacific, 1 - murder, 2 - policeman
            List<System.Collections.Generic.KeyValuePair<int, Photon.Realtime.Player>> players = PhotonNetwork.CurrentRoom.Players.ToList();

            int murderChance = Mathf.RoundToInt(Random.Range(0, players.Count * 10)/10);
            this.photonView.RPC("SetRoles", RpcTarget.All, players[murderChance].Value.NickName, 1, num);
            nums.Remove(num);
            players.RemoveAt(murderChance);

            num = nums[Random.Range(0, nums.Count)];
            int policemanChance = Mathf.RoundToInt(Random.Range(0, players.Count * 10) /10);
            this.photonView.RPC("SetRoles", RpcTarget.All, players[policemanChance].Value.NickName, 2, num);
            nums.Remove(num);
            players.RemoveAt(policemanChance);

            num = nums[Random.Range(0, nums.Count)];
            int detectiveChance = Mathf.RoundToInt(Random.Range(0, players.Count * 10) / 10);
            this.photonView.RPC("SetRoles", RpcTarget.All, players[detectiveChance].Value.NickName, 3, num);
            nums.Remove(num);
            players.RemoveAt(policemanChance);


            if (players.Count > 0)
            {
                foreach (var player in players)
                {
                    num = nums[Random.Range(0, nums.Count)];
                    this.photonView.RPC("SetRoles", RpcTarget.All, player.Value.NickName, 0, num);
                    nums.Remove(num);
                }
            }

            List<Transform> points = spawns.ToList();
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
            {
                int rand = Random.Range(0, points.Count);
                this.photonView.RPC("TpToSpawn", RpcTarget.AllViaServer, obj.GetPhotonView().ViewID, points[rand].position.x, points[rand].position.y, points[rand].position.z);
                points.Remove(points[rand]);
            }

            playersAlive = GameObject.FindGameObjectsWithTag("Player").ToList();
        }
    }

    IEnumerator gameManagerThings()
    {
        while (true)
        {
            for (int i = 5; i > 0; i--)
            {
                foreach (Transform spawn in coinsSpawns)
                {
                    if (spawn.childCount == 1) spawn.GetComponentInChildren<TextMeshPro>().text = i + "";
                }
                yield return new WaitForSeconds(1);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                List<Transform> spawns = coinsSpawns.ToList();

                if (GameObject.FindGameObjectsWithTag("money").Length != 0)
                {
                    foreach (GameObject coin in GameObject.FindGameObjectsWithTag("money"))
                    {
                        spawns.Remove(coin.transform.parent);
                    }
                }

                foreach (Transform spawn in spawns)
                {
                    spawn.GetComponentInChildren<TextMeshPro>().text = "";
                    GameObject g = PhotonNetwork.Instantiate(moneyPrefab.name, spawn.position + Vector3.up * 2, Quaternion.identity);
                    g.transform.parent = spawn;
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    [PunRPC]
    public void ChangeColor(int ViewID)
    {
        GameObject obj = PhotonView.Find(ViewID).gameObject;
        Material mat = new Material(obj.GetComponent<SkinnedMeshRenderer>().material);
        Color[] colors = new Color[10] { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta, Color.white, Color.grey, new Color(0, 0.5f, 0), new Color(1, 0.647f, 0) };
        mat.color = colors[Random.Range(0, 11)];
        obj.GetComponent<SkinnedMeshRenderer>().material = mat;
    }

    [PunRPC]
    public void SetRoles(string nick, int _role, int mask)
    {
        if (PhotonNetwork.LocalPlayer.NickName == nick)
        {
            endedGame = false;
            StartCoroutine(gameManagerThings());
            player.StartCoroutine(player.UpdateGame());
            player.lobbyButtons.SetActive(false);
            player.gameButtons.SetActive(true);

            player.role = _role;
            player.photonView.RPC("ShowObj", RpcTarget.All, player.headmasks[mask].GetPhotonView().ViewID);
            player.currentMask = player.headmasks[mask];

            if (_role == 1)
            {
                player.murderButtons.SetActive(true);
                player.roleNotification.GetComponentInChildren<Text>().text = "You are Murder!";
            }
            else if (_role == 2)
            {
                player.policemanButtons.SetActive(true);
                player.roleNotification.GetComponentInChildren<Text>().text = "You are Policeman!";
            }
            else if (_role == 3)
            {
                player.detectiveButtons.SetActive(true);
                player.roleNotification.GetComponentInChildren<Text>().text = "You are Detective!";
            }
            else
            {
                player.roleNotification.GetComponentInChildren<Text>().text = "You are Innocent!";
            }
            player.roleNotification.Stop();
            player.roleNotification.Play();
        }

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (obj.GetPhotonView().Owner.NickName == nick) obj.GetComponent<Player>().role = _role;
        }
    }

    [PunRPC]
    public void TpToSpawn(int ViewID, float x, float y, float z)
    {
        PhotonView.Find(ViewID).gameObject.transform.position = new Vector3(x, y, z);
    }

    [PunRPC]
    public void KillNotification(int ViewID1, int ViewID2, int trap)
    {
        if (trap == 1)
        {
            string text = PhotonView.Find(ViewID1).Owner.NickName;
            if (PhotonView.Find(ViewID1).gameObject.GetComponent<Player>().role == 1)
            {
                text = "Murder (" + PhotonView.Find(ViewID1).Owner.NickName + ")";
                this.photonView.RPC("EndGame", RpcTarget.All, "Innocents win!");
            }
            else if (PhotonView.Find(ViewID1).gameObject.GetComponent<Player>().role == 2) text = "Policeman (" + PhotonView.Find(ViewID1).Owner.NickName + ")";

            player.killNotification.GetComponentInChildren<Text>().text = text + " got in a trap!";
            playersAlive.Remove(PhotonView.Find(ViewID1).gameObject);

            if (playersAlive.Count == 1 && PhotonView.Find(ViewID1).gameObject.GetComponent<Player>().role != 1)
            {
                this.photonView.RPC("EndGame", RpcTarget.All, "Murder wins!");
            }
        }
        else
        {
            if (ViewID2 == -5)
            {
                player.killNotification.GetComponentInChildren<Text>().text = PhotonView.Find(ViewID1).Owner.NickName + " was killed by Murder!";
                playersAlive.Remove(PhotonView.Find(ViewID1).gameObject);
                if (playersAlive.Count == 1)
                {
                    this.photonView.RPC("EndGame", RpcTarget.All, "Murder wins!");
                }
            }
            else
            {
                string text = PhotonView.Find(ViewID1).Owner.NickName;
                if (PhotonView.Find(ViewID1).gameObject.GetComponent<Player>().role == 1)
                {
                    text = "Murder (" + PhotonView.Find(ViewID1).Owner.NickName + ")";
                    this.photonView.RPC("EndGame", RpcTarget.All, "Innocents win!");
                }

                player.killNotification.GetComponentInChildren<Text>().text = text + " was killed by Policeman (" + PhotonView.Find(ViewID2).Owner.NickName + ")!"; ;
            }
        }

        player.killNotification.Stop();
        player.killNotification.Play();
    }

    [PunRPC]
    public void EndGame(string whoWins)
    {
        player.murderButtons.SetActive(false);
        player.policemanButtons.SetActive(false);

        player.roleNotification.GetComponentInChildren<Text>().text = whoWins;
        player.roleNotification.Stop();
        player.roleNotification.Play();

        if (player.dead)
        {
            foreach (SkinnedMeshRenderer r in player.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Material material = r.material;

                material.SetOverrideTag("RenderType", "");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                material.color = new Color(material.color.r, material.color.g, material.color.b, 1);

                r.material = material;
            }

            player.photonView.RPC("HideEffectAdd", RpcTarget.Others, player.gameObject.GetPhotonView().ViewID);
        }

        StopAllCoroutines();
    }

    [PunRPC]
    public void OtherNotification(string _notification)
    {
        player.notification.GetComponentInChildren<Text>().text = _notification;
        player.notification.Stop();
        player.notification.Play();
    }

    [PunRPC]
    public new void SendMessage(string msg)
    {
        player.chatText.text += "\n" + msg;
    }

    public void Disconnect()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Application.LoadLevel("menu");
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        player.joinedNotification.GetComponentInChildren<Text>().text = newPlayer.NickName + " has joined lobby";
        player.joinedNotification.Stop();
        player.joinedNotification.Play();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        player.joinedNotification.GetComponentInChildren<Text>().text = otherPlayer.NickName + " has left lobby";
        player.joinedNotification.Stop();
        player.joinedNotification.Play();
    }
}
