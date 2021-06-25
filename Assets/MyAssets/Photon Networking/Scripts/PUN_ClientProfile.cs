using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Profile", menuName = "MyNetWork/ClientProfile")]
public class PUN_ClientProfile : ScriptableObject
{
    //Determinate if the client can create room or just join one
    public bool CanCreateRoom = true;

    public string NickName = "";

    public string Password = "12345";
}
