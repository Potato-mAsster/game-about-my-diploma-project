using UnityEngine;
using TMPro;

public class LeafUI : MonoBehaviour
{
    public TextMeshProUGUI leafText;

    void Update()
    {
        leafText.text = LeafCollector.leafCount + "/3";
    }
}
