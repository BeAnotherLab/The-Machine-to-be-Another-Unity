using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class IPText : MonoBehaviour
{
    [SerializeField] private Text _IPAddressText;
    
    // Start is called before the first frame update
    void Start()
    {
        _IPAddressText.text = "Local IP Adress : " + GetLocalIPAddress();
    }
    
    public static string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}
