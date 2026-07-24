using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the boss portrait's face sprite. Its only responsibility is swapping
/// the face image according to a <see cref="BossExpression"/> value. The frame
/// image never changes and is not managed by this script.
/// </summary>
public class BossPortraitUI : MonoBehaviour
{
    /// <summary>
    /// Inspector-friendly pairing of an expression with its corresponding sprite.
    /// Using an array of these instead of one field per expression keeps the
    /// class open for extension: new expressions only require new enum values
    /// and new array entries, never new code.
    /// </summary>
    [Serializable]
    private struct ExpressionSprite
    {
        [SerializeField] private BossExpression expression;
        [SerializeField] private Sprite sprite;

        public BossExpression Expression => expression;
        public Sprite Sprite => sprite;
    }

    [SerializeField] private Image faceImage;
    [SerializeField] private ExpressionSprite[] expressionSprites;

    private Dictionary<BossExpression, Sprite> spriteLookup;

    /// <summary>The expression currently being displayed.</summary>
    public BossExpression CurrentExpression { get; private set; } = BossExpression.Neutral;

    private void Awake()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        spriteLookup = new Dictionary<BossExpression, Sprite>(expressionSprites.Length);

        foreach (ExpressionSprite entry in expressionSprites)
        {
            if (!spriteLookup.ContainsKey(entry.Expression))
            {
                spriteLookup.Add(entry.Expression, entry.Sprite);
            }
        }
    }

    /// <summary>
    /// Swaps the portrait's face sprite to match the given expression.
    /// </summary>
    /// <param name="expression">The expression to display.</param>
    public void SetExpression(BossExpression expression)
    {
        if (faceImage == null)
        {
            Debug.LogWarning("[BossPortraitUI] Face image reference is missing.");
            return;
        }

        if (spriteLookup == null)
        {
            BuildLookup();
        }

        if (spriteLookup.TryGetValue(expression, out Sprite sprite) && sprite != null)
        {
            faceImage.sprite = sprite;
            CurrentExpression = expression;
        }
        else
        {
            Debug.LogWarning($"[BossPortraitUI] No sprite assigned for expression '{expression}'.");
        }
    }
}