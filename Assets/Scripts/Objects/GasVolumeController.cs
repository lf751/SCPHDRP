using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;
using UnityEngine.Rendering.HighDefinition;

public class GasVolumeController : MonoBehaviour
{
    public bool isActive;
    bool GasActive = false;
    public GameObject smokeDetector;
    public AudioSource smokeSound;
    public Volume gasVolume;
    [Header("Volumetric")]
    public LocalVolumetricFog targetRenderer;
    public float fadeDuration = 1f;

    // Update is called once per frame
    void Start()
    {
        if (smokeDetector.activeInHierarchy)
        {
            Switch(isActive);
            StartCoroutine(FadeAlpha());
        }
    }

    public void Switch(bool isActive)
    {
        isActive = smokeDetector.activeSelf;
        gasVolume.weight = 0f;
        if (isActive != GasActive)
        {
            if (gasVolume != null)
            {
                GasActive = isActive;
                gasVolume.weight = 0f;
                smokeSound.mute = !isActive;
                DOTween.Sequence()
                    .Append(DOTween.To(() => gasVolume.weight, x => gasVolume.weight = x, 1f, 1f))
                    .OnComplete(() =>
                    {

                    });
            }
        }

    }
    private IEnumerator FadeAlpha()
    {
        Material material;
        material = targetRenderer.parameters.materialMask;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeDuration;
            material.SetFloat("_Height_Thinning", 1f - normalizedTime);
            targetRenderer.parameters.meanFreePath = 10f - normalizedTime;
            yield return null;
        }

        // Ensure the final value is set correctly
        material.SetFloat("_Height_Thinning", 0f);
    }
}