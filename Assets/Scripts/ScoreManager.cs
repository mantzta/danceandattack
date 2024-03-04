using TMPro;
using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ScoreManager : MonoBehaviourPun
{
    public bool turnOffAdaptation = false;
    public WinningBar WinningBarLeft;
    public WinningBar WinningBarRight;

    public Saber redSaber; // Reference to the Saber script of local player
    public Saber blueSaber;

    public TextMeshProUGUI scoreTextPlayerLeft;
    public TextMeshProUGUI scoreTextPlayerRight;

    public TextMeshProUGUI placeTextPlayerLeft;
    public TextMeshProUGUI placeTextPlayerRight;

    public TextMeshProUGUI streakTextPlayerLeft;
    public TextMeshProUGUI streakTextPlayerRight;

    public TextMeshProUGUI LoadingTextPlayerLeft;
    public TextMeshProUGUI LoadingTextPlayerRight;

    public List<float> AdaptationFactors = new List<float>();

    public static int scorePlayerLeft = 0;
    public static int scorePlayerRight = 0;

    public int streakPlayerLeft = 0;
    public int streakPlayerRight = 0;

    public int streakFactorPlayerLeft = 1;
    public int streakFactorPlayerRight = 1;

    public List<int> allStreaksPlayerLeft = new List<int>();
    public List<int> allStreaksPlayerRight = new List<int>();

    public int mistakesPlayerLeft = 0;
    public int mistakesPlayerRight = 0;

    public List<int> allMistakeStreaksPlayerLeft = new List<int>();
    public List<int> allMistakeStreaksPlayerRight = new List<int>();
    private int lastMistakeStreakPlayerLeft = 0;
    private int lastMistakeStreakPlayerRight = 0;

    private Color originalTextColor;

    private bool isFirstHit = true;

    private LoadingBarController loadingBarController;

    void Start()
    {
        SaveLogs.CreateNewFile();
        AdaptationFactors.Add(1);

        AdaptationFactors = new List<float>();

        scorePlayerLeft = 0;
        scorePlayerRight = 0;

        streakPlayerLeft = 0;
        streakPlayerRight = 0;

        streakFactorPlayerLeft = 1;
        streakFactorPlayerRight = 1;

        allStreaksPlayerLeft = new List<int>();
        allStreaksPlayerRight = new List<int>();

        mistakesPlayerLeft = 0;
        mistakesPlayerRight = 0;

        allMistakeStreaksPlayerLeft = new List<int>();
        allMistakeStreaksPlayerRight = new List<int>();
        lastMistakeStreakPlayerLeft = 0;
        lastMistakeStreakPlayerRight = 0;

        // Initialize the custom score property of the player
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "TheScore", 0 } });
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "StreakFactor", 1 } });
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "LoadingFactor", 1 } });

        InvokeRepeating("UpdateOpponentGui", 0, 1);
        InvokeRepeating("SavePlayersLogs", 0, 10);

        Debug.Log("LOCAL PLAYER PROPS" + PhotonNetwork.LocalPlayer.CustomProperties);

        originalTextColor = scoreTextPlayerLeft.color;
    }

    // Add score points
    public void AddScore(int score, bool isPlayerLeft)
    {
        // Left ------------------------------------------------------------------------------------
        if (isPlayerLeft)
        {
            if(isFirstHit)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "TheScore", scorePlayerLeft } });
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "StreakFactor", 1 } });
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "LoadingFactor", 1 } });
                isFirstHit = false;
            }
            
            if (score > 0)
            {
                allMistakeStreaksPlayerLeft.Add(lastMistakeStreakPlayerLeft);

                lastMistakeStreakPlayerLeft = 0;
                streakPlayerLeft++;
            } else
            {
                allStreaksPlayerLeft.Add(streakPlayerLeft);

                streakPlayerLeft = 0;
                mistakesPlayerLeft++;
                lastMistakeStreakPlayerLeft++;

                StartCoroutine(ChangeTextColorToRed(scoreTextPlayerLeft));
            }

            streakFactorPlayerLeft = ((int)(streakPlayerLeft / 10)) + 1;

            var scoreToAdd = (score * streakFactorPlayerLeft);
            scorePlayerLeft += scoreToAdd;
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "TheScore", scorePlayerLeft } });
            UpdateScoreUI(scorePlayerLeft, scoreTextPlayerLeft, streakFactorPlayerLeft, streakTextPlayerLeft);

            //UpdatePlaceText(placeTextPlayerLeft, placeTextPlayerRight, scorePlayerLeft, scorePlayerRight);

            WinningBarLeft.GrowBar(scorePlayerLeft);
        }
        // RIGHT -------------------------------------------------------------------------
        else
        {
            if (isFirstHit)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "TheScore", scorePlayerRight } });
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "StreakFactor", 1 } });
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "LoadingFactor", 1 } });
                isFirstHit = false;
            }
            if (score > 0)
            {
                allMistakeStreaksPlayerRight.Add(lastMistakeStreakPlayerRight);

                streakPlayerRight++;
                lastMistakeStreakPlayerRight = 0;
            }
            else
            {
                allStreaksPlayerRight.Add(streakPlayerRight);

                streakPlayerRight = 0;
                mistakesPlayerRight++;
                lastMistakeStreakPlayerRight++;

                StartCoroutine(ChangeTextColorToRed(scoreTextPlayerRight));
            }

            streakFactorPlayerRight = ((int)(streakPlayerRight / 10)) + 1;

            var scoreToAdd = (score * streakFactorPlayerRight);
            scorePlayerRight += scoreToAdd;
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "TheScore", scorePlayerRight } });
            UpdateScoreUI(scorePlayerRight, scoreTextPlayerRight, streakFactorPlayerRight, streakTextPlayerRight);

            //UpdatePlaceText(placeTextPlayerRight, placeTextPlayerLeft, scorePlayerRight, scorePlayerLeft);

            WinningBarRight.GrowBar(scorePlayerRight);
        }

    }

    private void UpdateScoreUI(int score, TextMeshProUGUI gui, int streakFactor, TextMeshProUGUI streakGui)
    {
        if (streakFactor > 1)
        {
            streakGui.text = $"Streak: {streakFactor}x";
        } else if (streakFactor == 1)
        {
            streakGui.text = "";
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "StreakFactor", streakFactor } });
        gui.text = "" + score;

    }

    private void UpdateOpponentGui()
    {
        if (PhotonNetwork.PlayerList.Length > 1)
        {
            var isLeft = CheckPlayer.IsPlayerLeft();
            var isRight = CheckPlayer.IsPlayerRight();

            ExitGames.Client.Photon.Hashtable props = null;

            // Left ---------------------------------------
            if (isLeft)
            {
                props = PhotonNetwork.PlayerList[1].CustomProperties;

                // Update score
                var opponentScore = props["TheScore"] != null ? ((int)props["TheScore"]) : scorePlayerRight;

                // Check if the last score value was higher than the new one
                if (scorePlayerRight > opponentScore)
                {
                    StartCoroutine(ChangeTextColorToRed(scoreTextPlayerRight));
                }
                scorePlayerRight = opponentScore;
                scoreTextPlayerRight.text = "" + scorePlayerRight;
                //UpdatePlaceText(placeTextPlayerLeft, placeTextPlayerRight, scorePlayerLeft, scorePlayerRight);
                WinningBarRight.GrowBar(scorePlayerRight);

                // Update streaks text
                var opponentStreak = props["StreakFactor"] != null ? ((int)props["StreakFactor"]) : streakFactorPlayerRight;

                if (opponentStreak != streakFactorPlayerRight)
                {
                    if (opponentStreak == 1)
                    {
                        streakTextPlayerRight.text = "";
                    } else
                    {
                        streakTextPlayerRight.text = $"Streak: {opponentStreak}x";
                        streakFactorPlayerRight = opponentStreak;
                    }

                }

                // Show loading factor for other person too
                if (props.ContainsKey("LoadingFactor") && props["LoadingFactor"] is float)
                {
                    var opponentLoadingFactor = (float)props["LoadingFactor"];

                    if (opponentLoadingFactor > 1)
                    {
                        LoadingTextPlayerRight.text = $"{(int)opponentLoadingFactor}x";
                    }
                    else
                    {
                        LoadingTextPlayerRight.text = "";
                    }
                }

                

                if (!turnOffAdaptation)
                {
                    AdaptGame(scorePlayerLeft, scorePlayerRight, allMistakeStreaksPlayerLeft, mistakesPlayerLeft);
                }


                // Right ----------------------------------------
            } else if (isRight)
            {
                props = PhotonNetwork.PlayerList[0].CustomProperties;

                // Update opponent score
                var opponentScore = props["TheScore"] != null ? ((int)props["TheScore"]) : scorePlayerLeft;

                if (scorePlayerLeft > opponentScore)
                {
                    StartCoroutine(ChangeTextColorToRed(scoreTextPlayerLeft));
                }
                scorePlayerLeft = opponentScore;
                scoreTextPlayerLeft.text = "" + scorePlayerLeft;
                //UpdatePlaceText(placeTextPlayerRight, placeTextPlayerLeft, scorePlayerRight, scorePlayerLeft);
                WinningBarLeft.GrowBar(scorePlayerLeft);

                // Update opponent streak
                var opponentStreak = props["StreakFactor"] != null ? ((int)props["StreakFactor"]) : streakFactorPlayerLeft;

                if (opponentStreak != streakFactorPlayerLeft)
                {
                    if (opponentStreak == 1)
                    {
                        streakTextPlayerLeft.text = "";
                    }
                    else
                    {
                        streakTextPlayerLeft.text = $"Streak: ${opponentStreak}x";
                        streakFactorPlayerLeft = opponentStreak;
                    }

                }

                // Show loading factor for other person too
                if (props.ContainsKey("LoadingFactor") && props["LoadingFactor"] is float)
                {
                    var opponentLoadingFactor = (float)props["LoadingFactor"];

                    if (opponentLoadingFactor > 1)
                    {
                        LoadingTextPlayerLeft.text = $"{(int)opponentLoadingFactor}x";
                    }
                    else
                    {
                        LoadingTextPlayerLeft.text = "";
                    }
                }

                
                if (!turnOffAdaptation)
                {
                    AdaptGame(scorePlayerRight, scorePlayerLeft, allMistakeStreaksPlayerRight, mistakesPlayerRight);
                }
            }
        }
    }

    // Coroutine to change text color to red temporarily
    private IEnumerator ChangeTextColorToRed(TextMeshProUGUI text)
    {
        text.color = Color.red;

        // Wait for a few seconds (you can adjust the duration)
        yield return new WaitForSeconds(0.5f);  // Change this value as needed

        // Return the text color to the original color
        text.color = originalTextColor;
    }

    private void SavePlayersLogs()
    {
        if (CheckPlayer.IsPlayerLeft())
        {
            LogData data = new LogData("Left", scorePlayerLeft, allStreaksPlayerLeft, mistakesPlayerLeft, allMistakeStreaksPlayerLeft, AdaptationFactors);
            SaveLogs.SaveLogData(data);
        }
        else
        {
            LogData data = new LogData("Right", scorePlayerRight, allStreaksPlayerRight, mistakesPlayerRight, allMistakeStreaksPlayerRight, AdaptationFactors);
            SaveLogs.SaveLogData(data);
        }
    }

    private void UpdatePlaceText(TextMeshProUGUI textPlayer, TextMeshProUGUI textOpponent, int scorePlayer, int scoreOpponent)
    {
        if (scorePlayer > scoreOpponent)
        {
            textPlayer.text = "1. place";
            textOpponent.text = "2. place";
        } else if (scorePlayer < scoreOpponent)
        {
            textOpponent.text = "1. place";
            textPlayer.text = "2. place";
        } else
        {
            textPlayer.text = "2. place";
            textOpponent.text = "2. place";
        }
    }

    private void AdaptGame(int scorePlayer, int scoreOpponent, List<int> mistakeStreaks, int mistakes)
    {
        float factor = 1;
        bool isLeft = CheckPlayer.IsPlayerLeft();

        if (loadingBarController == null)
        {
            loadingBarController = FindLoadingBar();
        }

        if (scorePlayer < scoreOpponent && (scoreOpponent - scorePlayer) > 200)
        {
            float difference = scoreOpponent - scorePlayer;

            int count = 1;
            if (mistakeStreaks.Count > 0)
            {
                count = mistakeStreaks.FindAll(m => m > 2).Count;
                count = count <= 1 ? 1 : count;
            }

            float mistakesFactor = mistakes / 20;
            mistakesFactor = mistakesFactor <= 1 ? 1 : mistakesFactor;

            float rawFactor = (difference / 100) * count * mistakesFactor;

            //factor = 28 / (1 + Mathf.Exp(-rawFactor));

            factor = rawFactor <= 28 ? rawFactor : 28;

            loadingBarController.ChangeLoadingFactor(factor);
            UpdateLoadingText(factor);

            if (difference > 1500)
            {
                redSaber.PowerUpType = PowerUpType.Combi;
                blueSaber.PowerUpType = PowerUpType.Combi;

                redSaber.AttackType = AttackType.Combi;
                blueSaber.AttackType = AttackType.Combi;
            } else if (difference > 1000)
            {
                redSaber.PowerUpType = PowerUpType.NoDirections;
                blueSaber.PowerUpType = PowerUpType.NoDirections;

                redSaber.AttackType = AttackType.Rotate;
                blueSaber.AttackType = AttackType.Rotate;
            } else if (difference > 500)
            {
                redSaber.PowerUpType = PowerUpType.MultiDirections;
                blueSaber.PowerUpType = PowerUpType.MultiDirections;
            }
        } else if ((isLeft && LoadingTextPlayerLeft.text != "") || (!isLeft && LoadingTextPlayerRight.text != ""))
        {
            loadingBarController.ChangeLoadingFactor(factor);
            UpdateLoadingText(factor);
        }

        AdaptationFactors.Add(factor);
    }

    private void UpdateLoadingText(float loadingFactor)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "LoadingFactor", loadingFactor } });
        if (CheckPlayer.IsPlayerLeft())
        {
            if (loadingFactor > 1)
            {
                LoadingTextPlayerLeft.text = $"{(int)loadingFactor}x";
            }
            else
            {
                LoadingTextPlayerLeft.text = "";
            }
        }
        else
        {
            if (loadingFactor > 1)
            {
                LoadingTextPlayerRight.text = $"{(int)loadingFactor}x";
            }
            else
            {
                LoadingTextPlayerRight.text = "";
            }
        }
    }

    private LoadingBarController FindLoadingBar()
    {
        LoadingBarController[] loadingBarControllers = FindObjectsOfType<LoadingBarController>();

        if (CheckPlayer.IsPlayerLeft())
        {
            return loadingBarControllers.First<LoadingBarController>(l => l.LoadBarPosition == "LEFT");
        } else
        {
            return loadingBarControllers.First<LoadingBarController>(l => l.LoadBarPosition == "RIGHT");
        }

    }

}
