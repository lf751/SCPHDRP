using UnityEngine;

public class SegmentedSlider : MonoBehaviour
{
    [SerializeField] private GameObject[] Segments;

    public float[] SegmentIndices =
    {
        5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100
    };

    public void UpdateSlider(int Value)
    {
        for (int i = 0; i < SegmentIndices.Length; i++)
        {
            if (Value < SegmentIndices[i])
            {
                Segments[i].SetActive(false);
            }
            else Segments[i].SetActive(true);
        }
    }
}
