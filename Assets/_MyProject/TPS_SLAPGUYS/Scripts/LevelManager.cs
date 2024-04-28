using UnityEngine;
using Photon.Pun;

namespace PanderaStudios.SlapGuys
{
    public class LevelManager : MonoBehaviourPunCallbacks
    {
        public static LevelManager instance;
        [SerializeField] GameObject playerPrefab;
        [SerializeField] Transform spawnPoints;

        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            InGameUIManager.instance?.OnLevelStart();
            int i = Random.Range(0, spawnPoints.childCount);

            if (PhotonNetwork.InRoom)
                PhotonNetwork.Instantiate(playerPrefab.name, spawnPoints.GetChild(i).position, Quaternion.identity);
            else
                Instantiate(playerPrefab, spawnPoints.GetChild(i).position, Quaternion.identity);
        }
    }
}