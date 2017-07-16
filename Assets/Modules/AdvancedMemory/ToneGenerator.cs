using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ToneGenerator : MonoBehaviour {
    private float curTone, curVol, timeUntilTarget, targetTone, targetVolume;

    private float phase, phasePerUnit, timePerUnit;
    private static float rate;

    void Start() {
        if(rate == 0) rate = AudioSettings.outputSampleRate;
        timePerUnit = 1f / rate;
    }

    public void SetTone(float freq) {
        if(curVol == 0) curTone = freq;
        targetTone = freq;
        targetVolume = 0.025f;
        timeUntilTarget = 0.05f;
    }

    private void SetPhase(float freq) {
        phasePerUnit = freq / rate;
    }

    void OnAudioFilterRead(float[] data, int channels) {
        if(curVol <= 0 && targetVolume <= 0) return;

        int len = data.Length / channels;
        for(int a = 0; a < len; a++) {
            float x = curVol * Mathf.Sin(phase);
            phase += phasePerUnit;
            if(phase > Mathf.PI) phase -= Mathf.PI * 2;
            for(int b = 0; b < channels; b++) data[a * channels + b] += x;

            if(timeUntilTarget <= timePerUnit) {
                curVol = targetVolume;
                curTone = targetTone;
            }
            else {
                curVol = Mathf.Lerp(curVol, targetVolume, timePerUnit/timeUntilTarget);
                curTone = Mathf.Lerp(curTone, targetTone, timePerUnit/timeUntilTarget);
            }

            timeUntilTarget -= timePerUnit;
            if(timeUntilTarget <= 0) {
                timeUntilTarget = 2;
                targetVolume = 0;
            }
            SetPhase(curTone);
        }
    }
}