using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;

/// <summary>
/// Unity Editor Tool: Probe Group Generator
/// Generates a Light Probe Group and/or Reflection Probes surrounding
/// the bounds of the currently selected geometry.
///
/// Place this file inside any Editor/ folder in your project.
/// Open via:  Tools ▶ Probe Group Generator
/// </summary>
public class ProbeGroupGenerator : EditorWindow
{
    // ─────────────────────────────────────────────────────────────────────────
    //  Tab state
    // ─────────────────────────────────────────────────────────────────────────

    private enum Tab { LightProbes, ReflectionProbes, Both }
    private Tab activeTab = Tab.LightProbes;
    private readonly string[] tabLabels = { "Light Probes", "Reflection Probes", "Both" };

    // ─────────────────────────────────────────────────────────────────────────
    //  Shared / selection state
    // ─────────────────────────────────────────────────────────────────────────

    private Bounds selectionBounds;
    private bool   boundsValid   = false;
    private string statusMessage = "Select one or more GameObjects, then click Generate.";
    private Vector2 scrollPos;

    // ─────────────────────────────────────────────────────────────────────────
    //  Light Probe parameters
    // ─────────────────────────────────────────────────────────────────────────

    private float lp_padding       = 0.5f;
    private int   lp_subdivisionsX = 2;
    private int   lp_subdivisionsY = 2;
    private int   lp_subdivisionsZ = 2;
    private bool  lp_addOuterShell = false;
    private float lp_outerOffset   = 1.0f;

    // ─────────────────────────────────────────────────────────────────────────
    //  Reflection Probe parameters
    // ─────────────────────────────────────────────────────────────────────────

    private float  rp_padding        = 1.0f;
    private int    rp_countX         = 1;
    private int    rp_countY         = 1;
    private int    rp_countZ         = 1;
    private bool   rp_autoBoxProject = true;
    private bool   rp_overlapBoxes   = false;   // when true boxes overlap; when false they tile exactly
    private float  rp_blendDistance  = 1.0f;

    // Capture settings
    private ReflectionProbeMode       rp_mode       = ReflectionProbeMode.Baked;
    private ReflectionProbeRefreshMode rp_refresh   = ReflectionProbeRefreshMode.OnAwake;
    private int                        rp_resolution = 128;
    private bool                       rp_hdr        = true;
    private float                      rp_nearClip   = 0.3f;
    private float                      rp_farClip    = 1000f;
    private float                      rp_intensity  = 1f;
    private int                        rp_importance = 1;

    private static readonly int[] ResolutionOptions = { 16, 32, 64, 128, 256, 512, 1024 };

    // ─────────────────────────────────────────────────────────────────────────
    //  Menu entry
    // ─────────────────────────────────────────────────────────────────────────

    [MenuItem("Tools/Probe Group Generator")]
    public static void ShowWindow()
    {
        var w = GetWindow<ProbeGroupGenerator>("Probe Group Generator");
        w.minSize = new Vector2(340, 540);
        w.Show();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  OnGUI
    // ─────────────────────────────────────────────────────────────────────────

    private void OnGUI()
    {
        GUILayout.Space(8);
        EditorGUILayout.LabelField("Probe Group Generator", EditorStyles.boldLabel);
        DrawDivider();

        // ── Selection ───────────────────────────────────────────────────────
        DrawSelectionSection();
        DrawDivider();

        // ── Tab bar ─────────────────────────────────────────────────────────
        activeTab = (Tab)GUILayout.Toolbar((int)activeTab, tabLabels);
        GUILayout.Space(6);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        switch (activeTab)
        {
            case Tab.LightProbes:      DrawLightProbeSection();      break;
            case Tab.ReflectionProbes: DrawReflectionProbeSection(); break;
            case Tab.Both:
                DrawLightProbeSection();
                DrawDivider();
                DrawReflectionProbeSection();
                break;
        }

        EditorGUILayout.EndScrollView();

        DrawDivider();

        // ── Generate button(s) ──────────────────────────────────────────────
        GUI.enabled = boundsValid;

        switch (activeTab)
        {
            case Tab.LightProbes:
                if (GUILayout.Button("Generate Light Probe Group", GUILayout.Height(36)))
                    GenerateLightProbeGroup();
                break;

            case Tab.ReflectionProbes:
                if (GUILayout.Button("Generate Reflection Probes", GUILayout.Height(36)))
                    GenerateReflectionProbes();
                break;

            case Tab.Both:
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Generate Light Probes", GUILayout.Height(36)))
                    GenerateLightProbeGroup();
                if (GUILayout.Button("Generate Reflection Probes", GUILayout.Height(36)))
                    GenerateReflectionProbes();
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("Generate Both", GUILayout.Height(36)))
                {
                    GenerateLightProbeGroup();
                    GenerateReflectionProbes();
                }
                break;
        }

        GUI.enabled = true;

        GUILayout.Space(6);
        EditorGUILayout.HelpBox(statusMessage, MessageType.None);
        GUILayout.Space(4);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  GUI Sections
    // ─────────────────────────────────────────────────────────────────────────

    private void DrawSelectionSection()
    {
        EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel);
        GUILayout.Space(4);

        if (GUILayout.Button("Refresh Selection Bounds", GUILayout.Height(26)))
            RefreshBounds();

        if (boundsValid)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Vector3Field("Center", selectionBounds.center);
            EditorGUILayout.Vector3Field("Size",   selectionBounds.size);
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            EditorGUILayout.HelpBox("No valid selection. Select GameObjects with Renderers.", MessageType.Info);
        }
    }

    // ── Light Probe Section ───────────────────────────────────────────────────

    private void DrawLightProbeSection()
    {
        EditorGUILayout.LabelField("Light Probe Parameters", EditorStyles.boldLabel);
        GUILayout.Space(4);

        lp_padding = Mathf.Max(0f, EditorGUILayout.FloatField(
            new GUIContent("Bounds Padding", "Extra space outside the selection bounds on all sides (world units)."),
            lp_padding));

        GUILayout.Space(4);
        EditorGUILayout.LabelField("Interior Grid Subdivisions", EditorStyles.miniBoldLabel);
        lp_subdivisionsX = EditorGUILayout.IntSlider(new GUIContent("Subdivisions X", "Extra columns along X."), lp_subdivisionsX, 0, 8);
        lp_subdivisionsY = EditorGUILayout.IntSlider(new GUIContent("Subdivisions Y", "Extra rows along Y."),    lp_subdivisionsY, 0, 8);
        lp_subdivisionsZ = EditorGUILayout.IntSlider(new GUIContent("Subdivisions Z", "Extra slices along Z."),  lp_subdivisionsZ, 0, 8);

        GUILayout.Space(6);
        lp_addOuterShell = EditorGUILayout.Toggle(new GUIContent("Add Outer Shell",
            "Place an additional face-only ring of probes further outside the padded bounds."), lp_addOuterShell);
        if (lp_addOuterShell)
        {
            lp_outerOffset = Mathf.Max(0.01f, EditorGUILayout.FloatField(
                new GUIContent("  Outer Shell Offset", "Distance beyond padded bounds for the outer shell (world units)."),
                lp_outerOffset));
        }

        GUILayout.Space(4);
        int est = EstimateLightProbeCount();
        EditorGUILayout.LabelField($"Estimated Light Probe Count: {est}", EditorStyles.centeredGreyMiniLabel);
    }

    // ── Reflection Probe Section ──────────────────────────────────────────────

    private void DrawReflectionProbeSection()
    {
        EditorGUILayout.LabelField("Reflection Probe Parameters", EditorStyles.boldLabel);
        GUILayout.Space(4);

        // ── Placement ──────────────────────────────────────────────────────
        EditorGUILayout.LabelField("Placement", EditorStyles.miniBoldLabel);

        rp_padding = Mathf.Max(0f, EditorGUILayout.FloatField(
            new GUIContent("Bounds Padding", "Extra space outside the selection bounds on all sides (world units)."),
            rp_padding));

        rp_countX = EditorGUILayout.IntSlider(new GUIContent("Count X", "Number of reflection probes along X."), rp_countX, 1, 8);
        rp_countY = EditorGUILayout.IntSlider(new GUIContent("Count Y", "Number of reflection probes along Y."), rp_countY, 1, 8);
        rp_countZ = EditorGUILayout.IntSlider(new GUIContent("Count Z", "Number of reflection probes along Z."), rp_countZ, 1, 8);

        GUILayout.Space(4);

        // ── Box Projection ─────────────────────────────────────────────────
        EditorGUILayout.LabelField("Box Projection", EditorStyles.miniBoldLabel);

        rp_autoBoxProject = EditorGUILayout.Toggle(new GUIContent("Auto Box Projection",
            "Enables box projection on each probe with a box sized to its grid cell."), rp_autoBoxProject);

        if (rp_autoBoxProject)
        {
            rp_overlapBoxes = EditorGUILayout.Toggle(new GUIContent("  Overlap Boxes",
                "When enabled, each probe's box extends to the full padded bounds instead of its individual cell."), rp_overlapBoxes);

            rp_blendDistance = Mathf.Max(0f, EditorGUILayout.FloatField(
                new GUIContent("  Blend Distance", "Blend distance on each probe box face (world units)."),
                rp_blendDistance));
        }

        GUILayout.Space(4);

        // ── Capture Settings ───────────────────────────────────────────────
        EditorGUILayout.LabelField("Capture Settings", EditorStyles.miniBoldLabel);

        rp_mode = (ReflectionProbeMode)EditorGUILayout.EnumPopup(
            new GUIContent("Mode", "Baked = offline bake; Realtime = live update; Custom = manual cubemap."),
            rp_mode);

        if (rp_mode == ReflectionProbeMode.Realtime)
        {
            rp_refresh = (ReflectionProbeRefreshMode)EditorGUILayout.EnumPopup(
                new GUIContent("Refresh Mode", "When realtime probes re-capture the scene."), rp_refresh);
        }

        // Resolution popup
        int curResIdx = Mathf.Max(0, System.Array.IndexOf(ResolutionOptions, rp_resolution));
        string[] resLabels = System.Array.ConvertAll(ResolutionOptions, r => r.ToString());
        curResIdx   = EditorGUILayout.Popup(new GUIContent("Resolution", "Cubemap resolution per probe."), curResIdx, resLabels);
        rp_resolution = ResolutionOptions[curResIdx];

        rp_hdr       = EditorGUILayout.Toggle(new GUIContent("HDR", "Use HDR cubemap format."), rp_hdr);
        rp_intensity = EditorGUILayout.Slider(new GUIContent("Intensity", "Reflection intensity multiplier."), rp_intensity, 0f, 2f);
        rp_importance = EditorGUILayout.IntField(new GUIContent("Importance", "Higher value = takes priority when probes overlap."), rp_importance);

        GUILayout.Space(4);
        EditorGUILayout.LabelField("Clip Planes", EditorStyles.miniBoldLabel);
        rp_nearClip = Mathf.Max(0.01f, EditorGUILayout.FloatField(new GUIContent("Near Clip", "Near clip plane for capture."), rp_nearClip));
        rp_farClip  = Mathf.Max(rp_nearClip + 0.1f, EditorGUILayout.FloatField(new GUIContent("Far Clip", "Far clip plane for capture."), rp_farClip));

        GUILayout.Space(4);
        int rpCount = rp_countX * rp_countY * rp_countZ;
        EditorGUILayout.LabelField($"Reflection Probes to Create: {rpCount}", EditorStyles.centeredGreyMiniLabel);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Bounds refresh
    // ─────────────────────────────────────────────────────────────────────────

    private void RefreshBounds()
    {
        boundsValid     = false;
        selectionBounds = new Bounds();

        var renderers = new List<Renderer>();
        foreach (GameObject go in Selection.gameObjects)
            renderers.AddRange(go.GetComponentsInChildren<Renderer>(true));

        if (renderers.Count == 0)
        {
            statusMessage = "No Renderers found in selection.";
            Repaint();
            return;
        }

        selectionBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Count; i++)
            selectionBounds.Encapsulate(renderers[i].bounds);

        boundsValid   = true;
        statusMessage = $"Bounds captured from {renderers.Count} renderer(s).";
        Repaint();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Light Probe generation
    // ─────────────────────────────────────────────────────────────────────────

    private void GenerateLightProbeGroup()
    {
        var positions = BuildLightProbePositions(selectionBounds, lp_padding, false);

        if (lp_addOuterShell)
            positions.AddRange(BuildLightProbePositions(selectionBounds, lp_padding + lp_outerOffset, true));

        if (positions.Count == 0)
        {
            statusMessage = "No light probe positions generated – check parameters.";
            return;
        }

        var go  = new GameObject("Light Probe Group");
        var lpg = go.AddComponent<LightProbeGroup>();

        var local = new Vector3[positions.Count];
        for (int i = 0; i < positions.Count; i++)
            local[i] = go.transform.InverseTransformPoint(positions[i]);

        lpg.probePositions = local;

        Undo.RegisterCreatedObjectUndo(go, "Create Light Probe Group");
        Selection.activeGameObject = go;

        statusMessage = $"✓ Created Light Probe Group with {positions.Count} probes.";
        Repaint();
    }

    private List<Vector3> BuildLightProbePositions(Bounds b, float extraPadding, bool shellOnly)
    {
        var result = new List<Vector3>();
        Bounds padded = new Bounds(b.center, b.size + Vector3.one * extraPadding * 2f);

        int stepsX = lp_subdivisionsX + 2;
        int stepsY = lp_subdivisionsY + 2;
        int stepsZ = lp_subdivisionsZ + 2;

        for (int xi = 0; xi < stepsX; xi++)
        {
            float x = Mathf.Lerp(padded.min.x, padded.max.x, stepsX == 1 ? 0.5f : (float)xi / (stepsX - 1));
            for (int yi = 0; yi < stepsY; yi++)
            {
                float y = Mathf.Lerp(padded.min.y, padded.max.y, stepsY == 1 ? 0.5f : (float)yi / (stepsY - 1));
                for (int zi = 0; zi < stepsZ; zi++)
                {
                    float z = Mathf.Lerp(padded.min.z, padded.max.z, stepsZ == 1 ? 0.5f : (float)zi / (stepsZ - 1));

                    bool onFace = (xi == 0 || xi == stepsX - 1)
                               || (yi == 0 || yi == stepsY - 1)
                               || (zi == 0 || zi == stepsZ - 1);

                    if (!shellOnly || onFace)
                        result.Add(new Vector3(x, y, z));
                }
            }
        }
        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Reflection Probe generation
    // ─────────────────────────────────────────────────────────────────────────

    private void GenerateReflectionProbes()
    {
        Bounds padded = new Bounds(selectionBounds.center,
                                   selectionBounds.size + Vector3.one * rp_padding * 2f);

        // Per-cell size when tiling
        Vector3 cellSize = new Vector3(
            padded.size.x / rp_countX,
            padded.size.y / rp_countY,
            padded.size.z / rp_countZ);

        var parent = new GameObject("Reflection Probes");
        Undo.RegisterCreatedObjectUndo(parent, "Create Reflection Probes");

        int created = 0;
        for (int xi = 0; xi < rp_countX; xi++)
        {
            for (int yi = 0; yi < rp_countY; yi++)
            {
                for (int zi = 0; zi < rp_countZ; zi++)
                {
                    // Centre of this cell
                    Vector3 centre = padded.min + new Vector3(
                        (xi + 0.5f) * cellSize.x,
                        (yi + 0.5f) * cellSize.y,
                        (zi + 0.5f) * cellSize.z);

                    var probeGO = new GameObject($"ReflectionProbe_{xi}_{yi}_{zi}");
                    probeGO.transform.SetParent(parent.transform, true);
                    probeGO.transform.position = centre;

                    Undo.RegisterCreatedObjectUndo(probeGO, "Create Reflection Probe");

                    var rp = probeGO.AddComponent<ReflectionProbe>();

                    // ── Box size ───────────────────────────────────────────
                    rp.size        = rp_overlapBoxes ? padded.size : cellSize;
                    rp.center      = Vector3.zero; // local offset from probe GO

                    // ── Projection ─────────────────────────────────────────
                    rp.boxProjection = rp_autoBoxProject;
                    rp.blendDistance = rp_blendDistance;

                    // ── Capture ────────────────────────────────────────────
                    rp.mode        = rp_mode;
                    rp.refreshMode = rp_refresh;
                    rp.resolution  = rp_resolution;
                    rp.hdr         = rp_hdr;
                    rp.intensity   = rp_intensity;
                    rp.importance  = rp_importance;
                    rp.nearClipPlane = rp_nearClip;
                    rp.farClipPlane  = rp_farClip;

                    created++;
                }
            }
        }

        Selection.activeGameObject = parent;
        statusMessage = $"✓ Created {created} Reflection Probe(s) inside '{parent.name}'.";
        Repaint();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private int EstimateLightProbeCount()
    {
        int sx = lp_subdivisionsX + 2;
        int sy = lp_subdivisionsY + 2;
        int sz = lp_subdivisionsZ + 2;
        int total    = sx * sy * sz;
        int interior = Mathf.Max(0, sx - 2) * Mathf.Max(0, sy - 2) * Mathf.Max(0, sz - 2);
        int faces    = total - interior;
        return lp_addOuterShell ? total + faces : total;
    }

    private void DrawDivider()
    {
        GUILayout.Space(6);
        var rect = EditorGUILayout.GetControlRect(false, 1f);
        EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 0.5f));
        GUILayout.Space(6);
    }

    private void OnSelectionChange()
    {
        RefreshBounds();
        Repaint();
    }
}
