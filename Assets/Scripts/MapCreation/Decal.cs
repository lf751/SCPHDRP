using UnityEngine;
using Pixelplacement;
using UnityEngine.Rendering.HighDefinition;

public class Decal : MonoBehaviour
{
    public float Duration;
    public bool Instant;
    public float Scale;
    public Vector3 rotation;
    public int h, v;
    public Vector3 position;
    public Material DecalAtlas;
    GameObject plane;
    DecalProjector projector;

    void Awake()
    {
        // Create the plane GameObject
        plane = new GameObject("DecalPlane");
        projector = plane.AddComponent<DecalProjector>();
        plane.transform.position = transform.position;
        plane.transform.parent = transform;
    }

    public void SetDecal()
    {
        projector.scaleMode = DecalScaleMode.InheritFromHierarchy;
        transform.position = position;

        // Set rotation
        plane.transform.rotation = Quaternion.Euler(rotation);

        if (!Instant)
        {
            projector.size = Vector3.zero;
            plane.transform.localScale = Vector3.zero;
            Tween.LocalScale(plane.transform, new Vector3(Scale, Scale, Scale), Duration, 0, Tween.EaseOut);
        }
        else
        {
            transform.localScale = new Vector3(Scale, Scale, Scale);
        }
        // Apply UV offset and tiling if needed
        projector.uvBias = new Vector2(0.33f * h, 1 - 0.25f * v);
        projector.uvScale = new Vector2(0.35f, 0.25f);

        // Assign material
        projector.material = DecalAtlas;
    }
}
