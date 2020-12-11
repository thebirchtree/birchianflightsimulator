using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableTag : MonoBehaviour
{
    public GameObject respawnPrefab;
    public GameObject[] respawns;
    void Start()
    {
        if (respawns == null)
            respawns = GameObject.FindGameObjectsWithTag("");

        foreach (GameObject respawn in respawns)
        {
            Instantiate(respawnPrefab, respawn.transform.position, respawn.transform.rotation);
        }
    }
}