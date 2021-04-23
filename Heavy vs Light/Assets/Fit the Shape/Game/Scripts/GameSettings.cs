using UnityEngine;
using Watermelon.Core;

[SetupTab("Settings", texture = "icon_preferences")]
[CreateAssetMenu(fileName = "Game Settings", menuName = "Settings/Game Settings")]
public class GameSettings : ScriptableObject
{
    public int basicGemsAmount = 50;

    public int adsReward;
    public int adsRewardMultiplier;
    public int baitReward;
    public int megaBaitReward;
    public int missedBaitAdReward;

    [Tooltip("Delay in seconds between interstitial appearings.")]
    public float interstitialShowingDelay = 30f;
    [Tooltip("Length of game over count down before skipping revive in seconds.")]
    public float reviveCountdownTime = 6f;
}