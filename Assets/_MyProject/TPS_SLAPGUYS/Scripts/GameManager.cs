using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LevelState
{
    Level1,
    Level2,
    Level3
}
public delegate void OnFinishedPlayerCountChanged();
public delegate void OnGameFinished(FinishState state);

namespace PanderaStudios.SlapGuys
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager instance;
        Dictionary<int, Player> players = new();

        private LevelState levelState;
        public int startedPlayersCount;
        public static bool PlayersCanMove = false;

        public event OnFinishedPlayerCountChanged OnPlayerFinished;
        public event OnGameFinished OnGameFinish;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
        }
        void Start()
        {
            //levelState = LevelState.Level1;
            levelState = (LevelState)Getlevel();


            CountdownTimerSync.OnCountdownTimerHasExpired += TimerHasExpired;
            OnLevelStart();
        }
        void OnLevelStart()
        {
            while (!CheckLoadedPlayers()) { }

            if (PhotonNetwork.InRoom)
            {
                CountdownTimerSync.SetStartTime();
                //PhotonNetwork.CurrentRoom.FinishedPlayers.Clear();
                startedPlayersCount = PhotonNetwork.CurrentRoom.PlayerCount;
                //PhotonNetwork.CurrentRoom.ExceptedFinishCount = startedPlayersCount / 2;
            }

            InGameUIManager.instance?.SetupInstance();

            //if (startedPlayersCount == 1)
                //PhotonNetwork.CurrentRoom.ExceptedFinishCount++;

        }

        int Getlevel()
        {
            switch (SceneManager.GetActiveScene().buildIndex)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }
            return -1;
        }

        public IEnumerator NextLevel()
        {
            //if (!PhotonNetwork.CurrentRoom.FinishedPlayers.Values.Contains(PhotonNetwork.LocalPlayer))
            if (!PhotonNetwork.CurrentRoom.Players.Values.Contains(PhotonNetwork.LocalPlayer))
            {
                OnGameFinish?.Invoke(FinishState.Eliminated);
                PhotonNetwork.CurrentRoom.Players.Remove(PhotonNetwork.LocalPlayer.ActorNumber);
                PhotonNetwork.CloseConnection(PhotonNetwork.LocalPlayer);
                yield return new WaitForSeconds(2);
                SceneManager.LoadScene(0);
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LeaveLobby();
            }
            //else if (PhotonNetwork.CurrentRoom.ExceptedFinishCount == 1)
            //{
            //    OnGameFinish?.Invoke(FinishState.Win);
            //}
            else
            {
                OnGameFinish?.Invoke(FinishState.Qualified);
            }
            yield return null;
        }

        public void LoadNextLevel()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            switch (levelState)
            {
                case LevelState.Level1:
                    PhotonNetwork.LoadLevel(GetNextSceneName());
                    OnLevelStart();
                    break;
                case LevelState.Level2:
                    PhotonNetwork.LoadLevel(GetNextSceneName());
                    OnLevelStart();
                    break;
                case LevelState.Level3:
                    Debug.Log($"Player {PhotonNetwork.CurrentRoom.Players[0].NickName} has won the game!");
                    break;
            }
        }
        [PunRPC]
        public void LoadNextLevelRPC()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            switch (levelState)
            {
                case LevelState.Level1:
                    PhotonNetwork.LoadLevel(GetNextSceneName());
                    while (!CheckLoadedPlayers()) { }
                    levelState = LevelState.Level2;
                    //PhotonNetwork.CurrentRoom.FinishedPlayers.Clear();
                    startedPlayersCount = PhotonNetwork.CurrentRoom.PlayerCount;
                    //PhotonNetwork.CurrentRoom.ExceptedFinishCount = (startedPlayersCount / 2) == 0 ? 1 : startedPlayersCount / 2;
                    InGameUIManager.instance?.SetupInstance();
                    CountdownTimerSync.SetStartTime();
                    break;
                case LevelState.Level2:
                    PhotonNetwork.LoadLevel(GetNextSceneName());
                    while (!CheckLoadedPlayers()) { }
                    levelState = LevelState.Level3;
                    //PhotonNetwork.CurrentRoom.FinishedPlayers.Clear();
                    startedPlayersCount = PhotonNetwork.CurrentRoom.PlayerCount;
                    //PhotonNetwork.CurrentRoom.ExceptedFinishCount = 1;
                    InGameUIManager.instance?.SetupInstance();
                    CountdownTimerSync.SetStartTime();
                    break;
                case LevelState.Level3:
                    //Debug.Log($"Player {PhotonNetwork.CurrentRoom.FinishedPlayers[0].NickName} has won the game!");
                    Debug.Log($"Player {PhotonNetwork.CurrentRoom.Players[0].NickName} has won the game!");
                    break;
            }
        }

        private string GetNextSceneName()
        {
            string originalName = SceneManager.GetActiveScene().name;
            string prefix = "Level";
            string suffix = originalName[^2..];
            int nextIndex = int.Parse(originalName[5].ToString()) + 1;
            return prefix + nextIndex.ToString() + suffix;
        }
        private void TimerHasExpired()
        {
            PlayersCanMove = true;
            // Reset the countdown timer for the next game
           // CountdownTimer.SetStartTime();
        }
        public bool CheckLoadedPlayers()
        {
            //int res = 0;
            int res = players.Count;
            //foreach (Player p in players.Values)
            //{
            //    if (p.isSceneLoaded)
            //    {
            //        ++res;
            //        p.isSceneLoaded = false;
            //    }
            //}
            return res == players.Values.Count;
        }
        public IEnumerator CheckFinishState()
        {
            int viewID = photonView.ViewID;
           // PhotonNetwork.CurrentRoom.AddFinishedPlayer(PhotonNetwork.LocalPlayer);
            photonView.RPC("AddFinishedPlayerRPC", RpcTarget.OthersBuffered, PhotonNetwork.LocalPlayer);

            OnPlayerFinished?.Invoke();
            //if (PhotonNetwork.CurrentRoom.FinishedPlayersCount == PhotonNetwork.CurrentRoom.ExceptedFinishCount)
            if (PhotonNetwork.CurrentRoom.Players.Count == 1);// PhotonNetwork.CurrentRoom.ExceptedFinishCount)
            {
                Debug.Log("Game Finished");
                OnGameFinished();
                photonView.RPC("OnGameFinishedRPC", RpcTarget.OthersBuffered);
                yield return new WaitForSeconds(3f);
                if (!PhotonNetwork.LocalPlayer.IsMasterClient)
                    photonView.RPC("LoadNextLevelRPC", RpcTarget.MasterClient);
                else
                    LoadNextLevel();
            }
        }

        [PunRPC]
        public void AddFinishedPlayerRPC(Player player)
        {
            //PhotonNetwork.CurrentRoom.AddFinishedPlayer(player);
        }
        private void OnGameFinished()
        {
            StartCoroutine(NextLevel());
        }

        [PunRPC]
        public void OnGameFinishedRPC()
        {
            GameManager.instance.StartCoroutine(NextLevel());
        }
    }
}