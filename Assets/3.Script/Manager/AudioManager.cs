using UnityEngine;
using UnityEngine.SceneManagement;


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("효과음 재생용 AudioSource")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxdefaultSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource objSource;

    [Header("BGM 목록")]
    // BGM 목록
    public AudioClip currentBGM;
    public AudioClip titleBGM;
    public AudioClip trainingBGM;

    [Header("Default SFX_Player 목록")]
    // 기본 플레이어 SFX
    public AudioClip RunSFX;
    public AudioClip climbingSFX;
    public AudioClip slidingSFX;
    public AudioClip ceilingSFX;

    [Header("SFX_Player 목록")]
    // 플레이어 SFX
    public AudioClip jumpSFX;
    public AudioClip shootSFX;
    public AudioClip accelSFX;
    public AudioClip windSFX;
    public AudioClip dashSFX;
    public AudioClip chargeAttackSFX;
    public AudioClip excDashSFX;
    public AudioClip anchorSFX;

    [Header("SFX_Obj 목록")]
    // 오브젝트 SFX
    public AudioClip gateOpenSFX;
    public AudioClip gateWarningSFX;
    public AudioClip goalAppearSFX;
    public AudioClip UISFX;

    private void Awake()
    {
        // 이미 인스턴스가 있으면 자기 자신 파괴
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // 싱글톤 설정
        instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log($"{SceneManager.GetActiveScene().name} : AudioManager를 새로 생성했습니다.");
    }

    public void InitializeData()
    {
        if(SceneLoader.nextSceneName == "Title")
        {
            currentBGM = titleBGM;
        }
        else if (SceneLoader.nextSceneName == "TrainingRoom01" || SceneLoader.nextSceneName == "TrainingRoom02")
        {
            currentBGM = trainingBGM;
        }
        else
        {
            currentBGM = null;
        }

        bgmSource.Stop();
        Debug.Log("AudioManager를 초기화 완료했습니다.");
    }

    // BGM 재생
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null)
        {
            bgmSource.Stop();
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // SFX 재생
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }

    // defult SFX 재생
    public void PlayDefaultSFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxdefaultSource.PlayOneShot(clip);
    }

    public void StopPlayDefaultSFX()
    {
        sfxdefaultSource.Stop();
    }

    // SFX_Obj 재생
    public void PlaySFX_Obj(AudioClip clip)
    {
        if (clip == null) return;

        objSource.PlayOneShot(clip);
    }
}
