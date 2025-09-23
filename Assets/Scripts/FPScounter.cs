using UnityEngine;

public class FPScounter : MonoBehaviour
{
    public int avgFrameRate;

    public void Update ()
    {
        float current = 0;
        current = (int)(1f / Time.unscaledDeltaTime);
        avgFrameRate = (int)current;
        Debug.Log(avgFrameRate.ToString() + " FPS");
    }
}
