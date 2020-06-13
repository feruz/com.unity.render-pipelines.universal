using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

// Big thank you to @thomasedw and @MD_Reptile on the Unity forums for making this
// https://forum.unity.com/threads/the-new-2d-lighting-on-tilemaps-advice-and-suggestions.810927/
// https://forum.unity.com/threads/shadow-caster-2d-not-working-on-tilemap.793803/

public class TilemapShadows : MonoBehaviour {
    public static TilemapShadows Instance;

    [SerializeField] private bool selfShadows = default;

    private CompositeCollider2D tilemapCollider;
    private GameObject shadowCasterContainer;
    private List<GameObject> shadowCasters = new List<GameObject>(), toDelete = new List<GameObject>();
    private List<PolygonCollider2D> shadowPolygons = new List<PolygonCollider2D>();
    private List<ShadowCaster2D> shadowCasterComponents = new List<ShadowCaster2D>();

    private bool doReset = false, doCleanup = false;

    public void Start() {
        Instance = this;
        tilemapCollider = GetComponent<CompositeCollider2D>();

        shadowCasterContainer = new GameObject("Shadow Casters");
        Vector3 lP = shadowCasterContainer.transform.localPosition;
        shadowCasterContainer.transform.parent = gameObject.transform;
        shadowCasterContainer.transform.localPosition = lP;
        for (int i = 0; i < tilemapCollider.pathCount; i++) {
            Vector2[] pathVertices = new Vector2[tilemapCollider.GetPathPointCount(i)];
            tilemapCollider.GetPath(i, pathVertices);
            GameObject shadowCaster = new GameObject("shadow_caster_" + i);
            shadowCasters.Add(shadowCaster);
            PolygonCollider2D shadowPolygon = (PolygonCollider2D)shadowCaster.AddComponent(typeof(PolygonCollider2D));
            shadowPolygons.Add(shadowPolygon);
            lP = shadowCaster.transform.localPosition;
            shadowCaster.transform.parent = shadowCasterContainer.transform;
            shadowCaster.transform.localPosition = lP;
            shadowPolygon.points = pathVertices;
            shadowPolygon.enabled = false;
            //if (shadowCaster.GetComponent<ShadowCaster2D>() != null) // remove existing caster?
            //    Destroy(shadowCaster.GetComponent<ShadowCaster2D>());
            ShadowCaster2D shadowCasterComponent = shadowCaster.AddComponent<ShadowCaster2D>();
            shadowCasterComponents.Add(shadowCasterComponent);
            shadowCasterComponent.selfShadows = selfShadows;
        }
    }

    private void Reset() {
        toDelete = new List<GameObject>(shadowCasters);
        shadowCasters.Clear();
        shadowPolygons.Clear();
        shadowCasterComponents.Clear();

        for (int i = 0; i < tilemapCollider.pathCount; i++) {
            Vector2[] pathVertices = new Vector2[tilemapCollider.GetPathPointCount(i)];
            tilemapCollider.GetPath(i, pathVertices);
            GameObject shadowCaster = new GameObject("shadow_caster_" + i);
            shadowCasters.Add(shadowCaster);
            PolygonCollider2D shadowPolygon = (PolygonCollider2D)shadowCaster.AddComponent(typeof(PolygonCollider2D));
            shadowPolygons.Add(shadowPolygon);
            Vector3 lP = shadowCaster.transform.localPosition;
            shadowCaster.transform.parent = shadowCasterContainer.transform;
            shadowCaster.transform.localPosition = lP;
            shadowPolygon.points = pathVertices;
            shadowPolygon.enabled = false;
            //if (shadowCaster.GetComponent<ShadowCaster2D>() != null) // remove existing caster?
            //    Destroy(shadowCaster.GetComponent<ShadowCaster2D>());
            ShadowCaster2D shadowCasterComponent = shadowCaster.AddComponent<ShadowCaster2D>();
            shadowCasterComponents.Add(shadowCasterComponent);
            shadowCasterComponent.selfShadows = selfShadows;
        }
        doCleanup = true;
    }

    private void LateUpdate() {
        if (doReset) {
            Reset();
            doReset = false;
        }
        if (doCleanup) {
            StartCoroutine(Cleanup());
            doCleanup = false;
        }
    }

    IEnumerator Cleanup() {
        yield return null;
        for (int i = 0; i < toDelete.Count; i++) {
            Destroy(toDelete[i]);
        }
        toDelete.Clear();
    }

    public void UpdateShadows() {
        doReset = true;
    }
}