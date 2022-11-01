using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    TextMeshProUGUI ArrowText => GetComponentInChildren<TextMeshProUGUI>();
    private void OnEnable() => StartCoroutine(Animation());
    IEnumerator Animation()
    {
        ArrowText.enabled = true;

        while (true)
        {
            yield return new WaitForSecondsRealtime(.666f);

            ArrowText.enabled = !ArrowText.enabled;
        }
    }
}
