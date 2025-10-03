using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScreenController : MonoBehaviour
{
    void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>();

        buttons[0].onClick.AddListener(() =>
        {
            GameSettingsData.enemyType = EnemyType.SingleScreen;
            LoadBoardScreen();
        });
        buttons[1].onClick.AddListener(() =>
        {
            GameSettingsData.enemyType = EnemyType.Bot;
            LoadBoardScreen();
        });
        buttons[2].onClick.AddListener(() =>
        {
            GameSettingsData.enemyType = EnemyType.Multiplayer;
            LoadBoardScreen();
        });
    }

    private void LoadBoardScreen()
    {
        SceneManager.LoadScene("BoardScreen");
    }
}
