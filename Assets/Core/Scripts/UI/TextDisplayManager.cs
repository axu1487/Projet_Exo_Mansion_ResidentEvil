using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using NaughtyAttributes;

public enum ReadingSpeed
{
    Slow,     // Lente (~12 Caractères/sec)
    Standard, // Standard (~16 Caractères/sec)
    Fast,     // Rapide (~25 Caractères/sec)
    VeryFast  // Très rapide (~40 Caractères/sec)
}

public class TextDisplayManager : MonoBehaviour
{
    public static TextDisplayManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private GameObject textPanel;

    [ShowIf("requireClickToAdvance")]
    [SerializeField] private GameObject continuePrompt;

    [Header("Game Pause Settings")]
    [Tooltip("Fige le temps du jeu (Time.timeScale = 0) pendant l'affichage du dialogue.")]
    [SerializeField] private bool pauseGameDuringDialogue = true;

    [Header("Typewriter Effect")]
    [Tooltip("Active l'effet de frappe machine à écrire (lettre par lettre).")]
    [SerializeField] private bool useTypewriter = true;

    [ShowIf("useTypewriter")]
    [Tooltip("Si vrai, la vitesse de frappe s'adapte automatiquement à la vitesse de lecture choisie (ReadingSpeed).")]
    [SerializeField] private bool syncTypewriterWithReadingSpeed = true;

    [ShowIf("ShowManualTimeBetweenCharacters")]
    [Tooltip("Délai manuel en secondes entre chaque caractère (utilisé uniquement si la synchronisation est décochée).")]
    [SerializeField] private float manualTimeBetweenCharacters = 0.03f;

    [Header("Text Duration")]
    [Tooltip("Permet de forcer un calcul automatique de la durée d'affichage basé sur la vitesse de lecture.")]
    [SerializeField] private bool useAutomaticCalculation = true;

    [HideIf("useAutomaticCalculation")]
    [SerializeField] private float defaultDuration = 3f;

    [ShowIf("ShowReadingSpeed")]
    [Tooltip("Vitesse de lecture appliquée (impacte la durée et la vitesse de frappe si synchronisée).")]
    [SerializeField] private ReadingSpeed readingSpeed = ReadingSpeed.Standard;

    [ShowIf("useAutomaticCalculation")]
    [Tooltip("Si vrai, ajoute du temps d'affichage supplémentaire pour les ponctuations (., !? ;).")]
    [SerializeField] private bool includePunctuationPauses = true;

    [Header("Maximum Lines Per Panel")]
    [Tooltip("Nombre maximal de lignes affichées par bulle de texte.")]
    [SerializeField] private int maxLines = 4;

    [Header("Input System (New Input System)")]
    [Tooltip("Si vrai, attend un clic/validation pour passer la bulle et bloque le joueur. Si faux, le texte défile au chrono et le joueur reste libre.")]
    [SerializeField] private bool requireClickToAdvance = true;

    [ShowIf("requireClickToAdvance")]
    [Tooltip("Le composant PlayerInput situé sur votre Joueur.")]
    [SerializeField] private PlayerInput playerInputComponent;

    [ShowIf("requireClickToAdvance")]
    [SerializeField] private string playerMapName = "Player";

    [ShowIf("requireClickToAdvance")]
    [SerializeField] private string uiMapName = "UI";

    private DynamicMaxWidth dynamicMaxWidth;
    private Coroutine displayCoroutine;
    private InputAction advanceAction;

    // --- MÉTHODES DE CONDITION POUR NAUGHTYATTRIBUTES ---
    private bool ShowManualTimeBetweenCharacters() => useTypewriter && !syncTypewriterWithReadingSpeed;
    private bool ShowReadingSpeed() => useAutomaticCalculation || (useTypewriter && syncTypewriterWithReadingSpeed);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (textComponent != null)
        {
            dynamicMaxWidth = textComponent.GetComponent<DynamicMaxWidth>();
        }

        HidePanel();
    }

    private void Start()
    {
        if (playerInputComponent == null)
            playerInputComponent = FindFirstObjectByType<PlayerInput>();

        if (playerInputComponent != null)
        {
            advanceAction = playerInputComponent.actions.FindAction("Click");
            if (advanceAction == null)
                advanceAction = playerInputComponent.actions.FindAction("Submit");
        }
    }

    public void ShowMessage(string message)
    {
        ShowMessageWithDuration(message, defaultDuration);
    }

    public void ShowMessageWithDuration(string message, float duration)
    {
        if (textComponent == null) return;

        if (displayCoroutine != null) 
            StopCoroutine(displayCoroutine);

        displayCoroutine = StartCoroutine(DisplaySequence(message, duration));
    }

    private IEnumerator DisplaySequence(string fullText, float durationPerBubble)
    {
        // Fige le jeu en arrière-plan si activé
        if (pauseGameDuringDialogue)
        {
            Time.timeScale = 0f;
        }

        if (requireClickToAdvance)
        {
            EnableUIInputMap(true);
        }

        if (textPanel != null)
            textPanel.SetActive(true);

        string remainingText = fullText;

        while (!string.IsNullOrEmpty(remainingText))
        {
            // Masque la flèche pendant le calcul du layout et l'animation de frappe
            if (continuePrompt != null)
                continuePrompt.SetActive(false);

            textComponent.text = remainingText;
            ForceLayoutCalculation();

            string currentBubbleText;

            // Découpage propre sur un espace si le texte dépasse le nombre de lignes max
            if (textComponent.textInfo.lineCount > maxLines)
            {
                int lastCharIndex = textComponent.textInfo.lineInfo[maxLines - 1].lastCharacterIndex;
                int searchStartIndex = Mathf.Clamp(lastCharIndex - 4, 0, remainingText.Length - 1);
                int spaceIndex = remainingText.LastIndexOf(' ', searchStartIndex);
                int cutIndex = (spaceIndex > 0) ? spaceIndex : searchStartIndex;

                currentBubbleText = remainingText.Substring(0, cutIndex).TrimEnd() + " ...";
                remainingText = "... " + remainingText.Substring(cutIndex).TrimStart();

                textComponent.text = currentBubbleText;
                ForceLayoutCalculation();
            }
            else
            {
                currentBubbleText = remainingText;
                remainingText = string.Empty;
            }

            // --- CORRECTION DU CLIGNOTEMENT (FLICKER) ---
            // On réinitialise à 0 caractères visibles AVANT la frame d'attente
            if (useTypewriter)
            {
                textComponent.maxVisibleCharacters = 0;
            }

            yield return null;

            // --- ANIMATION TYPEWRITER (MACHINE À ÉCRIRE) ---
            if (useTypewriter)
            {
                int totalVisibleCharacters = currentBubbleText.Length;
                float charDelay = GetTimeBetweenCharacters();

                for (int i = 1; i <= totalVisibleCharacters; i++)
                {
                    textComponent.maxVisibleCharacters = i;

                    // Si le joueur clique pendant la frappe, on affiche instantanément toute la bulle
                    if (requireClickToAdvance && advanceAction != null && advanceAction.triggered)
                    {
                        textComponent.maxVisibleCharacters = totalVisibleCharacters;
                        yield return null; // Consomme le clic de frappe
                        break;
                    }

                    // Utilisation de Realtime pour ne pas être impacté par Time.timeScale = 0
                    yield return new WaitForSecondsRealtime(charDelay);
                }

                textComponent.maxVisibleCharacters = totalVisibleCharacters;
            }
            else
            {
                textComponent.maxVisibleCharacters = int.MaxValue;
            }

            // --- ATTENTE DE VALIDATION / DUREE ---
            if (requireClickToAdvance && advanceAction != null)
            {
                // La flèche ne s'active QUE s'il reste une bulle suivante
                bool hasMoreText = !string.IsNullOrEmpty(remainingText);
                if (continuePrompt != null) 
                    continuePrompt.SetActive(hasMoreText);

                yield return new WaitUntil(() => advanceAction.triggered);

                if (continuePrompt != null) 
                    continuePrompt.SetActive(false);
            }
            else
            {
                float waitTime = useAutomaticCalculation ? CalculateReadingTime(currentBubbleText) : durationPerBubble;
                // Utilisation de Realtime pour ne pas être impacté par Time.timeScale = 0
                yield return new WaitForSecondsRealtime(waitTime);
            }
        }

        HidePanel();
    }

    private void ForceLayoutCalculation()
    {
        if (dynamicMaxWidth == null && textComponent != null)
            dynamicMaxWidth = textComponent.GetComponent<DynamicMaxWidth>();

        if (dynamicMaxWidth != null)
            dynamicMaxWidth.RecalculateWidth();

        if (textPanel != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(textPanel.GetComponent<RectTransform>());

        Canvas.ForceUpdateCanvases();
        textComponent.ForceMeshUpdate();
    }

    public void HidePanel()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }

        if (textComponent != null)
        {
            textComponent.text = string.Empty;
            textComponent.maxVisibleCharacters = int.MaxValue;
        }

        if (textPanel != null)
            textPanel.SetActive(false);

        if (continuePrompt != null) 
            continuePrompt.SetActive(false);

        EnableUIInputMap(false);

        // Rétablit l'écoulement normal du temps quand le dialogue se ferme
        Time.timeScale = 1f;
    }

    private void EnableUIInputMap(bool enableUI)
    {
        if (playerInputComponent == null) return;

        string targetMap = enableUI ? uiMapName : playerMapName;
        playerInputComponent.SwitchCurrentActionMap(targetMap);
    }

    public float GetTimeBetweenCharacters()
    {
        if (!syncTypewriterWithReadingSpeed)
            return manualTimeBetweenCharacters;

        switch (readingSpeed)
        {
            case ReadingSpeed.Slow:
                return 0.06f;   // ~16 caractères/sec

            case ReadingSpeed.Fast:
                return 0.025f;  // ~40 caractères/sec

            case ReadingSpeed.VeryFast:
                return 0.012f;  // ~80 caractères/sec

            case ReadingSpeed.Standard:
            default:
                return 0.04f;   // ~25 caractères/sec
        }
    }

    public float CalculateReadingTime(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0f;

        float baseDuration;
        float timePerChar;
        float sentencePause; 
        float commaPause;    

        switch (readingSpeed)
        {
            case ReadingSpeed.Slow:
                baseDuration = 1.5f;
                timePerChar = 0.08f;
                sentencePause = 0.5f;
                commaPause = 0.3f;
                break;

            case ReadingSpeed.Fast:
                baseDuration = 0.8f;
                timePerChar = 0.04f;
                sentencePause = 0.25f;
                commaPause = 0.15f;
                break;

            case ReadingSpeed.VeryFast:
                baseDuration = 0.4f;
                timePerChar = 0.025f;
                sentencePause = 0.12f;
                commaPause = 0.06f;
                break;

            case ReadingSpeed.Standard:
            default:
                baseDuration = 1.0f;
                timePerChar = 0.06f;
                sentencePause = 0.4f;
                commaPause = 0.2f;
                break;
        }

        float calculatedDuration = baseDuration + (text.Length * timePerChar);

        if (includePunctuationPauses)
        {
            foreach (char c in text)
            {
                if (c == '.' || c == '!' || c == '?')
                    calculatedDuration += sentencePause;
                else if (c == ',' || c == ';')
                    calculatedDuration += commaPause;
            }
        }

        return calculatedDuration;
    }
}