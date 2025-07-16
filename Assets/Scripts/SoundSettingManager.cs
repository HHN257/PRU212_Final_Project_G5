using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoundSettingManager : MonoBehaviour
{
    public GameObject soundSettingPanel;
    public Slider masterSlider; // Tổng thể
    public Slider musicSlider;  // Nhạc nền
    public TextMeshProUGUI masterValueText;
    public TextMeshProUGUI musicValueText;
    public AudioSource backgroundMusic;
    public Button quitButton; // Nút Quit Game
    public Button resetButton; // Nút Reset

    void Start()
    {
        // Gán giá trị mặc định
        masterSlider.value = AudioListener.volume;
        musicSlider.value = backgroundMusic != null ? backgroundMusic.volume : 1f;

        masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);

        UpdateMasterText(masterSlider.value);
        UpdateMusicText(musicSlider.value);

        // Đảm bảo nhạc nền đúng giá trị thực tế khi vào game
        UpdateBackgroundMusicVolume();

        // Gán sự kiện cho nút Quit Game
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        // Gán sự kiện cho nút Reset
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetSoundSettings);
    }

    void Update()
    {
        // Đóng panel khi nhấn Escape
        if (soundSettingPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            HidePanel();
        }
    }

    public void OnMasterSliderChanged(float value)
    {
        AudioListener.volume = value;
        UpdateMasterText(value);
        UpdateBackgroundMusicVolume();
    }

    public void OnMusicSliderChanged(float value)
    {
        UpdateMusicText(value);
        UpdateBackgroundMusicVolume();
    }

    private void UpdateBackgroundMusicVolume()
    {
        if (backgroundMusic != null)
            backgroundMusic.volume = musicSlider.value * masterSlider.value;
    }

    private void UpdateMasterText(float value)
    {
        if (masterValueText != null)
            masterValueText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    private void UpdateMusicText(float value)
    {
        if (musicValueText != null)
            musicValueText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    public void ShowPanel()
    {
        soundSettingPanel.SetActive(true);
    }

    public void HidePanel()
    {
        soundSettingPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game!");
    }

    public void ResetSoundSettings()
    {
        masterSlider.value = 1f;
        musicSlider.value = 1f;
        // Các hàm onValueChanged sẽ tự động cập nhật âm lượng và text
    }
} 