using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioRenderer : MonoBehaviour
{
    [Header("Move Sounds")]
    public AudioClip MovePieceSound;
    public AudioClip CaptureSound;
    public AudioClip CastleSound;
    public AudioClip CheckSound;
    public AudioClip PromoteSound;
    public AudioClip CheckMateSound;
    [Header("Game Sounds")]
    public AudioClip VictorySound;
    public AudioClip DefeatSound;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayMoveSound(MoveData moveData)
    {
        AudioClip soundToPLay = moveData.MoveSpeciality switch
        {
            MoveSpeciality.isCapture => CaptureSound,
            MoveSpeciality.isCheck => CheckSound,
            MoveSpeciality.isCastling => CastleSound,
            _ => MovePieceSound
        };

        audioSource.PlayOneShot(soundToPLay);
    }

    public void PlayEndGameSound(bool isMyTurn)
    {
        if (isMyTurn)
            audioSource.PlayOneShot(CheckMateSound);
        else
            audioSource.PlayOneShot(DefeatSound);
    }
}
