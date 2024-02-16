using UnityEngine;
using Photon.Pun;
//this class is for spawning the player i.e. the user prefab
public class PlayerSpawner : MonoBehaviourPun
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] Transform SpawnPoint;
    [SerializeField] Transform ParentTransform;

    private void Start()
    {
        RectTransform temp = new RectTransform();
        GameObject playerObj = new GameObject();

        if (PlayerSetup.LocalPlayerInstance == null)
        {
            playerObj = PhotonNetwork.Instantiate(this.playerPrefab.name, SpawnPoint.GetComponent<RectTransform>().anchoredPosition, SpawnPoint.rotation, 0) as GameObject;
            //playerObj = PhotonNetwork.InstantiateRoomObject(this.playerPrefab.name, SpawnPoint.GetComponent<RectTransform>().anchoredPosition, SpawnPoint.rotation, 0) as GameObject;
   
            //playerObj.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
            playerObj.gameObject.tag = "Player" + PhotonNetwork.LocalPlayer.ActorNumber; //tag the player gameobject as "Player 1", etc
        }
        //positioning the prefab properly on the screen
        playerObj.transform.SetParent(ParentTransform, false); 
        playerObj.transform.localPosition = SpawnPoint.GetComponent<RectTransform>().localPosition;
    }
}
