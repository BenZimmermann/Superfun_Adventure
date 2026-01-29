using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image[] lifeIcons;

    [Header("Psycho")]
    [SerializeField] private Slider psychosisSlider;

    [Header("Full")]
    [SerializeField] private Sprite lifeFullSprite;
    [SerializeField] private Sprite lifeEmptySprite;

    private PlayerMovement player;



    private void OnEnable()
    {
        GameManager.Instance.OnPlayerReady += BindPlayer;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerReady -= BindPlayer;
    }
    private void BindPlayer(PlayerMovement p)
    {
        player = p;

        player.OnHealthChanged += UpdateHealth;
       player.OnPsychosisChanged += UpdatePsychosis;
       // player.OnPlayerDied += OnPlayerDied;
    }

    private void OnDestroy()
    {
        if (player == null) return;

        player.OnHealthChanged -= UpdateHealth;
        player.OnPsychosisChanged -= UpdatePsychosis;
        //player.OnPlayerDied -= OnPlayerDied;
    }
    private void UpdatePsychosis(float current, float max)
    {
        psychosisSlider.maxValue = max;
        psychosisSlider.value = current;
    }
    private void UpdateHealth(int current, int max)
    {
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (i < current)
            {
                lifeIcons[i].sprite = lifeFullSprite;
                lifeIcons[i].enabled = true;
            }
            else
            {
                lifeIcons[i].sprite = lifeEmptySprite;
                lifeIcons[i].enabled = true;
            }
        }
    }


}
