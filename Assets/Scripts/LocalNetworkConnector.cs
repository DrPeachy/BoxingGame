using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FishNet;
using System.Net;

public class LocalNetworkConnector : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Text localIPText;

    // client connection
    public void ConnectToServer(){
        string ip = inputField.text.Trim();
        Debug.Log($"Connecting to {ip}");
        InstanceFinder.ClientManager.StartConnection(ip, 7770);
    }

    // server connection
    public void StartServer(){
        InstanceFinder.ServerManager.StartConnection();
        localIPText.text = GetLocalIPAddress();
        // connect self to server
        InstanceFinder.ClientManager.StartConnection(localIPText.text, 7770);
    }

    public static string GetLocalIPAddress()
    {
        string localIP = "127.0.0.1"; // Default fallback (localhost)

        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) // IPv4 only
            {
                localIP = ip.ToString();
                break;
            }
        }
        Debug.Log($"Local IP Address: {localIP}");
        return localIP;
    }
}
