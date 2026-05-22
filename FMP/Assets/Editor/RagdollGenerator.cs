using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity Editor tool that generates a ragdoll from a humanoid or custom armature.
/// Place this file in any Editor/ folder in your project.
/// Open via: Window > Ragdoll Generator
/// </summary>
public class RagdollGenerator : EditorWindow
{
    // ─── Bone mapping ────────────────────────────────────────────────────────
    private Transform rootBone;
    private Transform hips;
    private Transform spine;
    private Transform chest;
    private Transform head;
    private Transform leftUpperArm, leftLowerArm, leftHand;
    private Transform rightUpperArm, rightLowerArm, rightHand;
    private Transform leftUpperLeg, leftLowerLeg, leftFoot;
    private Transform rightUpperLeg, rightLowerLeg, rightFoot;

    // ─── Settings ────────────────────────────────────────────────────────────
    private float totalMass = 70f;
    private float jointSpring = 0f;
    private float jointDamper = 0f;
    private float angularSpringLimit = 45f;
    private bool autoDetectBones = true;
    private bool removeExistingComponents = true;
    private bool useHumanoidAvatar = true;

    // ─── Capsule axis ────────────────────────────────────────────────────────
    private enum CapsuleAxis { X = 0, Y = 1, Z = 2 }

    // ─── UI state ────────────────────────────────────────────────────────────
    private Vector2 scroll;
    private bool showBoneMapping = true;
    private bool showSettings = true;
    private bool showAdvanced = false;
    private GUIStyle headerStyle;
    private GUIStyle boxStyle;
    private Animator targetAnimator;

    [MenuItem("Window/Ragdoll Generator")]
    public static void Open() => GetWindow<RagdollGenerator>("Ragdoll Generator");

    // ─────────────────────────────────────────────────────────────────────────
    // GUI
    // ─────────────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        InitStyles();
        scroll = EditorGUILayout.BeginScrollView(scroll);

        DrawHeader();
        DrawTargetSection();
        DrawSettingsSection();
        DrawBoneMappingSection();
        DrawAdvancedSection();
        DrawGenerateButton();

        EditorGUILayout.EndScrollView();
    }

    private void InitStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                margin = new RectOffset(4, 4, 8, 4)
            };
        }
        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 8, 8)
            };
        }
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Ragdoll Generator", headerStyle);
        EditorGUILayout.LabelField("Builds colliders, rigidbodies, and joints from an armature.",
            EditorStyles.miniLabel);
        EditorGUILayout.Space(4);
        DrawSeparator();
    }

    private void DrawTargetSection()
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        targetAnimator = (Animator)EditorGUILayout.ObjectField(
            "Animator", targetAnimator, typeof(Animator), true);
        if (EditorGUI.EndChangeCheck() && targetAnimator != null && autoDetectBones)
            AutoDetectBones();

        rootBone = (Transform)EditorGUILayout.ObjectField(
            "Root / Hips Override", rootBone, typeof(Transform), true);

        autoDetectBones = EditorGUILayout.Toggle("Auto-Detect Bones", autoDetectBones);

        EditorGUILayout.Space(2);
    }

    private void DrawSettingsSection()
    {
        DrawSeparator();
        showSettings = EditorGUILayout.Foldout(showSettings, "Settings", true, EditorStyles.foldoutHeader);
        if (!showSettings) return;

        EditorGUILayout.Space(2);
        totalMass = EditorGUILayout.FloatField("Total Mass (kg)", totalMass);
        removeExistingComponents = EditorGUILayout.Toggle("Remove Existing Components", removeExistingComponents);
        EditorGUILayout.Space(2);
    }

    private void DrawBoneMappingSection()
    {
        DrawSeparator();
        showBoneMapping = EditorGUILayout.Foldout(showBoneMapping, "Bone Mapping", true, EditorStyles.foldoutHeader);
        if (!showBoneMapping) return;

        EditorGUILayout.Space(4);

        EditorGUILayout.LabelField("Core", EditorStyles.miniBoldLabel);
        hips          = BoneField("Hips",          hips);
        spine         = BoneField("Spine",          spine);
        chest         = BoneField("Chest",          chest);
        head          = BoneField("Head",           head);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Left Arm", EditorStyles.miniBoldLabel);
        leftUpperArm  = BoneField("Upper Arm",     leftUpperArm);
        leftLowerArm  = BoneField("Lower Arm",     leftLowerArm);
        leftHand      = BoneField("Hand",           leftHand);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Right Arm", EditorStyles.miniBoldLabel);
        rightUpperArm = BoneField("Upper Arm",     rightUpperArm);
        rightLowerArm = BoneField("Lower Arm",     rightLowerArm);
        rightHand     = BoneField("Hand",           rightHand);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Left Leg", EditorStyles.miniBoldLabel);
        leftUpperLeg  = BoneField("Upper Leg",     leftUpperLeg);
        leftLowerLeg  = BoneField("Lower Leg",     leftLowerLeg);
        leftFoot      = BoneField("Foot",           leftFoot);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Right Leg", EditorStyles.miniBoldLabel);
        rightUpperLeg = BoneField("Upper Leg",     rightUpperLeg);
        rightLowerLeg = BoneField("Lower Leg",     rightLowerLeg);
        rightFoot     = BoneField("Foot",           rightFoot);

        EditorGUILayout.Space(4);

        if (GUILayout.Button("Auto-Detect from Animator / Hierarchy"))
            AutoDetectBones();
    }

    private void DrawAdvancedSection()
    {
        DrawSeparator();
        showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced Joint Settings", true, EditorStyles.foldoutHeader);
        if (!showAdvanced) return;

        EditorGUILayout.Space(2);
        jointSpring       = EditorGUILayout.FloatField("Joint Spring",        jointSpring);
        jointDamper       = EditorGUILayout.FloatField("Joint Damper",        jointDamper);
        angularSpringLimit = EditorGUILayout.FloatField("Angular Limit (°)",  angularSpringLimit);
        EditorGUILayout.Space(2);
    }

    private void DrawGenerateButton()
    {
        DrawSeparator();
        EditorGUILayout.Space(6);

        bool ready = hips != null;
        EditorGUI.BeginDisabledGroup(!ready);

        Color prev = GUI.backgroundColor;
        GUI.backgroundColor = ready ? new Color(0.4f, 0.8f, 0.4f) : Color.gray;
        if (GUILayout.Button("Generate Ragdoll", GUILayout.Height(36)))
            BuildRagdoll();
        GUI.backgroundColor = prev;

        EditorGUI.EndDisabledGroup();

        if (!ready)
            EditorGUILayout.HelpBox("Assign at least the Hips bone to enable generation.", MessageType.Info);

        EditorGUILayout.Space(10);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Auto-detection
    // ─────────────────────────────────────────────────────────────────────────
    private void AutoDetectBones()
    {
        // Try Humanoid avatar first
        if (targetAnimator != null && targetAnimator.isHuman)
        {
            hips          = targetAnimator.GetBoneTransform(HumanBodyBones.Hips);
            spine         = targetAnimator.GetBoneTransform(HumanBodyBones.Spine);
            chest         = targetAnimator.GetBoneTransform(HumanBodyBones.Chest);
            head          = targetAnimator.GetBoneTransform(HumanBodyBones.Head);
            leftUpperArm  = targetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            leftLowerArm  = targetAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            leftHand      = targetAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightUpperArm = targetAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            rightLowerArm = targetAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            rightHand     = targetAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            leftUpperLeg  = targetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            leftLowerLeg  = targetAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            leftFoot      = targetAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightUpperLeg = targetAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            rightLowerLeg = targetAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            rightFoot     = targetAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
            Debug.Log("[RagdollGenerator] Detected humanoid bones via Avatar.");
            return;
        }

        // Fallback: name-based search in the selection or animator's hierarchy
        Transform searchRoot = targetAnimator != null
            ? targetAnimator.transform
            : (Selection.activeTransform);

        if (searchRoot == null) { Debug.LogWarning("[RagdollGenerator] No target to search."); return; }

        var all = searchRoot.GetComponentsInChildren<Transform>(true);
        hips          = FindBone(all, "hips", "pelvis", "root");
        spine         = FindBone(all, "spine");
        chest         = FindBone(all, "chest", "spine1", "spine2");
        head          = FindBone(all, "head");
        leftUpperArm  = FindBone(all, "leftshoulder", "leftupperarm", "upperarm_l", "arm_l");
        leftLowerArm  = FindBone(all, "leftforearm", "leftlowerarm", "lowerarm_l", "forearm_l");
        leftHand      = FindBone(all, "lefthand", "hand_l");
        rightUpperArm = FindBone(all, "rightshoulder", "rightupperarm", "upperarm_r", "arm_r");
        rightLowerArm = FindBone(all, "rightforearm", "rightlowerarm", "lowerarm_r", "forearm_r");
        rightHand     = FindBone(all, "righthand", "hand_r");
        leftUpperLeg  = FindBone(all, "leftupperleg", "leftthigh", "thigh_l", "upleg_l");
        leftLowerLeg  = FindBone(all, "leftlowerleg", "leftcalf", "calf_l", "leg_l");
        leftFoot      = FindBone(all, "leftfoot", "foot_l");
        rightUpperLeg = FindBone(all, "rightupperleg", "rightthigh", "thigh_r", "upleg_r");
        rightLowerLeg = FindBone(all, "rightlowerleg", "rightcalf", "calf_r", "leg_r");
        rightFoot     = FindBone(all, "rightfoot", "foot_r");

        Debug.Log("[RagdollGenerator] Detected bones via name matching.");
    }

    private static Transform FindBone(Transform[] bones, params string[] keywords)
    {
        foreach (var t in bones)
        {
            string name = t.name.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");
            foreach (var kw in keywords)
                if (name == kw || name.Contains(kw)) return t;
        }
        return null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Generation
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildRagdoll()
    {
        if (hips == null) { Debug.LogError("[RagdollGenerator] Hips bone is required."); return; }

        Undo.SetCurrentGroupName("Generate Ragdoll");
        int undoGroup = Undo.GetCurrentGroup();

        // Disable animator to avoid fighting the ragdoll
        if (targetAnimator != null)
            Undo.RecordObject(targetAnimator, "Disable Animator");

        // ── Mass distribution table ──────────────────────────────────────
        // Rough proportional masses from biomechanics data (% of total body mass)
        var massMap = new Dictionary<Transform, float>();
        Assign(massMap, hips,          0.142f);
        Assign(massMap, spine,         0.091f);
        Assign(massMap, chest,         0.100f);
        Assign(massMap, head,          0.081f);
        Assign(massMap, leftUpperArm,  0.028f);
        Assign(massMap, leftLowerArm,  0.016f);
        Assign(massMap, leftHand,      0.006f);
        Assign(massMap, rightUpperArm, 0.028f);
        Assign(massMap, rightLowerArm, 0.016f);
        Assign(massMap, rightHand,     0.006f);
        Assign(massMap, leftUpperLeg,  0.100f);
        Assign(massMap, leftLowerLeg,  0.047f);
        Assign(massMap, leftFoot,      0.014f);
        Assign(massMap, rightUpperLeg, 0.100f);
        Assign(massMap, rightLowerLeg, 0.047f);
        Assign(massMap, rightFoot,     0.014f);

        // ── Process each bone ────────────────────────────────────────────
        var boneConfigs = BuildBoneConfigs();

        foreach (var cfg in boneConfigs)
        {
            if (cfg.bone == null) continue;

            if (removeExistingComponents) RemoveRagdollComponents(cfg.bone);

            float boneMass = massMap.TryGetValue(cfg.bone, out float ratio)
                ? totalMass * ratio
                : totalMass * 0.02f;

            AddCapsule(cfg.bone, cfg.axis, boneMass, cfg.connectedBody);
        }

        Undo.CollapseUndoOperations(undoGroup);
        Debug.Log($"[RagdollGenerator] Ragdoll built on '{hips.root.name}' with {boneConfigs.Count(c => c.bone != null)} bones.");
        EditorUtility.DisplayDialog("Ragdoll Generator",
            "Ragdoll generated successfully!\n\nCheck the Console for details.", "OK");
    }

    // ─── Bone config ─────────────────────────────────────────────────────────
    private struct BoneConfig
    {
        public Transform bone;
        public Transform connectedBody; // parent bone in the ragdoll chain
        public CapsuleAxis axis;
    }

    private IEnumerable<BoneConfig> BuildBoneConfigs() => new[]
    {
        // Core chain — no connected body on hips (it's the root)
        new BoneConfig { bone = hips,          connectedBody = null,          axis = CapsuleAxis.Y },
        new BoneConfig { bone = spine,         connectedBody = hips,          axis = CapsuleAxis.Y },
        new BoneConfig { bone = chest,         connectedBody = spine,         axis = CapsuleAxis.Y },
        new BoneConfig { bone = head,          connectedBody = chest ?? spine, axis = CapsuleAxis.Y },
        // Left arm
        new BoneConfig { bone = leftUpperArm,  connectedBody = chest ?? spine, axis = CapsuleAxis.X },
        new BoneConfig { bone = leftLowerArm,  connectedBody = leftUpperArm,  axis = CapsuleAxis.X },
        new BoneConfig { bone = leftHand,      connectedBody = leftLowerArm,  axis = CapsuleAxis.X },
        // Right arm
        new BoneConfig { bone = rightUpperArm, connectedBody = chest ?? spine, axis = CapsuleAxis.X },
        new BoneConfig { bone = rightLowerArm, connectedBody = rightUpperArm, axis = CapsuleAxis.X },
        new BoneConfig { bone = rightHand,     connectedBody = rightLowerArm, axis = CapsuleAxis.X },
        // Left leg
        new BoneConfig { bone = leftUpperLeg,  connectedBody = hips,          axis = CapsuleAxis.Y },
        new BoneConfig { bone = leftLowerLeg,  connectedBody = leftUpperLeg,  axis = CapsuleAxis.Y },
        new BoneConfig { bone = leftFoot,      connectedBody = leftLowerLeg,  axis = CapsuleAxis.Z },
        // Right leg
        new BoneConfig { bone = rightUpperLeg, connectedBody = hips,          axis = CapsuleAxis.Y },
        new BoneConfig { bone = rightLowerLeg, connectedBody = rightUpperLeg, axis = CapsuleAxis.Y },
        new BoneConfig { bone = rightFoot,     connectedBody = rightLowerLeg, axis = CapsuleAxis.Z },
    };

    // ─── Add collider + rigidbody + joint to one bone ───────────────────────
    private void AddCapsule(Transform bone, CapsuleAxis axis, float mass, Transform connectedBone)
    {
        Undo.RecordObject(bone.gameObject, "Add Ragdoll Component");

        // ── Measure bone length toward first child ────────────────────
        float length = 0.2f;
        if (bone.childCount > 0)
            length = Vector3.Distance(bone.position, bone.GetChild(0).position);
        float radius = Mathf.Max(length * 0.18f, 0.04f);

        // ── Capsule collider ─────────────────────────────────────────
        var col = Undo.AddComponent<CapsuleCollider>(bone.gameObject);
        col.direction = (int)axis;
        col.radius    = radius;
        col.height    = Mathf.Max(length, radius * 2f);

        // Centre the capsule along the bone's local axis
        Vector3 centre = Vector3.zero;
        centre[(int)axis] = length * 0.5f;
        col.center = centre;

        // ── Rigidbody ─────────────────────────────────────────────────
        var rb = Undo.AddComponent<Rigidbody>(bone.gameObject);
        rb.mass           = mass;
        rb.linearDamping  = 0.05f;
        rb.angularDamping = 0.05f;

        if (bone == hips) return; // Hips: root — no joint

        // ── Character joint ───────────────────────────────────────────
        var joint = Undo.AddComponent<CharacterJoint>(bone.gameObject);

        if (connectedBone != null)
        {
            var connectedRb = connectedBone.GetComponent<Rigidbody>();
            if (connectedRb != null) joint.connectedBody = connectedRb;
        }

        // Twist axis along bone's length
        joint.axis = LocalAxisFromCapsuleAxis(axis);
        joint.swingAxis = Vector3.Cross(joint.axis, Vector3.up).normalized;
        if (joint.swingAxis.sqrMagnitude < 0.01f)
            joint.swingAxis = Vector3.Cross(joint.axis, Vector3.right).normalized;

        var softLimit = new SoftJointLimitSpring
        {
            spring = jointSpring,
            damper = jointDamper
        };

        joint.twistLimitSpring = softLimit;
        joint.swingLimitSpring = softLimit;

        joint.lowTwistLimit  = new SoftJointLimit { limit = -angularSpringLimit * 0.5f };
        joint.highTwistLimit = new SoftJointLimit { limit =  angularSpringLimit * 0.5f };
        joint.swing1Limit    = new SoftJointLimit { limit =  angularSpringLimit };
        joint.swing2Limit    = new SoftJointLimit { limit =  angularSpringLimit * 0.5f };

        joint.enableProjection = true;
    }

    private static Vector3 LocalAxisFromCapsuleAxis(CapsuleAxis axis) =>
        axis switch
        {
            CapsuleAxis.X => Vector3.right,
            CapsuleAxis.Y => Vector3.up,
            _             => Vector3.forward
        };

    // ─── Remove existing ragdoll components ──────────────────────────────────
    private static void RemoveRagdollComponents(Transform bone)
    {
        foreach (var rb  in bone.GetComponents<Rigidbody>())       Undo.DestroyObjectImmediate(rb);
        foreach (var col in bone.GetComponents<CapsuleCollider>()) Undo.DestroyObjectImmediate(col);
        foreach (var col in bone.GetComponents<BoxCollider>())     Undo.DestroyObjectImmediate(col);
        foreach (var col in bone.GetComponents<SphereCollider>())  Undo.DestroyObjectImmediate(col);
        foreach (var j   in bone.GetComponents<CharacterJoint>())  Undo.DestroyObjectImmediate(j);
        foreach (var j   in bone.GetComponents<ConfigurableJoint>())Undo.DestroyObjectImmediate(j);
        foreach (var j   in bone.GetComponents<HingeJoint>())      Undo.DestroyObjectImmediate(j);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────
    private static Transform BoneField(string label, Transform current) =>
        (Transform)EditorGUILayout.ObjectField(label, current, typeof(Transform), true);

    private static void Assign(Dictionary<Transform, float> map, Transform t, float ratio)
    {
        if (t != null) map[t] = ratio;
    }

    private static void DrawSeparator()
    {
        EditorGUILayout.Space(2);
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
        EditorGUILayout.Space(2);
    }
}
