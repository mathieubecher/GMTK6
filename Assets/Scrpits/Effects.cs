using Cinemachine;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Effects : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;

    private CinemachineBasicMultiChannelPerlin m_noise;

    private void Start()
    {
        m_noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void ScreenShake(float _intensity, float _sustain, float _release)
    {
        StartCoroutine(ScreenShakeCoroutine(_intensity, _sustain, _release));
    }
    public IEnumerator ScreenShakeCoroutine(float _intensity, float _sustain, float _release)
    {
        float elapsed = 0f;

        while (elapsed < _sustain)
        {
            yield return new WaitForEndOfFrame();
            elapsed += Time.deltaTime * Time.timeScale;
            m_noise.m_AmplitudeGain = _intensity / math.remap(Time.timeScale, 0.01f, 1f, 0.1f, 1f);
            m_noise.m_FrequencyGain = _intensity / math.remap(Time.timeScale, 0.01f, 1f, 0.1f, 1f);
        }

        elapsed = 0f;

        while (elapsed < _release)
        {
            yield return new WaitForEndOfFrame();
            elapsed += Time.deltaTime * Time.timeScale;

            m_noise.m_AmplitudeGain = _intensity * (1 - elapsed / _release) / Time.timeScale;
            m_noise.m_FrequencyGain = _intensity * (1 - elapsed / _release) / Time.timeScale;
        }
        m_noise.m_AmplitudeGain = 0.0f;
        m_noise.m_FrequencyGain = 0.0f;

        yield return null;
    }

    public void FreezeTime(float _duration)
    {
        StartCoroutine(FreezeTimeCoroutine(_duration));
    }
    
    public IEnumerator FreezeTimeCoroutine(float _duration)
    {
        Time.timeScale = 0.0001f;
        yield return new WaitForSecondsRealtime(_duration);
        Time.timeScale = 1.0f;
        yield return null;
    }
}
