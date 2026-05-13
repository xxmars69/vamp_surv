using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CelikenVP
{
    public class LineStatRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statNameText;
        [SerializeField] private TextMeshProUGUI statValueText;

        public void UpdateStatName(string statName)
        {
            statNameText.text = statName;
        }
        public void UpdateStatValue(float statValue, StackingMode stacking)
        {
            if (stacking == StackingMode.Multiplicative)
                statValueText.text = $"{statValue:0.#}%";
            if (stacking == StackingMode.MultiplicativeInt)
                statValueText.text = $"{statValue:0}%";
            if (stacking == StackingMode.Additive)
                statValueText.text = $"{statValue:0.#}";
            if (stacking == StackingMode.AdditiveInt)
                statValueText.text = $"{statValue:0}";
        }
    }
}
