using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Serializer
{
    public class Dispatcher : MonoBehaviour
    {
        struct ActionCustom
        {
            public Action act;
            public float time;
        }
        static Dispatcher instance;
        Queue<ActionCustom> actions = new Queue<ActionCustom>();
        [SerializeField] int maxCmdPerFrame = 500;
        [SerializeField] int current;
        public static Dispatcher Instance
        {
            get
            {
                if (instance == null)
                    Create();
                return instance;
            }
        }
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
        public void ExecuteInMainThread(Action act, float time = 0)
        {
            //Debug.Log("ExecuteInMainThread");
            lock (actions)
                actions.Enqueue(new ActionCustom { act = act, time = time });
        }

        // Update is called once per frame
        void Update()
        {
            while (actions.Count > 0 && maxCmdPerFrame > current)
            {
                current++;
                ActionCustom cmd;
                lock (actions)
                {
                    cmd = actions.Dequeue();
                    cmd.time -= Time.deltaTime;
                    if (cmd.time > 0)
                    {
                        actions.Enqueue(cmd);
                        cmd = default;
                    }
                }
                cmd.act?.Invoke();
            }
            current = 0;

        }

        public static void Create()
        {
            if (instance == null)
                instance = new GameObject("Dispatcher").AddComponent<Dispatcher>();
        }
    }
}