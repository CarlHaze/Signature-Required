using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI speedText;

    public void UpdateSpeed(float speed)
    {
        if (speedText != null)
        {
            int currentSpeed = Mathf.RoundToInt(speed);
            speedText.text = "Speed: " + currentSpeed.ToString();
        }
    }
}
