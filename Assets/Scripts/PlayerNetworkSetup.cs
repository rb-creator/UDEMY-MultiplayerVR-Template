using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class PlayerNetworkSetup : MonoBehaviourPunCallbacks
{
    public GameObject LocalXROrigin;
    public GameObject MainAvatarGameObject;

    public GameObject AvatarHead;
    public GameObject AvatarBody;

    public GameObject[] AvatarModelPrefabs;

    public TextMeshProUGUI PlayerName_Text;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            //The Player is Local
            LocalXROrigin.SetActive(true);

            //Get the avatar selection data so that the correct avatar model is instantiated
            object avatarSelectionNumber;

            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(MultiplayerVRConstants.AVATAR_SELECTION_NUMBER,out avatarSelectionNumber))
            {
                Debug.Log("Avatar selection number: " + (int)avatarSelectionNumber);
                photonView.RPC("InitializeSelectedAvatarModel", RpcTarget.AllBuffered,(int)avatarSelectionNumber);
            }

            SetLayerRecursively(AvatarHead, 6);
            SetLayerRecursively(AvatarBody, 7);

            TeleportationArea[] teleportationAreas = GameObject.FindObjectsOfType<TeleportationArea>();
            if (teleportationAreas.Length > 0)
            {
                Debug.Log("Found " + teleportationAreas.Length + " teleportation area. ");
                foreach (var item in teleportationAreas)
                {
                    item.teleportationProvider = LocalXROrigin.GetComponent<TeleportationProvider>();
                }
            }
            MainAvatarGameObject.AddComponent<AudioListener>();    
        }
        else
        {
            //The Player is Remote
            LocalXROrigin.SetActive(false);

            SetLayerRecursively(AvatarHead, 0);
            SetLayerRecursively(AvatarBody, 0);
        }

        if (PlayerName_Text != null)
        {
            PlayerName_Text.text = photonView.Owner.NickName;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetLayerRecursively(GameObject go, int layerNumber)
    {
        if (go == null) return;
        foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = layerNumber;
        }
    }

    [PunRPC]
    public void InitializeSelectedAvatarModel(int avatarSelectionNumber)
    {
        GameObject selectedAvatarGameobject = Instantiate(AvatarModelPrefabs[avatarSelectionNumber], LocalXROrigin.transform);

        AvatarInputConverter avatarInputConverter = LocalXROrigin.GetComponent<AvatarInputConverter>();
        AvatarHolder avatarHolder = selectedAvatarGameobject.GetComponent<AvatarHolder>();
        SetUpAvatarGameobject(avatarHolder.HeadTransform, avatarInputConverter.AvatarHead);
        SetUpAvatarGameobject(avatarHolder.BodyTransform, avatarInputConverter.AvatarBody);
        SetUpAvatarGameobject(avatarHolder.HandLeftTransform, avatarInputConverter.AvatarHand_Left);
        SetUpAvatarGameobject(avatarHolder.HandRightTransform, avatarInputConverter.AvatarHand_Right);
    }

    void SetUpAvatarGameobject(Transform avatarModelTransform, Transform mainAvatarTransform)
    {
        avatarModelTransform.SetParent(mainAvatarTransform);
        avatarModelTransform.localPosition = Vector3.zero;
        avatarModelTransform.localRotation = Quaternion.identity;
    }
}
