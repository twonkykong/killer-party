using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : MonoBehaviourPun
{
    public GameObject[] show, skins, headmasks;
    public GameObject policemanButtons, murderButtons, detectiveButtons, pickupPistol, pickupKey, interactButton, pistolItemPrefab, getBoostButton, startButton, gameButtons, lobbyButtons, pistol, backMask, currentMask, hideCylinder, keyPrefab, keyButton, camButton, killer;
    public Animation killNotification, notification, roleNotification, warningNotification, joinedNotification;
    public int role, money;
    public bool hasPolicemanPistol;
    public Text playersText, moneyText, maskEffect, hideEffect, speedEffect;

    GameObject pistolItem, keyItem, interactionObj, boostObj, keyObj;
    GameManager gameManager;
    public bool dead, key;

    public InputField textInput;
    public Text chatText;

    public GameObject tie1, tie2, gloves, skin, jacket, deadBody;
    Color[] glovesPalette = new Color[10] { Color.red, new Color(1, 165/255, 0), Color.yellow, Color.green, new Color(0, 0.5f, 0), Color.cyan, Color.blue, Color.magenta, Color.black, Color.white};
    
    private void Start()
    {
        if (!this.photonView.IsMine)
        {
            foreach (GameObject obj in show) obj.SetActive(false);
            GetComponent<PlayerWalk>().enabled = false;
        }
        else
        {
            gameManager = GameObject.Find("gameManager").GetComponent<GameManager>();
            gameManager.player = GetComponent<Player>();
            int rand = Random.Range(0, skins.Length + 1);
            if (rand != skins.Length) _ShowObj(skins[rand]);

            Color color = glovesPalette[Random.Range(0, glovesPalette.Length)];
            //this.photonView.RPC("ChangeColor", RpcTarget.AllBufferedViaServer, tie1.GetPhotonView().ViewID, color.r, color.g, color.b);
            //this.photonView.RPC("ChangeColor", RpcTarget.AllBufferedViaServer, tie2.GetPhotonView().ViewID, color.r, color.g, color.b);
            this.photonView.RPC("ChangeColor", RpcTarget.AllBufferedViaServer, gloves.GetPhotonView().ViewID, color.r, color.g, color.b);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
    }

    [PunRPC]
    public void ChangeColor(int ViewID, float r, float g, float b)
    {
        Material mat = new Material(PhotonView.Find(ViewID).gameObject.GetComponent<SkinnedMeshRenderer>().material);
        mat.color = new Color(r, g, b);
        PhotonView.Find(ViewID).gameObject.GetComponent<SkinnedMeshRenderer>().material = mat;
    }

    public IEnumerator UpdateGame()
    {
        moneyText.gameObject.SetActive(true);
        while (true)
        {
            moneyText.text = "<b>money: " + money + "</b>";
            yield return new WaitForEndOfFrame();
        }
    }

    public void _ShowObj(GameObject obj)
    {
        this.photonView.RPC("ShowObj", RpcTarget.All, obj.GetPhotonView().ViewID);
    }

    [PunRPC]
    public void ShowObj(int ViewID)
    {
        PhotonView.Find(ViewID).gameObject.SetActive(!PhotonView.Find(ViewID).gameObject.activeInHierarchy);
    }

    [PunRPC]
    public void Death(int ViewID, int KillerViewID)
    {
        if (PhotonView.Find(ViewID).IsMine)
        {
            GameObject g = PhotonNetwork.Instantiate(deadBody.name, transform.position, transform.rotation);
            if (KillerViewID != -5)
            {
                this.photonView.RPC("ConfigureDeadBody", RpcTarget.All, g.GetPhotonView().ViewID, this.photonView.ViewID, KillerViewID);
            }
            else this.photonView.RPC("ConfigureDeadBody", RpcTarget.All, g.GetPhotonView().ViewID, this.photonView.ViewID, -5);
            
            if (role == 1)
            {
                murderButtons.SetActive(false);

                if (hideCylinder.activeSelf)
                {
                    this.photonView.RPC("HideEffectAdd", RpcTarget.All, this.photonView.ViewID);
                    hideCylinder.SetActive(false);
                }
            }
            else
            {
                if (role == 2)
                {
                    PhotonNetwork.Instantiate(pistolItemPrefab.name, transform.position + Vector3.up * 2, Quaternion.identity);
                    gameManager.photonView.RPC("OtherNotification", RpcTarget.All, "Pistol dropped!");
                    policemanButtons.SetActive(false);
                }
                else if (role == 3)
                {
                    PhotonNetwork.Instantiate(keyPrefab.name, transform.position + Vector3.up * 2, Quaternion.identity);
                    gameManager.photonView.RPC("OtherNotification", RpcTarget.All, "Key dropped!");
                    detectiveButtons.SetActive(false);
                    keyButton.SetActive(false);
                }

                if (hideCylinder.activeSelf)
                {
                    this.photonView.RPC("HideEffectAdd", RpcTarget.All, this.photonView.ViewID);
                    hideCylinder.SetActive(false);
                }

                foreach (SkinnedMeshRenderer r in GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    Material material = new Material(r.material);
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    material.color = new Color(material.color.r, material.color.g, material.color.b, 0.5f);
                    r.material = material;
                }

                this.photonView.RPC("HideEffectAdd", RpcTarget.Others, this.photonView.ViewID);
                dead = true;
            }
            pickupPistol.SetActive(false);
            interactButton.SetActive(false);
            getBoostButton.SetActive(false);
            moneyText.gameObject.SetActive(false);
        }
    }

    [PunRPC] 
    public void ConfigureDeadBody(int ViewID1, int ViewID2, int KillerViewID)
    {
        PhotonView.Find(ViewID1).gameObject.GetComponent<DeadBody>().player = PhotonView.Find(ViewID2).gameObject.GetComponent<Player>();
        if (KillerViewID != -5) PhotonView.Find(ViewID1).gameObject.GetComponent<DeadBody>().killer = PhotonView.Find(KillerViewID).gameObject;
        PhotonView.Find(ViewID1).gameObject.GetComponent<DeadBody>().Configure();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (dead) return;
        if (other.CompareTag("pistol"))
        {
            if (role == 0 || role == 3)
            {
                pickupPistol.SetActive(true);
                pistolItem = other.gameObject;
            }
        }

        else if (other.CompareTag("interactive"))
        {
            interactButton.SetActive(true);
            interactionObj = other.gameObject;
        }

        else if (other.CompareTag("boost"))
        {
            getBoostButton.SetActive(true);
            boostObj = other.gameObject;
            string boost = "speed";
            if (boostObj.GetComponent<GetBoost>().type == 1) boost = "invisibility";
            else if (boostObj.GetComponent<GetBoost>().type == 2) boost = "mask";
            getBoostButton.GetComponentInChildren<Text>().text = boostObj.GetComponent<GetBoost>().cost[boostObj.GetComponent<GetBoost>().type] + " money -> " + boost;
        }

        else if (other.CompareTag("money"))
        {
            if (money == 10) return;
            other.gameObject.GetPhotonView().TransferOwnership(PhotonNetwork.LocalPlayer);
            PhotonNetwork.Destroy(other.gameObject);
            money += 1;
        }

        else if (other.CompareTag("key"))
        {
            if (role != 1)
            {
                pickupKey.SetActive(true);
                keyItem = other.gameObject;
            }
        }

        else if (other.CompareTag("keyUse"))
        {
            keyObj = other.gameObject;
            keyButton.SetActive(true);

            string cost = "";
            if (!key) cost = " (" + keyObj.GetComponent<Interactive>().coins + ")";
            keyButton.GetComponentInChildren<Text>().text = "open" + cost;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (dead) return;
        if (other.CompareTag("pistol"))
        {
            if (role == 0 || role == 3)
            {
                pickupPistol.SetActive(false);
                pistolItem = null;
            }
        }

        else if (other.CompareTag("interactive"))
        {
            interactButton.SetActive(false);
            interactionObj = null;
        }

        else if (other.CompareTag("boost"))
        {
            getBoostButton.SetActive(false);
            boostObj = null;
        }

        else if (other.CompareTag("key"))
        {
            if (role != 1)
            {
                pickupKey.SetActive(false);
                keyItem = null;
            }
        }

        else if (other.CompareTag("keyUse"))
        {
            keyObj = null;
            keyButton.SetActive(false);
        }
    }

    public void PickUpPistol()
    {
        pistolItem.GetPhotonView().TransferOwnership(PhotonNetwork.LocalPlayer);
        PhotonNetwork.Destroy(pistolItem);
        gameManager.photonView.RPC("OtherNotification", RpcTarget.All, "Pistol picked up!");
        policemanButtons.SetActive(true);
        hasPolicemanPistol = true;
    }

    public void PickUpKey()
    {
        keyItem.GetPhotonView().TransferOwnership(PhotonNetwork.LocalPlayer);
        PhotonNetwork.Destroy(keyItem);
        key = true;
        notification.GetComponentInChildren<Text>().text = "You got a key!";
        notification.Stop();
        notification.Play();
    }

    public void Interact(bool key)
    {
        GameObject obj = interactionObj;
        if (key)
        {
            obj = keyObj;

            if (this.key)
            {
                money += obj.GetComponent<Interactive>().coins;
                obj.GetComponent<Interactive>().Interact(GetComponent<Player>());
                return;
            }
        }
        
        if (obj.GetComponent<Interactive>().coins <= money)
        {
            obj.GetComponent<Interactive>().Interact(GetComponent<Player>());
        }
        else
        {
            warningNotification.GetComponentInChildren<Text>().text = "Not enough money!";
            warningNotification.Stop();
            warningNotification.Play();
        }
    }

    public void GetBoost()
    {
        if (boostObj.GetComponent<GetBoost>().cost[boostObj.GetComponent<GetBoost>().type] <= money)
        {
            boostObj.GetComponent<GetBoost>().Interact(GetComponent<Player>());
        }
        else
        {
            warningNotification.GetComponentInChildren<Text>().text = "Not enough money!";
            warningNotification.Stop();
            warningNotification.Play();
        }
    }

    public IEnumerator SpeedEffect()
    {
        speedEffect.gameObject.SetActive(true);
        GetComponent<PlayerWalk>().speed *= 1.5f;
        GetComponent<Animator>().SetFloat("walkSpeed", GetComponent<Animator>().GetFloat("walkSpeed") * 1.5f);
        for (int i = 3; i > 0; i--)
        {
            speedEffect.text = i + "s";
            yield return new WaitForSeconds(1);
        }

        speedEffect.gameObject.SetActive(false);
        GetComponent<PlayerWalk>().speed /= 1.5f;
        GetComponent<Animator>().SetFloat("walkSpeed", GetComponent<Animator>().GetFloat("walkSpeed") / 1.5f);
    }

    public IEnumerator MaskEffect()
    {
        GameObject newMask = headmasks[Random.Range(0, headmasks.Length)];
        _ShowObj(newMask);
        _ShowObj(backMask);
        _ShowObj(currentMask);
        maskEffect.gameObject.SetActive(true);
        for (int i = 20; i > 0; i--)
        {
            maskEffect.text = i + "s";
            yield return new WaitForSeconds(1);
        }

        maskEffect.gameObject.SetActive(false);
        _ShowObj(newMask);
        _ShowObj(backMask);
        _ShowObj(currentMask);
    }

    public IEnumerator HideEffect()
    {
        this.photonView.RPC("HideEffectAdd", RpcTarget.All, gameObject.GetPhotonView().ViewID);
        hideCylinder.SetActive(true);
        hideEffect.gameObject.SetActive(true);
        for (int i = 7; i > 0; i--)
        {
            hideEffect.text = i + "s";
            yield return new WaitForSeconds(1);
        }
        hideEffect.gameObject.SetActive(false);
        this.photonView.RPC("HideEffectAdd", RpcTarget.All, gameObject.GetPhotonView().ViewID);
        hideCylinder.SetActive(false);
    }

    [PunRPC]
    public void HideEffectAdd(int ViewID)
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (obj.GetPhotonView().ViewID == ViewID)
            {
                foreach (SkinnedMeshRenderer r in obj.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    r.enabled = !r.enabled;
                }
                return;
            }
        }
    }

    public void GetPistol()
    {
        roleNotification.GetComponentInChildren<Text>().text = "You got one shot!";
        roleNotification.Stop();
        roleNotification.Play();
        policemanButtons.SetActive(true);
    }

    public void UseKey()
    {
        money += keyObj.GetComponent<Interactive>().coins;
        keyObj.GetComponent<Interactive>().Interact(GetComponent<Player>());
    }

    public void LeaveRoom()
    {
        gameManager.Disconnect();
    }

    public void StartMatch()
    {
        gameManager.StartMatch();
    }

    public new void SendMessage()
    {
        gameManager.photonView.RPC("SendMessage", RpcTarget.All, PhotonNetwork.NickName + " : " + textInput.text);
        textInput.text = "";
    }
}
