using UnityEngine;

/// <summary>
/// Tracks tribute modifiers consumed during the current deity battle.
/// These modifiers persist through the battle and add to the capture chance
/// when attempting attunement.
/// </summary>
public class TributeModifierTracker : MonoBehaviour
{
    private static TributeModifierTracker _instance;
    public static TributeModifierTracker Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("TributeModifierTracker");
                _instance = go.AddComponent<TributeModifierTracker>();
            }
            return _instance;
        }
    }

    [SerializeField] private int _tributeModifierStacks = 0;
    [SerializeField] private float _modifierPerTribute = 0.10f; // 10% per tribute

    public int TributeStacks => _tributeModifierStacks;
    public float TotalModifier => _tributeModifierStacks * _modifierPerTribute;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    /// <summary>
    /// Adds one tribute stack and returns the new total modifier percentage.
    /// </summary>
    public float AddTributeStack()
    {
        _tributeModifierStacks++;
        Debug.Log($"TributeModifierTracker: Added tribute stack. Total: {_tributeModifierStacks} (Modifier: {TotalModifier * 100}%)");
        return TotalModifier;
    }

    /// <summary>
    /// Resets all tribute stacks. Call this at battle start or end.
    /// </summary>
    public void ResetStacks()
    {
        _tributeModifierStacks = 0;
        Debug.Log("TributeModifierTracker: Reset all tribute stacks.");
    }

    /// <summary>
    /// Sets the modifier percentage per tribute (default 10% = 0.10).
    /// </summary>
    public void SetModifierPerTribute(float modifier)
    {
        _modifierPerTribute = modifier;
    }
}
