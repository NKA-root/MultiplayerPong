using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetIp : MonoBehaviour
{
    public void SetIP(string ip)
    {
        PlayerPrefs.SetString("IP", ip.Contains('.') ? ip : "127.0.0.1" );
    }
}
