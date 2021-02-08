﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

// Teleports local player to the point after the countdown 
// ends. AutoMatcher uses this to boot players out of the rooms
// after the matching.
public class PrivateRoomTimer : UdonSharpBehaviour
{
    // root/parent of a bunch of places to teleport the player to after the countdown.
    public Transform teleportPointRoot;
    private Transform[] teleportPoints;

    public UnityEngine.UI.Text visual;
    public OccupantTracker currentRoom;
    public float countdown;
    public bool countdownActive = false;

    void Start()
    {
        teleportPoints = teleportPointRoot.GetComponentsInChildren<Transform>();
    }

    public void StartCountdown(float countdownSecs)
    {
        countdown = countdownSecs;
        countdownActive = true;
    }
    void Update()
    {
        if (countdownActive)
        {
            if ((countdown -= Time.deltaTime) < 0)
            {
                countdownActive = false;
                // only teleport if the player is still in the room.
                if (currentRoom != null && currentRoom.localPlayerOccupying)
                {
                    Debug.Log($"[PrivateRoomTimer] countdown over, teleporting player out");
                    TeleportOut();
                }
                else
                {
                    Debug.Log($"[PrivateRoomTimer] didn't teleport because player wasn't in room {currentRoom}");
                }
            }
        }
        visual.text = countdownActive ? $"{Mathf.RoundToInt(countdown)} seconds remaining..." : "";
    }

    public void TeleportOut()
    {
        // choose a random teleport point.
        var teleportPoint = teleportPoints[UnityEngine.Random.Range(0, teleportPoints.Length)];
        // avoid lerping the player to the teleport (apparently it does by default)
        Networking.LocalPlayer.TeleportTo(teleportPoint.position, teleportPoint.rotation,
            VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, lerpOnRemote: false);
    }
}
