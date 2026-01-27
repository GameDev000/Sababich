using TMPro;
using UnityEngine;

public class UsernameDirectionFinal : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;

    private int lastLength;
    private bool inNumberRun;

    private void Reset() => input = GetComponent<TMP_InputField>();

    private void OnEnable()
    {
        input.onValueChanged.AddListener(OnTextChanged);
        lastLength = input.text.Length;
        inNumberRun = false;
    }

    private void OnDisable()
    {
        input.onValueChanged.RemoveListener(OnTextChanged);
    }

    private void OnTextChanged(string text)
    {
        // Decide direction by the FIRST LETTER only.
        // If there is NO letter yet (only digits/punctuation), keep LTR so numbers are typed naturally LTR.
        Direction dir = DetectDirectionByFirstLetter(text);
        bool finalRTL = (dir == Direction.Hebrew); // Unknown -> LTR

        // Apply direction + alignment
        input.textComponent.isRightToLeftText = finalRTL;
        input.textComponent.alignment = finalRTL
            ? TextAlignmentOptions.MidlineRight
            : TextAlignmentOptions.MidlineLeft;

        if (input.placeholder is TMP_Text ph)
        {
            ph.isRightToLeftText = finalRTL;
            ph.alignment = finalRTL
                ? TextAlignmentOptions.MidlineRight
                : TextAlignmentOptions.MidlineLeft;
        }

        // Caret behavior: only relevant in RTL mode
        if (finalRTL && text.Length > lastLength)
        {
            int caret = input.caretPosition;
            char inserted = (caret > 0 && caret - 1 < text.Length)
                ? text[caret - 1]
                : '\0';

            if (char.IsDigit(inserted))
            {
                // While typing digits in RTL mode:
                // step caret left so digits are typed left-to-right (e.g., 123 stays 123)
                input.caretPosition = Mathf.Max(0, caret - 1);
                inNumberRun = true;
            }
            else
            {
                // Leaving a digit run -> return typing to the visual LEFT edge of the whole string
                // (best supported in older TMP via MoveTextEnd)
                if (inNumberRun)
                {
                    input.MoveTextEnd(false);

                    // Your TMP version does not have caretSelectPosition, so set caretPosition only
                    input.caretPosition = text.Length;
                }

                inNumberRun = false;
            }
        }

        // Reset if deleting characters
        if (text.Length < lastLength)
        {
            inNumberRun = false;
        }

        lastLength = text.Length;
    }

    // ---------------- Direction detection ----------------

    private enum Direction { Unknown, Hebrew, English }

    private Direction DetectDirectionByFirstLetter(string s)
    {
        foreach (char ch in s)
        {
            // Ignore spaces and punctuation/symbols until we find a LETTER
            if (char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsSymbol(ch))
                continue;

            // Digits do not decide direction
            if (char.IsDigit(ch))
                continue;

            if (IsHebrew(ch)) return Direction.Hebrew;
            if (IsEnglish(ch)) return Direction.English;
        }

        // No letters yet -> keep LTR by default
        return Direction.Unknown;
    }

    private bool IsHebrew(char ch) => ch >= '\u0590' && ch <= '\u05FF';

    private bool IsEnglish(char ch) =>
        (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');
}
