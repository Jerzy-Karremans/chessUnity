using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static BoardLogic;

public class MenuScreenController : MonoBehaviour
{

    void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>();

        buttons[0].onClick.AddListener(() => SingleScreenMode());
        buttons[1].onClick.AddListener(() => BotMode());
        buttons[2].onClick.AddListener(() => MultiplayerMode());
    }

    void SingleScreenMode()
    {
        GameSettings.enemyType = GameSettings.EnemyType.SingleScreen;
        SceneManager.LoadScene("BoardScreen");
    }

    void MultiplayerMode()
    {
        GameSettings.enemyType = GameSettings.EnemyType.Multiplayer;
        SceneManager.LoadScene("BoardScreen");
    }

    void BotMode()
    {
        GameSettings.enemyType = GameSettings.EnemyType.RandomBot;
        SceneManager.LoadScene("BoardScreen");
    }
}
