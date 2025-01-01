using UnityEngine;

public class Lantern_Swing : MonoBehaviour
{ 
    [SerializeField] private float swingAmplitude = 30f; // Maximum swing angle in degrees
    [SerializeField] private float swingSpeed = 2f; // Speed of the swinging motion
    [SerializeField] private float dampingFactor = 0.98f; // Factor to gradually reduce the swing

    private float swingTime;
    private float currentAmplitude;
    private bool isSwinging;

    void Update()
    {
        if (isSwinging)
        {
            // Increment the time for the swinging motion
            swingTime += Time.deltaTime * swingSpeed;

            // Calculate the swing angle using a sine wave and current amplitude
            float angle = currentAmplitude * Mathf.Sin(swingTime);

            // Apply the rotation to the lantern
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // Gradually reduce the amplitude
            currentAmplitude *= dampingFactor;

            // Stop swinging if the amplitude is very small
            if (currentAmplitude < 0.1f)
            {
                isSwinging = false;
                currentAmplitude = 0f;
            }
        }
    }

    // Call this function to start the swing
    public void SwingLantern()
    {
        // Reset and randomize the swing
        swingTime = Random.Range(0f, Mathf.PI * 2f);
        currentAmplitude = swingAmplitude;
        isSwinging = true;
    }
}
