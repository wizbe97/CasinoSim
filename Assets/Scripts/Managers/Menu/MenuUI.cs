using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [Header("MENU ITEMS")]
    public GameObject menuPanel;
    public Button newGameButton;
    public Button loadGameButton;

    [Header("NEW GAME SLOT")]
    public GameObject chooseSlotPanel;
    public Button chooseSlotBackButton;
    public Button[] chooseSlotButtons;
    public TextMeshProUGUI[] chooseSlotLabels;


    [Header("LOAD GAME ITEMS")]
    public Button loadGameBackButton;
    public GameObject loadGamePanel;
    public Button[] loadSlotButtons;
    public Button[] removeSlotButtons;

    public TextMeshProUGUI autoSaveSlotText;

    public GameManagerSO gameManager;

    void Start()
    {
        loadGameBackButton.onClick.AddListener(LoadGameBackClick);
        chooseSlotBackButton.onClick.AddListener(ChooseSlotBackClick);
        newGameButton.onClick.AddListener(StartNewGame);
        loadGameButton.onClick.AddListener(LoadGameClick);

        for (int i = 0; i < loadSlotButtons.Length; i++)
        {
            int slot = i;
            loadSlotButtons[i].onClick.AddListener(() => OnSlotButtonClicked(slot));
            removeSlotButtons[i].onClick.AddListener(() => OnRemoveSlotClicked(slot));
        }

        for (int i = 0; i < chooseSlotButtons.Length; i++)
        {
            int slot = i;
            chooseSlotButtons[i].onClick.AddListener(() => OnChooseSlotClicked(slot));
        }

        UpdateSlotButtons();
        UpdateChooseSlotButtons();
    }

    private void UpdateSlotButtons()
    {
        for (int i = 0; i < loadSlotButtons.Length; i++)
        {
            int slot = i;
            if (gameManager.saveManager.IsDataSaved(slot))
            {
                if (i == 0)
                {
                    autoSaveSlotText.text = "AUTO SAVE\n<size=20>" + gameManager.saveManager.GetSaveTime(slot) + "</size>";
                }
                removeSlotButtons[i].gameObject.SetActive(true);
                loadSlotButtons[i].interactable = true;
                loadGameButton.interactable = true;
            }
            else
            {
                removeSlotButtons[i].gameObject.SetActive(false);
                loadSlotButtons[i].interactable = false;
            }
        }
    }
    private void UpdateChooseSlotButtons()
    {
        for (int i = 0; i < chooseSlotButtons.Length; i++)
        {
            int slot = i;
            if (gameManager.saveManager.IsDataSaved(slot))
            {
                chooseSlotLabels[i].text = "Override";
                if (i == 0)
                {
                    chooseSlotLabels[i].text += "\n <size=20>(Auto Saved!)</size>";
                }
            }
            else
            {
                chooseSlotLabels[i].text = "EMPTY";
            }


        }
    }

    private void OnSlotButtonClicked(int slot)
    {
        if (gameManager.saveManager.IsDataSaved(slot))
        {
            Debug.Log("DATA SAVED");
            Load(slot);
        }
        else
        {
            Debug.Log("DATA NOT SAVED    " + slot);
        }
        UpdateSlotButtons();
    }

    private void OnChooseSlotClicked(int slot)
    {
        OverrideSlot(slot);
    }

    private void OnRemoveSlotClicked(int slot)
    {
        gameManager.saveManager.RemoveSlot(slot);
        UpdateSlotButtons();
    }

    private void Load(int slot)
    {
        gameManager.saveManager.currentSlot = slot;
        SceneManager.LoadScene("Casino");
    }

    private void OverrideSlot(int slot)
    {
        gameManager.saveManager.RemoveSlot(slot);
        gameManager.saveManager.currentSlot = slot;
        SceneManager.LoadScene("Casino");
    }

    private void StartNewGame()
    {
        chooseSlotPanel.SetActive(true);
        menuPanel.SetActive(false);
        UpdateChooseSlotButtons();
    }

    private void LoadGameBackClick()
    {
        menuPanel.SetActive(true);
        loadGamePanel.SetActive(false);
    }

    private void ChooseSlotBackClick()
    {
        menuPanel.SetActive(true);
        chooseSlotPanel.SetActive(false);
    }

    private void LoadGameClick()
    {
        menuPanel.SetActive(false);
        loadGamePanel.SetActive(true);
        UpdateSlotButtons();
    }
}
