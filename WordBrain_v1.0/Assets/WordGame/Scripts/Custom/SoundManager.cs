using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    private static SoundManager _soundManager;
    public static SoundManager Instance()
    {
        return _soundManager;
    }

    private AudioSource _audio;

    private AudioClip _audioClipLetter;
    private AudioClip _audioClipWin;

    AudioSource _source0;
    AudioSource _source1;

    bool _isFirst = true; //is _source0 currently the active AudioSource (plays some sound right now)

    Coroutine _zerothSourceFadeRoutine = null;
    Coroutine _firstSourceFadeRoutine = null;

    void Awake()
    {
        _soundManager = GetComponent<SoundManager>();

        //re-connect _soruce0 and _source1 to the ones in attachedSources[]
        Component[] attachedSources = gameObject.GetComponents(typeof(AudioSource));
        //For some reason, unity doesn't accept "as AudioSource[]" casting. We would get
        //'null' array instead if we would attempt. Need to re-create a new array:
        AudioSource[] sources = attachedSources.Select(c => c as AudioSource).ToArray();

        InitSources(sources);
    }
    
    public void PlayLetterSound()
    {
        if (_audioClipLetter == null)
        {
            _audioClipLetter = Resources.Load("Sounds/keyboard", typeof(AudioClip)) as AudioClip;
        }
        
        CrossFade(_audioClipLetter, 0.5f, 0.01f, 0f);
    }

    public void PlayWinSound()
    {
        if (_audioClipWin == null)
        {
            _audioClipWin = Resources.Load("Sounds/gmae", typeof(AudioClip)) as AudioClip;
        }

        if (_audio == null)
            _audio = transform.gameObject.AddComponent<AudioSource>();
        _audio.PlayOneShot(_audioClipWin);
    }
    
    //re-establishes references to audio sources on this game object:
    void InitSources(AudioSource[] audioSources)
    {

        if (ReferenceEquals(audioSources, null) || audioSources.Length == 0)
        {
            _source0 = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
            _source1 = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
            //DefaultTheSource(_source0);
            // DefaultTheSource(_source1);  //remove? we do this in editor only
            return;
        }

        switch (audioSources.Length)
        {
            case 1:
            {
                _source0 = audioSources[0];
                _source1 = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
                //DefaultTheSource(_source1);  //TODO remove?  we do this in editor only
            }
                break;
            default:
            {
                //2 and more
                _source0 = audioSources[0];
                _source1 = audioSources[1];
            }
                break;
        } //end switch
    }

    //gradually shifts the sound comming from our audio sources to the this clip:
    // maxVolume should be in 0-to-1 range
    public void CrossFade(AudioClip playMe,
        float maxVolume,
        float fadingTime,
        float delay_before_crossFade = 0)
    {

        var fadeRoutine = StartCoroutine(Fade(playMe,
            maxVolume,
            fadingTime,
            delay_before_crossFade));
    }//end CrossFade()



    IEnumerator Fade(AudioClip playMe,
        float maxVolume,
        float fadingTime,
        float delay_before_crossFade = 0)
    {


        if (delay_before_crossFade > 0)
        {
            yield return new WaitForSeconds(delay_before_crossFade);
        }

        if (_isFirst)
        { // _source0 is currently playing the most recent AudioClip
            //so launch on source1
            
            _source1.clip = playMe;
            _source1.Play();
            _source1.volume = 0;

            if (_firstSourceFadeRoutine != null)
            {
                StopCoroutine(_firstSourceFadeRoutine);
            }
            _firstSourceFadeRoutine = StartCoroutine(fadeSource(_source1,
                _source1.volume,
                maxVolume,
                fadingTime));
            if (_zerothSourceFadeRoutine != null)
            {
                StopCoroutine(_zerothSourceFadeRoutine);
            }
            _zerothSourceFadeRoutine = StartCoroutine(fadeSource(_source0,
                _source0.volume,
                0,
                fadingTime));
            _isFirst = false;

            yield break;
        }

        //otherwise, _source1 is currently active, so play on _source0
        _source0.clip = playMe;
        _source0.Play();
        _source0.volume = 0;

        if (_zerothSourceFadeRoutine != null)
        {
            StopCoroutine(_zerothSourceFadeRoutine);
        }
        _zerothSourceFadeRoutine = StartCoroutine(fadeSource(_source0,
            _source0.volume,
            maxVolume,
            fadingTime));

        if (_firstSourceFadeRoutine != null)
        {
            StopCoroutine(_firstSourceFadeRoutine);
        }
        _firstSourceFadeRoutine = StartCoroutine(fadeSource(_source1,
            _source1.volume,
            0,
            fadingTime));
        _isFirst = true;
    }



    IEnumerator fadeSource(AudioSource sourceToFade, float startVolume, float endVolume, float duration)
    {
        float startTime = Time.time;

        while (true)
        {
            float elapsed = Time.time - startTime;

            sourceToFade.volume = Mathf.Clamp01(Mathf.Lerp(startVolume,
                endVolume,
                elapsed / duration));

            if (sourceToFade.volume == endVolume)
            {
                break;
            }
            yield return null;
        }//end while
    }


    //returns false if BOTH sources are not playing and there are no sounds are staged to be played.
    //also returns false if one of the sources is not yet initialized
    public bool isPlaying
    {
        get
        {
            if (_source0 == null || _source1 == null)
            {
                return false;
            }

            //otherwise, both sources are initialized. See if any is playing:
            if (_source0.isPlaying || _source1.isPlaying)
            {
                return true;
            }

            //none is playing:
            return false;
        }//end get
    }

    public bool bothPlaying()
    {
        if (_source0 != null && _source1 != null && _source0.isPlaying && _source1.isPlaying)
        {
            return true;
        }
        return false;
    }
}
