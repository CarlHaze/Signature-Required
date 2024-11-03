using UnityEngine;
using UnityEngine.UIElements;

public class SpeedoController : MonoBehaviour
{
    public CarControl car;
    private Label speedLabel;

    void Start()
    {
        VisualElement root = gameObject.GetComponent<UIDocument>().rootVisualElement;
        root.Q<Speedo>().dataSource = car;

        // Add the speed label to update text with the current speed
        speedLabel = root.Q<Label>("CurrentSpeedLabel");
    }

    // Update is called once per frame
    void Update()
    {
        // Update the label text with the current speed from the car
        if (speedLabel != null && car != null)
        {
            speedLabel.text = $"Speed: {Mathf.RoundToInt(car.GetCurrentSpeed())} km/h";
        }
    }
}
