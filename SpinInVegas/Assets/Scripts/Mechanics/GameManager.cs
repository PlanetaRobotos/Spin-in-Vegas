using System;
using System.Collections;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    public static event Action OnStartAd;
    
    [SerializeField] private MainUI ui;

    [SerializeField] private Transform wheel;
    [SerializeField] private CircleCollider2D arrowCollider;
    [SerializeField] private int countOfNumbers = 3;

    [SerializeField] private float speed;

    [Tooltip("How much times need to create a false match")] [SerializeField]
    private int nextFalseMatch = 2;

    [SerializeField] private float[] speedsForMatch;

    [Header("Particle Win Stuff")] [SerializeField]
    private GameObject winObjPrefab;

    [Header("Audio")] [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private AudioClip spinClip;
    [SerializeField] private AudioClip coinMinusClip;

    private AudioSource _audioSource;
    private int _buildIndex;
    private bool _canClick;
    private int _currentSpin;

    // private Dictionary<float, SlotType> _slots;
    // private SlotType[] _currentView;
    private int _readyToCheck;
    private int[] _rotAngles;
    private int _spinOffset;
    private int _startRot;
    private int[] numberRotValues;
    [SerializeField] private int adDelay=6;
    private int _counter;

    public static int[] Numbers { get; private set; }
    public static int CurrentNumber { get; private set; }


    private void Start()
    {
        CurrentNumber = 0;
        arrowCollider.enabled = false;
        Numbers = new int[countOfNumbers];
        _audioSource = GetComponent<AudioSource>();

        _canClick = true;
        _rotAngles = new[] {30, 60, 90, 120, 150, 180, 210, 240, 270, 300, 330, 360};


        nextFalseMatch = 1;
        // _counter = 1;
        _buildIndex = SceneManager.GetActiveScene().buildIndex;
        if (_buildIndex == 0)
            StartCoroutine(EndlessRotation());

        ClearNumbers();
        Rotate();
        _startRot = _rotAngles[Random.Range(0, _rotAngles.Length)];
    }

    public static event Action<int> OnSetNumber;

    private IEnumerator EndlessRotation()
    {
        while (gameObject.activeInHierarchy)
        {
            StartCoroutine(RotateWheel());

            _audioSource.PlayOneShot(coinMinusClip);
            _audioSource.PlayOneShot(spinClip);

            yield return new WaitForSeconds(13f);
            _readyToCheck = 0;
        }
    }

    /// <summary>
    ///     When button clicked
    /// </summary>
    public void Rotate()
    {
        if (!_canClick) return;

        arrowCollider.enabled = false;
        _audioSource.PlayOneShot(coinMinusClip);
        _audioSource.PlayOneShot(spinClip);

        // if (_currentSpin % nextFalseMatch == 0)
        //     _spinOffset = Random.Range(0, 7);

        // foreach (Transform row in rows)
        StartCoroutine(RotateWheel());

        if (CurrentNumber >= countOfNumbers - 1) StartCoroutine(AwaitReadyToCheckIe());

        _canClick = false;
    }

    /// <summary>
    ///     Work after rows stops - check match and instantiate win coins
    /// </summary>
    /// <returns></returns>
    private IEnumerator AwaitReadyToCheckIe()
    {
        yield return new WaitUntil(() => _readyToCheck >= countOfNumbers);

        _startRot = _rotAngles[Random.Range(0, _rotAngles.Length)];
        CurrentNumber = 0;
        _currentSpin++;
        _readyToCheck = 0;
        nextFalseMatch = Random.Range(2, 7);

        if (CheckNumbers())
        {
            int newScore = PlayerPrefs.GetInt("CurrentScore") + 1;
            PlayerPrefs.SetInt("CurrentScore", newScore);
            ui.UpdateScoreText();

            int rand = Random.Range(12, 30);
            float offset = 5f / rand;
            for (var i = 0; i < rand; i++)
            {
                float newXPos = -2.5f + offset * i;
                var newCoin = Instantiate(winObjPrefab, new Vector2(newXPos,
                    Random.Range(-3f, 4f)), Quaternion.identity);
                var rb = newCoin.GetComponent<Rigidbody2D>();
                rb.AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(2f, 5f)), ForceMode2D.Impulse);
            }

            Debug.Log("Win");
            _audioSource.PlayOneShot(winClip);
        }
        else
        {
            Debug.Log("Lose");
            _audioSource.PlayOneShot(loseClip);
        }

        yield return new WaitForSeconds(1f);
        ClearNumbers();

        _canClick = true;
    }

    /// <summary>
    ///     Main logic for rows (rotation, set slots type)
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private IEnumerator RotateWheel()
    {
        float currSpeed = speed;

        if (_currentSpin % nextFalseMatch == 0 && _buildIndex == 1)
        {
            wheel.eulerAngles = Vector3.forward * _startRot;
            currSpeed = speedsForMatch[Random.Range(0, speedsForMatch.Length)];
        }
        else
        {
            float offset = speed / 3;
            currSpeed += Random.Range(-offset, offset);
        }

        while (currSpeed >= 0.2f)
        {
            wheel.eulerAngles += Vector3.back * (currSpeed * Time.deltaTime);
            currSpeed -= Time.deltaTime * 30f;

            yield return null;
        }

        float mostNextTo = 100;
        var j = 0;
        for (var i = 0; i < _rotAngles.Length; i++)
        {
            float dist = Mathf.Abs(wheel.eulerAngles.z - _rotAngles[i]);
            if (dist < mostNextTo)
            {
                mostNextTo = dist;
                j = i;
            }
        }

        float angle = _rotAngles[j];

        while (Mathf.Abs(wheel.eulerAngles.z - angle) > 1f)
        {
            var rot = wheel.eulerAngles;
            rot = Vector3.Lerp(rot,
                Vector3.forward * angle, Time.deltaTime * 5f);
            wheel.eulerAngles = rot;
            yield return null;
        }

        wheel.eulerAngles = Vector3.forward * angle;
        arrowCollider.enabled = true;
        yield return new WaitForSeconds(0.2f);
        if (_buildIndex == 1)
        {
            OnSetNumber?.Invoke(CurrentNumber);
            CurrentNumber++;
            _readyToCheck++;
        }
        yield return new WaitForSeconds(0.4f);
        _canClick = true;
        _counter++;
        
        if (_counter % adDelay == 0 && _buildIndex == 1)
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log($"here");
            OnStartAd?.Invoke();
        }
    }

    private static bool CheckNumbers()
    {
        return Numbers.Count(t => Numbers[0] == t) >= Numbers.Length;
    }

    private void ClearNumbers()
    {
        for (var i = 0; i < countOfNumbers; i++)
        {
            Numbers[i] = 0;
            OnSetNumber?.Invoke(i);
        }
    }
}