using UnityEngine;
using System;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    [Header("Gameplay")]
    public bool infiniteHealth = false;
    [Range(0.25f, 5f)] public float damageMultiplier = 1f;

    public event Action OnChanged;

    const string KeyInf = "opt_infiniteHealth";
    const string KeyDmg = "opt_damageMultiplier";

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void SetInfiniteHealth(bool v) { infiniteHealth = v; Save(); OnChanged?.Invoke(); }
    public void SetDamageMultiplier(float v) { damageMultiplier = Mathf.Clamp(v, 0.25f, 5f); Save(); OnChanged?.Invoke(); }

    public void Save()
    {
        PlayerPrefs.SetInt(KeyInf, infiniteHealth ? 1 : 0);
        PlayerPrefs.SetFloat(KeyDmg, damageMultiplier);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        infiniteHealth = PlayerPrefs.GetInt(KeyInf, 0) == 1;
        damageMultiplier = PlayerPrefs.GetFloat(KeyDmg, 1f);
        OnChanged?.Invoke();
    }

    public bool IsInvincible => infiniteHealth;
    public float DamageMult => damageMultiplier;
}
