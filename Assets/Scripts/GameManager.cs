using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Globalization;


public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    [Header("------ Menu UI --------")]
    //[SerializeField] GameObject menuActive;
    // [SerializeField] GameObject menuPause;

    [Header("------ Player UI --------")]
    public Image playerHPBar;
    public Image playerStaminaBar;
    public Image playerQiBar;
    public Image playerExpBar;

    [Header("------ Game Data --------")]
    public GameObject player;
    public playerController playerScript;
    public bool isPaused;


    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            statePause();

        }
        else
        {
            stateUnpause();
        }
    }

    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void stateUnpause()
    {
        isPaused = !isPaused;
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }
}
