using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateBand : MonoBehaviour
{
    private void OnEnable()
    {
        CanvasManager.Instance.tradDropdown.value = ClientCommands.Instance.defaultLanguage;
    }
}
