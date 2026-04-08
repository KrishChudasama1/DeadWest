using System.Collections;
using UnityEngine;

namespace StableLevel
{
    public class RaceTrackManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Drag the PhantomRider in this scene. If left empty, it will be found automatically.")]
        public PhantomRider phantomRider;

        [Tooltip("Where the player should spawn when this scene loads.")]
        public Transform playerSpawnPoint;

        [Header("Timing")]
        [Tooltip("Seconds after scene load before the rider activates.")]
        [SerializeField] private float riderActivateDelay = 3f;

        [Header("Duel Timer")]
        [Tooltip("Seconds the player has to defeat the rider before he charges.")]
        [SerializeField] private float duelTimeLimit = 15f;

        private bool riderDefeated = false;
        private float _timeRemaining;
        private bool _timerRunning = false;
        private bool _timedOut = false;

     
        private void Start()
        {
            PositionPlayer();

            if (phantomRider == null)
                phantomRider = FindObjectOfType<PhantomRider>(true);

            if (phantomRider == null)
            {
                Debug.LogWarning("RaceTrackManager: No PhantomRider found in the scene!");
                return;
            }

            phantomRider.OnRiderDefeated += HandleRiderDefeated;
            StartCoroutine(ActivateRiderAfterDelay());
        }

        private void OnDestroy()
        {
            if (phantomRider != null)
                phantomRider.OnRiderDefeated -= HandleRiderDefeated;
        }

        private void Update()
        {
            if (!_timerRunning) return;

            _timeRemaining -= Time.deltaTime;

            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                _timerRunning = false;
                _timedOut = true;
                OnTimerExpired();
            }
        }

       
        private void OnGUI()
        {
            if (!_timerRunning && !_timedOut) return;

            // Timer label
            float displayTime = Mathf.Max(0f, _timeRemaining);
            string timerText = _timedOut ? "TIME'S UP!" : $"{displayTime:F1}s";

            // Style
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 36;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.UpperCenter;

            
            if (_timedOut)
                style.normal.textColor = Color.red;
            else if (displayTime <= 10f)
                style.normal.textColor = Color.Lerp(Color.red, Color.yellow, displayTime / 10f);
            else
                style.normal.textColor = Color.white;

            Rect rect = new Rect(0, Screen.height - 60 - 20, Screen.width, 60);
            GUI.Label(rect, timerText, style);
        }

        
        private void PositionPlayer()
        {
            if (playerSpawnPoint == null) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("RaceTrackManager: No GameObject tagged 'Player' found.");
                return;
            }

            player.transform.position = playerSpawnPoint.position;
            Debug.Log($"RaceTrackManager: Player positioned at {playerSpawnPoint.position}");

            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
                pm.SetMovementLocked(false);
        }

    

        private IEnumerator ActivateRiderAfterDelay()
        {
            Debug.Log($"RaceTrackManager: Rider activating in {riderActivateDelay}s...");
            yield return new WaitForSeconds(riderActivateDelay);

            if (phantomRider != null)
            {
                phantomRider.Activate();
                Debug.Log("RaceTrackManager: Phantom Rider activated — fight!");

                // Start the duel countdown
                _timeRemaining = duelTimeLimit;
                _timerRunning = true;
                Debug.Log($"RaceTrackManager: Duel timer started — {duelTimeLimit}s to defeat the rider!");
            }
        }

       

        private void HandleRiderDefeated()
        {
            if (riderDefeated) return;
            riderDefeated = true;

            
            _timerRunning = false;

            Debug.Log("RaceTrackManager: Phantom Rider defeated — advancing to duel phase.");
          
        }

        private void OnTimerExpired()
        {
            if (riderDefeated) return;

            Debug.Log("RaceTrackManager: TIME'S UP — rider charges the player!");

            if (phantomRider != null)
                phantomRider.ChargeAndKillPlayer();
        }
    }
}
