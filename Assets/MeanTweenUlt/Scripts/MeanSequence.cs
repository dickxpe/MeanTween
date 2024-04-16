// Author: Peter Dickx https://github.com/dickxpe
// MIT License - Copyright (c) 2024 Peter Dickx

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UltEvents;
using UnityEngine;

namespace com.zebugames.meantween.ult
{
    [Serializable]
    public class StringEvent : UltEvent<string> { };

    [System.Serializable]
    public struct SequenceTween
    {
        public GameObject targetGameObject;

        public bool playSimultaneously;

        public List<MeanBehaviour> tweens;
    }

    public class MeanSequence : MonoBehaviour
    {
        [SerializeField]
        public bool playOnAwake = false;

        [SerializeField]
        public List<SequenceTween> sequence = new List<SequenceTween>();

        [SerializeField]
        public StringEvent onPlayNext;
        [SerializeField]
        public UltEvent onCompleted;

        public bool showEvents = false;

        List<MeanBehaviour> tweens = new List<MeanBehaviour>();

        bool trigger = false;

        void Awake()
        {
            for (int i = 0; i < sequence.Count; i++)
            {
                for (int j = 0; j < sequence[i].tweens.Count; j++)
                {
                    tweens.Add(sequence[i].tweens[j]);

                }
            }
        }

        void Start()
        {
            if (playOnAwake)
            {
                Play();
            }

        }

        public void Play()
        {
            StartCoroutine(PlaySequence());
        }

        public void Cancel()
        {
            foreach (SequenceTween sequenceTween in sequence)
            {
                LeanTween.cancel(sequenceTween.targetGameObject);
            }
        }

        private IEnumerator PlaySequence()
        {
            foreach (SequenceTween sequenceTween in sequence)
            {
                if (sequenceTween.playSimultaneously)
                {
                    MeanBehaviour longestTween = sequenceTween.tweens.OrderByDescending(x => x.totalDuration).First();
                    foreach (MeanBehaviour tween in sequenceTween.tweens)
                    {
                        if (tween != longestTween)
                        {
                            if (tween.infiniteLoop)
                            {
                                tween.Animate(true);
                            }
                            else
                            {
                                tween.Animate();
                            }
                        }
                    }
                    yield return WaitUntilEvent(longestTween, longestTween.onLoopsComplete);
                }
                else
                {
                    foreach (MeanBehaviour tween in sequenceTween.tweens.ToList())
                    {
                        yield return WaitUntilEvent(tween, tween.onLoopsComplete);
                    }
                }
            }
            onCompleted.Invoke();
        }

        private IEnumerator WaitUntilEvent(MeanBehaviour playNext, UltEvent unityEvent)
        {

            trigger = false;
            Action action = Trigger;
            unityEvent.AddPersistentCall(action);

            if (playNext.infiniteLoop)
            {
                playNext.Animate(true);
            }
            else
            {
                playNext.Animate();
            }
            onPlayNext.Invoke(playNext.objectToTween.name + " → " + playNext.tweenName);
            yield return new WaitUntil(() => trigger);
            unityEvent.RemovePersistentCall(action);
        }

        public void Trigger()
        {
            trigger = true;
        }
    }
}