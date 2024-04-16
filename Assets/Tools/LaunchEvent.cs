// Author: Peter Dickx https://github.com/dickxpe
// MIT License - Copyright (c) 2024 Peter Dickx
using UltEvents;
using UnityEngine;
using UnityEngine.Events;

namespace com.zebugames.meantween.unity
{

    public class LaunchEvent : MonoBehaviour
    {
        [SerializeField]
        UltEvent launch;
        public void Launch()
        {
            launch.Invoke();
        }
    }

}