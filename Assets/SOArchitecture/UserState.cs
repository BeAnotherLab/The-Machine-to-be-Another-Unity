﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum UserState { headsetOff, headsetOn, readyToConsent, readyToStart, questionnaire }

public static class UserStateOperations
{
    public static bool IsBeforeConsent(UserState state)
    {
        return state == UserState.headsetOff ||
               state == UserState.headsetOn ||
               state == UserState.readyToConsent;
    }
}