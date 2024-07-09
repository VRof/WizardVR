using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStartMenu : MonoBehaviour
{
    bool profileSelected;
    string profileModelName;
    int profileIndex;
    public List<TMP_Dropdown.OptionData> profiles;

    [Header("UI Pages")]
    public GameObject mainMenu;
    public GameObject profileMenu;
    public GameObject createProfileMenu;
    public GameObject options;
    public GameObject about;
   
    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button selectProfile;
    public Button optionButton;
    public Button aboutButton;
    public Button quitButton;
    public List<Button> returnButtons;

    [Header("Select Profile Menu Buttons")]
    public Button select;
    public Button createProfile;
    public TMP_Dropdown dropDownProfiles;

    [Header("Create Profile Menu Buttons")]
    public TMP_InputField nameInputField;
    public Button create;
    public TMP_Text createProfileMenuLabel;
    // Start is called before the first frame update
    void Start()
    {
        profiles = new List<TMP_Dropdown.OptionData>();
        EnableMainMenu();
        //Hook events
        profileSelected = false;
        startButton.interactable = false;
        startButton.onClick.AddListener(StartGame);
        optionButton.onClick.AddListener(EnableOption);
        aboutButton.onClick.AddListener(EnableAbout);
        quitButton.onClick.AddListener(QuitGame);
        selectProfile.onClick.AddListener(EnableSelectProfile);
        select.onClick.AddListener(ProfileSelectedYes);
        createProfile.onClick.AddListener(CreateProfile);
        dropDownProfiles.onValueChanged.AddListener(OnDropdownValueChanged);
        create.onClick.AddListener(CreateNewProfileFile);
        foreach (var item in returnButtons)
        {
            item.onClick.AddListener(EnableMainMenu);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    public void CreateProfile()
    {
        EnableCreateProfileMenu();
    }
    public void EnableCreateProfileMenu()
    {
        mainMenu.SetActive(false);
        profileMenu.SetActive(false);
        createProfileMenu.SetActive(true);
        options.SetActive(false);
        about.SetActive(false);
    }
    public void CreateNewProfileFile()
    {
        bool isNameValid = true;
        Debug.Log(nameInputField.text);
        foreach (TMP_Dropdown.OptionData o in profiles)
        {
            Debug.Log(o.text);
            Debug.Log(o.text.Equals(nameInputField.text));
            Debug.Log(o.text == nameInputField.text);
            if (o.text.Equals(nameInputField.text))
            {
                createProfileMenuLabel.text = "Choose another name";
                //createProfileMenuLabel.IsActive();
                createProfileMenuLabel.enabled = true;
                isNameValid = false;
            }
        }
        if(isNameValid == true)
        {
            CreateModelForNewProfile();
            EnableSelectProfile();
        }
    }
    public void CreateModelForNewProfile()
    {
        string sourceFilePath = Path.Combine(Application.dataPath, "Scripts", "UnityPython", "models","base.h5");  
        string destinationFilePath = Path.Combine(Application.dataPath, "Scripts", "UnityPython", "models", nameInputField.text + ".h5");
        try
        {
            File.Copy(sourceFilePath, destinationFilePath, true);
        }
        catch (System.Exception ex)
        {
           Debug.Log($"An error occurred: {ex.Message}");
        }
    }
    public void ProfileSelectedYes()
    {
        startButton.interactable = false;
        profileSelected = false;
        if(dropDownProfiles.value != 0)
        {
            profileSelected = true;
            startButton.interactable = true;
        }
        EnableMainMenu();
    }
    public void StartGame()
    {
        if(profileSelected == true)
        {
            pythonConnector.modelName = profileModelName;
            HideAll();
            SceneTransitionManager.singleton.GoToSceneAsync(1);
        }
        else
        {
            Debug.Log("you need to select profile!");
        }
    }
    
    public void HideAll()
    {
        mainMenu.SetActive(false);
        profileMenu.SetActive(false);
        createProfileMenu.SetActive(false);
        options.SetActive(false);
        about.SetActive(false);
    }
    public void EnableMainMenu()
    {
        mainMenu.SetActive(true);
        profileMenu.SetActive(false);
        createProfileMenu.SetActive(false);
        options.SetActive(false);
        about.SetActive(false);
    }
    public void EnableSelectProfile()
    {
        mainMenu.SetActive(false);
        profileMenu.SetActive(true);
        createProfileMenu.SetActive(false);
        options.SetActive(false);
        about.SetActive(false);
        createProfileMenuLabel.enabled = false;
        // Collect all profile names based on models amount and names
        string profilesFolderPath = Path.Combine(Application.dataPath, "Scripts", "UnityPython", "models");
        List<string> fileNames = new List<string>();
        string[] files = Directory.GetFiles(profilesFolderPath);

        //Collect files that ends in .h5 extention
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            if (fileName.EndsWith(".h5", System.StringComparison.OrdinalIgnoreCase))
            {
                fileNames.Add(fileName);
            }
        }
        profiles.Clear();
        profiles.Add(new TMP_Dropdown.OptionData("Select..", null));
        foreach (string name in fileNames)
        {
            if (!name.Equals("base.h5"))
            {
                profiles.Add(new TMP_Dropdown.OptionData(Path.GetFileNameWithoutExtension(name), null));
            }
        }

        //Update Profile Names DropDown
        dropDownProfiles.ClearOptions();
        dropDownProfiles.AddOptions(profiles);
        dropDownProfiles.RefreshShownValue();
        dropDownProfiles.value = profileIndex;
    }
    public void OnDropdownValueChanged(int index)
    {
        profileIndex = index;
        profileModelName = dropDownProfiles.options[index].text;
        Debug.Log($"Selected profile: {dropDownProfiles.options[index].text}");
    }
    public void EnableOption()
    {
        mainMenu.SetActive(false);
        profileMenu.SetActive(false);
        createProfileMenu.SetActive(false);
        options.SetActive(true);
        about.SetActive(false);
    }
    public void EnableAbout()
    {
        mainMenu.SetActive(false);
        profileMenu.SetActive(false);
        createProfileMenu.SetActive(false);
        options.SetActive(false);
        about.SetActive(true);
    }
}
