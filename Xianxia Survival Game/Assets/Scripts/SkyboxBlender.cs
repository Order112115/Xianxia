using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxBlender : MonoBehaviour
{
    public Light sun;
    public Material blendedSkyboxMaterial;
    public Material morningSkybox;
    public Material noonSkybox;
    public Material eveningSkybox;
    public Material nightSkybox;
    public Material rainSkybox;
    public Material snowSkybox;

    public float dayDuration = 1200f; // Duration of a day in seconds (20 minutes)
    public float weatherChangeInterval = 60f; // Interval between weather changes in seconds
    public float weatherBlendDuration = 5f; // Duration for blending weather skyboxes

    public GameObject rainPrefab; // Reference to the rain particle system prefab
    public GameObject snowPrefab; // Reference to the snow particle system prefab

    public float terrainWidth = 1000f; // Width of the terrain
    public float terrainHeight = 1000f; // Height of the terrain
    public float particleSpacing = 50f; // Spacing between particle systems

    private float time; // Initialize in Start method
    private float sunInitialIntensity;
    private float weatherTimer = 0f;
    private float weatherBlendTimer = 0f;
    private enum WeatherType { Sunny, Rainy, Snowy }
    private WeatherType currentWeather;
    private WeatherType previousWeather;

    private GameObject[,] rainParticleSystems;
    private GameObject[,] snowParticleSystems;
    private bool isBlending = false;

    void Start()
    {
        // Set initial time to noon
        time = 600f;
        sunInitialIntensity = sun.intensity;
        ChangeWeather();

        GameObject weatherParent = new GameObject("Weather");

        // Instantiate the rain particle systems across the terrain
        if (rainPrefab != null)
        {
            int numRainSystemsX = Mathf.CeilToInt(terrainWidth / particleSpacing);
            int numRainSystemsZ = Mathf.CeilToInt(terrainHeight / particleSpacing);
            rainParticleSystems = new GameObject[numRainSystemsX, numRainSystemsZ];

            for (int x = 0; x < numRainSystemsX; x++)
            {
                for (int z = 0; z < numRainSystemsZ; z++)
                {
                    Vector3 position = new Vector3(x * particleSpacing, 50, z * particleSpacing);
                    rainParticleSystems[x, z] = Instantiate(rainPrefab, position, Quaternion.identity, weatherParent.transform);
                    rainParticleSystems[x, z].SetActive(false); // Start with rain turned off
                }
            }
        }

        // Instantiate the snow particle systems across the terrain
        if (snowPrefab != null)
        {
            int numSnowSystemsX = Mathf.CeilToInt(terrainWidth / particleSpacing);
            int numSnowSystemsZ = Mathf.CeilToInt(terrainHeight / particleSpacing);
            snowParticleSystems = new GameObject[numSnowSystemsX, numSnowSystemsZ];

            for (int x = 0; x < numSnowSystemsX; x++)
            {
                for (int z = 0; z < numSnowSystemsZ; z++)
                {
                    Vector3 position = new Vector3(x * particleSpacing, 50, z * particleSpacing);
                    snowParticleSystems[x, z] = Instantiate(snowPrefab, position, Quaternion.identity, weatherParent.transform);
                    snowParticleSystems[x, z].SetActive(false); // Start with snow turned off
                }
            }
        }

        // Update initial skybox and intensity
        AdjustIntensity();
        UpdateSkybox();

        // Debug the initial time
        Debug.Log($"Initial Time: {time}");
    }

    void Update()
    {
        time += Time.deltaTime;

        // Loop the time of day
        if (time >= dayDuration)
        {
            time = 0f;
        }

        float angle = (time / dayDuration) * 360f;
        sun.transform.rotation = Quaternion.Euler(new Vector3(angle - 90, 170, 0));
        AdjustIntensity();
        UpdateSkybox();

        // Handle weather change
        weatherTimer += Time.deltaTime;
        if (weatherTimer >= weatherChangeInterval)
        {
            weatherTimer = 0f;
            previousWeather = currentWeather;
            ChangeWeather();
            isBlending = true;
            weatherBlendTimer = 0f;
        }

        // Handle weather blending
        if (isBlending)
        {
            weatherBlendTimer += Time.deltaTime;
            if (weatherBlendTimer >= weatherBlendDuration)
            {
                weatherBlendTimer = weatherBlendDuration;
                isBlending = false;
            }
        }
    }

    void AdjustIntensity()
    {
        float intensityMultiplier = 1;
        if (time <= 300f || time >= 900f) // Dawn and dusk
        {
            intensityMultiplier = Mathf.Clamp01(1 - ((Mathf.Abs(time - 300f) * 2 / 600f) % 1));
        }
        sun.intensity = sunInitialIntensity * intensityMultiplier;
    }

    void UpdateSkybox()
    {
        Material fromSkybox = null;
        Material toSkybox = null;
        float blendFactor = 0f;

        if (isBlending)
        {
            fromSkybox = GetSkyboxForWeather(previousWeather);
            toSkybox = GetSkyboxForWeather(currentWeather);
            blendFactor = Mathf.SmoothStep(0f, 1f, weatherBlendTimer / weatherBlendDuration);
        }
        else
        {
            if (currentWeather == WeatherType.Sunny)
            {
                if (time < 300f)
                {
                    fromSkybox = nightSkybox;
                    toSkybox = morningSkybox;
                    blendFactor = Mathf.SmoothStep(0f, 1f, time / 300f);
                }
                else if (time < 600f)
                {
                    fromSkybox = morningSkybox;
                    toSkybox = noonSkybox;
                    blendFactor = Mathf.SmoothStep(0f, 1f, (time - 300f) / 300f);
                }
                else if (time < 900f)
                {
                    fromSkybox = noonSkybox;
                    toSkybox = eveningSkybox;
                    blendFactor = Mathf.SmoothStep(0f, 1f, (time - 600f) / 300f);
                }
                else
                {
                    fromSkybox = eveningSkybox;
                    toSkybox = nightSkybox;
                    blendFactor = Mathf.SmoothStep(0f, 1f, (time - 900f) / 300f);
                }
            }
            else if (currentWeather == WeatherType.Rainy)
            {
                fromSkybox = rainSkybox;
                toSkybox = rainSkybox;
                blendFactor = 1f;
            }
            else if (currentWeather == WeatherType.Snowy)
            {
                fromSkybox = snowSkybox;
                toSkybox = snowSkybox;
                blendFactor = 1f;
            }
        }

        // Set the textures on the blended skybox material
        blendedSkyboxMaterial.SetTexture("_Skybox1", fromSkybox.GetTexture("_Tex"));
        blendedSkyboxMaterial.SetTexture("_Skybox2", toSkybox.GetTexture("_Tex"));
        blendedSkyboxMaterial.SetFloat("_BlendFactor", blendFactor);

        // Set the blended skybox material as the current skybox
        RenderSettings.skybox = blendedSkyboxMaterial;

        // Update the ambient light based on the time of day
        DynamicGI.UpdateEnvironment();

        // Debugging information
        Debug.Log($"Time of Day: {time}");
        Debug.Log($"From Skybox: {fromSkybox.name}, To Skybox: {toSkybox.name}");
        Debug.Log($"Blend Factor: {blendFactor}");
        Debug.Log($"Current Weather: {currentWeather}");
    }

    Material GetSkyboxForWeather(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Sunny:
                if (time < 300f) return morningSkybox;
                else if (time < 600f) return noonSkybox;
                else if (time < 900f) return eveningSkybox;
                else return nightSkybox;
            case WeatherType.Rainy:
                return rainSkybox;
            case WeatherType.Snowy:
                return snowSkybox;
            default:
                return morningSkybox;
        }
    }

    void ChangeWeather()
    {
        int randomValue = Random.Range(0, 7); // Random value between 0 and 6

        if (randomValue == 0) // 1 in 7 chance for rain
        {
            currentWeather = WeatherType.Rainy;
        }
        else if (randomValue == 1) // 1 in 7 chance for snow
        {
            currentWeather = WeatherType.Snowy;
        }
        else // The rest is sunny
        {
            currentWeather = WeatherType.Sunny;
        }

        UpdateWeatherEffects();
    }

    void UpdateWeatherEffects()
    {
        // Deactivate all rain particle systems
        if (rainParticleSystems != null)
        {
            foreach (GameObject rainSystem in rainParticleSystems)
            {
                rainSystem.SetActive(false);
            }
        }

        // Deactivate all snow particle systems
        if (snowParticleSystems != null)
        {
            foreach (GameObject snowSystem in snowParticleSystems)
            {
                snowSystem.SetActive(false);
            }
        }

        switch (currentWeather)
        {
            case WeatherType.Sunny:
                break;
            case WeatherType.Rainy:
                // Activate all rain particle systems
                if (rainParticleSystems != null)
                {
                    foreach (GameObject rainSystem in rainParticleSystems)
                    {
                        rainSystem.SetActive(true);
                    }
                }
                break;
            case WeatherType.Snowy:
                // Activate all snow particle systems
                if (snowParticleSystems != null)
                {
                    foreach (GameObject snowSystem in snowParticleSystems)
                    {
                        snowSystem.SetActive(true);
                    }
                }
                break;
        }

        // Debugging information
        Debug.Log($"Current Weather: {currentWeather}");
    }

    // Public method to trigger rain
    public void MakeItRain()
    {
        previousWeather = currentWeather;
        currentWeather = WeatherType.Rainy;
        UpdateWeatherEffects();
        isBlending = true;
        weatherBlendTimer = 0f; // Reset the weather change timer
    }

    // Public method to trigger snow
    public void MakeItSnow()
    {
        previousWeather = currentWeather;
        currentWeather = WeatherType.Snowy;
        UpdateWeatherEffects();
        isBlending = true;
        weatherBlendTimer = 0f; // Reset the weather change timer
    }
}
