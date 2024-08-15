using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InGameMenuScript : MonoBehaviour
{
    [SerializeField] GameObject pauseMenuCanvas;
    [SerializeField] GameObject rayInteractor;
    [SerializeField] Button continueBtn;
    [SerializeField] Button exitToMenuBtn;
    bool paused = false;
    // Start is called before the first frame update
    void Start()
    {
        continueBtn.onClick.AddListener(ContinueButtonHandler);
        exitToMenuBtn.onClick.AddListener(ExitToMenuButtonHandler);
    }

    // Update is called once per frame
    void Update()
    {
        InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).IsPressed(InputHelpers.Button.MenuButton, out bool MenuButtonPressed);
        if (!paused && MenuButtonPressed) {
            gameObject.GetComponent<Draw>().enabled = false;
            rayInteractor.SetActive(true);
            pauseMenuCanvas.SetActive(true);
            paused = true;
            Time.timeScale = 0f;
        }
    }

    public void ContinueButtonHandler() { 
        pauseMenuCanvas?.SetActive(false);
        rayInteractor.SetActive(false);
        gameObject.GetComponent<Draw>().enabled = true;
        Time.timeScale = 1f;
        paused = false;
    }

    public void ExitToMenuButtonHandler() {
        pauseMenuCanvas?.SetActive(false);
        rayInteractor?.SetActive(false);
        Time.timeScale = 1f;
        SceneTransitionManager.singleton.GoToScene(0);
        paused = false;
    }
}
