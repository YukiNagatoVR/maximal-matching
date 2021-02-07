﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MatchingTrackerUi : UdonSharpBehaviour
{
    public GameObject ToggleRoot;
    public MatchingTracker MatchingTracker;

    private UnityEngine.UI.Toggle[] toggles;
    private bool[] lastSeenToggle;
    private string[] activePlayerLastUpdate;
    private UnityEngine.UI.Text[] texts;

    private float updateCooldown = 0f;

    void Start()
    {
        toggles = ToggleRoot.GetComponentsInChildren<UnityEngine.UI.Toggle>(includeInactive: true);
        lastSeenToggle = new bool[toggles.Length];
        activePlayerLastUpdate = new string[toggles.Length];
        texts = ToggleRoot.GetComponentsInChildren<UnityEngine.UI.Text>(includeInactive: true);
    }
    private void Update()
    {
        if ((updateCooldown -= Time.deltaTime) > 0) return;
        updateCooldown = 1f;

        VRCPlayerApi[] players = MatchingTracker.GetOrderedPlayers();
        var playerCount = players.Length;
        int i;
        for (i = 0; i < playerCount; i++)
        {
            VRCPlayerApi p = players[i];
            // skip ourselves
            if (Networking.LocalPlayer == p)
            {
                toggles[i].gameObject.SetActive(false);
                activePlayerLastUpdate[i] = null;
                continue;
            }
            toggles[i].gameObject.SetActive(true);
            var wasMatchedWith = MatchingTracker.GetLocallyMatchedWith(p);
            if (wasMatchedWith)
            {
                texts[i].text = MatchingTracker.GetDisplayName(p);
                var seconds = Time.time - MatchingTracker.GetLastMatchedWith(p);
                var minutes = seconds / 60f;
                var hours = minutes / 60f;
                texts[i].text = $"{MatchingTracker.GetDisplayName(p)} " +
                    (hours > 0 ? $"({Mathf.FloorToInt(hours):D2}:{Mathf.FloorToInt(minutes):D2} ago)" :
                    minutes > 1 ? $"({Mathf.FloorToInt(minutes):D2} seconds ago)" :
                    $"({Mathf.FloorToInt(seconds):D2} seconds ago)");
            }
            else
            {
                texts[i].text = MatchingTracker.GetDisplayName(p);
            }
            if (activePlayerLastUpdate[i] == MatchingTracker.GetDisplayName(p))
            {
                // if player changed state in ui (doesn't match our internal state)
                if (toggles[i].isOn != lastSeenToggle[i])
                {
                    MatchingTracker.SetLocallyMatchedWith(p, toggles[i].isOn);
                } else
                {
                    // set UI from tracker state
                    toggles[i].isOn = wasMatchedWith;
                }
                lastSeenToggle[i] = toggles[i].isOn;

            } else
            {
                // wasn't the same player before
                activePlayerLastUpdate[i] = MatchingTracker.GetDisplayName(p);
                // set the UI state ignoring what it was
                toggles[i].isOn = wasMatchedWith;
                lastSeenToggle[i] = wasMatchedWith;
            }
        }
        for (; i < toggles.Length; i++)
        {
            toggles[i].gameObject.SetActive(false);
            activePlayerLastUpdate[i] = null;
        }
    }
}
