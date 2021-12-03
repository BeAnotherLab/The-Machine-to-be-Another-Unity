using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum UserStatus { headsetOff, headsetOn, readyToStart } 

[Serializable]
public class UserStates
{
    public UserStatus selfStatus;
    public UserStatus otherStatus;
    public bool dataCollectionConsent;
}
