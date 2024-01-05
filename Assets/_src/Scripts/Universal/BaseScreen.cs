using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BaseScreen : MonoBehaviour
{
    
        [SerializeField] GameObject root;

        public virtual void SetActive(bool v)
        {
            root.SetActive(v);
        }

        public bool ActiveSelf()
        {
            return root.activeSelf;
        }
    private void Reset()
    {
        root = gameObject;
    }
}
