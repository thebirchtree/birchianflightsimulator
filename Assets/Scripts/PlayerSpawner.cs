
using Photon.Pun;
using UnityEngine;
using Cinemachine;
namespace PhotonTutorial
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab = null;
        [SerializeField] private CinemachineVirtualCamera playerCamera = null;

        private void Start()
        {
           // var player = 
				PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
          // playerCamera.Follow = player.transform;
          // playerCamera.LookAt = player.transform;
        }
    }
}
