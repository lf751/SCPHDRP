using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class WorldDecal : MonoBehaviour
{
    private DecalProjector projector;
    public int h, v;
    // Start is called before the first frame update
    void Awake()
    {
        DecalSystem.instance.DecalStatic(transform.position, transform.rotation, transform.localScale.x, h, v);
        projector = this.gameObject.AddComponent<DecalProjector>();
        projector.material = DecalSystem.instance.DecalAtlas;
        projector.uvBias = new Vector2(0.33f * h, 0.25f * v);
        projector.uvScale = new Vector2(0.35f, 0.25f);
        projector.scaleMode = DecalScaleMode.InheritFromHierarchy;
    }
}
