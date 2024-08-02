using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class playerController : MonoBehaviour, IDamage
{
    [Header("------ Components --------")]
    [SerializeField] CharacterController controller;


    [Header("------ Stats --------")]
    [SerializeField] int Hp;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;
    [SerializeField] int crouchSpeed;
    [SerializeField] float savingThrowTime;
    [SerializeField] int maxStamina;
    [SerializeField] int maxQi;
    [SerializeField] int expPerQiCycle; // EXP gained per full Qi cycle
    [SerializeField] int expForNextRealm;// EXP required for next realm
    [SerializeField] float cultivateRate = 1.0f;

    [Header("------ Cultivation Effect --------")]
    [SerializeField] GameObject cultivationEffectPrefab; // Reference to the particle effect prefab
    private GameObject cultivationEffectInstance;


    public float crouchHeight = 1.0f; // Height when crouching
    public float standHeight = 2.0f; // Height when standing
    public float crouchVolumeFactor = 0.5f; // Volume reduction factor when crouching
    bool isSprinting;


    bool isShooting;
    bool isCrouching = false;


    public int HPOrig;
    public int staminaOrig;
    int qiOrig;
    int jumpCount;
    public int currentStamina;
    public float currentQi;
    public int currentExp = 0;
    int currentRealm = 1;
    public bool isPaused;
    bool isCultivating;

    Vector3 moveDir;
    Vector3 playerVel;

    // Start is called before the first frame update
    void Start()
    {
        HPOrig = Hp;
        staminaOrig = maxStamina;
        qiOrig = maxQi;
        currentStamina = maxStamina;
        currentQi = 0;
        currentExp = 0;
        updatePlayerUI();
        UpdateExpForNextRealm();
    }

    // Update is called once per frame
    void Update()
    {

       

        movement();

        if (!isSprinting)
        {
            sprint();
        }
        
        crouch();
        updatePlayerUI();
        updateStaminaUI();
        updateQiUI();
        updateExpUI();
        checkCultivation();

        if (Input.GetKey(KeyCode.C))
        {
            if (!isCultivating)
            {
                StartCultivating();
            }
        }
        else
        {
            if (isCultivating)
            {
                StopCultivating();
            }
        }

        if (isCultivating)
        {
            Cultivate();
        }

    }


    // ************ MOVEMENT ************

    void movement()
    {

        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;

        if (isCrouching)
        {
            controller.Move(moveDir * crouchSpeed * Time.deltaTime);
        }
        else
        {
            controller.Move(moveDir * speed * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;


        }
        playerVel.y -= gravity * Time.deltaTime;
        controller.Move(playerVel * Time.deltaTime);


    }
    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
            isSprinting = true;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
            isSprinting = false;
        }
    }

    void crouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standHeight;


        // Check input
        if (Input.GetButtonDown("Crouch"))
        {
            isCrouching = true;

        }
        else if (Input.GetButtonUp("Crouch"))
        {
            isCrouching = false;

        }

        // Smoothly interpolate the height and volume
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 10);

    }

   
    // ************ PLAYER HEALTH,DAMAGE MODIFIERS ************

    public void takeDamage(int amount)
    {
        Hp -= amount;

        updatePlayerUI();

      
    }

    public void AddHealth(int amount)
    {
        Hp += amount;
        if (Hp > HPOrig)
        {
            Hp = HPOrig;
        }
        updatePlayerUI();
    }

    void updatePlayerUI()
    {
        float targetFillAmount = (float)Hp / HPOrig;
        float smoothFillSpeed = 5f;

        GameManager.instance.playerHPBar.fillAmount = Mathf.Lerp(GameManager.instance.playerHPBar.fillAmount, targetFillAmount, Time.deltaTime * smoothFillSpeed);

    }

    void updateStaminaUI()
    {
        float targetFillAmount = (float)currentStamina / staminaOrig;
        float smoothFillSpeed = 5f;

        GameManager.instance.playerStaminaBar.fillAmount = Mathf.Lerp(GameManager.instance.playerStaminaBar.fillAmount, targetFillAmount, Time.deltaTime * smoothFillSpeed);
    }

    void updateQiUI()
    {
        float targetFillAmount = (float)currentQi / qiOrig;
        float smoothFillSpeed = 5f;

        GameManager.instance.playerQiBar.fillAmount = Mathf.Lerp(GameManager.instance.playerQiBar.fillAmount, targetFillAmount, Time.deltaTime * smoothFillSpeed);
    }

    void updateExpUI()
    {
        float targetFillAmount = (float)currentExp / expForNextRealm;
        float smoothFillSpeed = 5f;
        GameManager.instance.playerExpBar.fillAmount = Mathf.Lerp(GameManager.instance.playerExpBar.fillAmount, targetFillAmount, Time.deltaTime * smoothFillSpeed);
    }
    void checkCultivation()
    {
        if (currentQi >= maxQi)
        {
            currentQi = 0;
            currentExp += expPerQiCycle;
            if (currentExp >= expForNextRealm)
            {
                advanceRealm();
            }
            updateQiUI();
        }
    }
    void advanceRealm()
    {
        currentExp = 0;
        currentRealm++;
        UpdateExpForNextRealm(); // Update the experience needed for the next realm
        // Implement further realm-specific effects here
    }

    void UpdateExpForNextRealm()
    {
        switch (currentRealm)
        {
            case 1: expForNextRealm = 100; break; // Qi Condensation
            case 2: expForNextRealm = 100 * 2; break; // Foundation Establishment
            case 3: expForNextRealm = 100 * 4; break; // Core Formation
            case 4: expForNextRealm = 100 * 8; break; // Nascent Soul
            case 5: expForNextRealm = 100 * 16; break; // Deity Transformation
            case 6: expForNextRealm = 100 * 32; break; // Spatial Tempering
            case 7: expForNextRealm = 100 * 64; break; // Body Integration
            case 8: expForNextRealm = 100 * 128; break; // Grand Ascension
            case 9: expForNextRealm = 100 * 256; break; // Tribulation Transcendence
            case 10: expForNextRealm = 100 * 512; break; // Pseudo-Immortal
            case 11: expForNextRealm = 100 * 1024; break; // True Immortal
            case 12: expForNextRealm = 100 * 2048; break; // Golden Immortal
            case 13: expForNextRealm = 100 * 4096; break; // Supreme Unity Jade Immortal
            case 14: expForNextRealm = 100 * 8192; break; // Zenith Heaven
            case 15: expForNextRealm = 100 * 16384; break; // Dao Ancestor
            case 16: expForNextRealm = 100 * 32768; break; // Primordial Chaos Dao Ancestor
            default: expForNextRealm = 100 * 32768; break; // Ensure it doesn't go beyond the last realm
        }
    }

    void Cultivate()
    {
        currentQi += 0.1f * cultivateRate; // Increase Qi by 10 when cultivating, you can adjust this value
        if (currentQi > maxQi)
        {
            currentQi = maxQi;
        }
        updateQiUI();
    }
    void StartCultivating()
    {
        isCultivating = true;
        if (cultivationEffectPrefab != null && cultivationEffectInstance == null)
        {
            Vector3 effectPosition = new Vector3(transform.position.x, transform.position.y - controller.height / 2, transform.position.z);
            cultivationEffectInstance = Instantiate(cultivationEffectPrefab, effectPosition, Quaternion.identity, transform);
        }
    }

    void StopCultivating()
    {
        isCultivating = false;
        if (cultivationEffectInstance != null)
        {
            Destroy(cultivationEffectInstance);
        }
    }
}
