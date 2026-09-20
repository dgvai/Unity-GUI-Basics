using UnityEditor;
using UnityEngine;

/// Editor utility that lays out a playable first pass of the runner scene:
/// a row of fixed/moving floor segments with gaps, obstacle cubes, a player ball,
/// a void zone under the gaps, the GameManager and a chase camera.
/// Run it, then tweak positions/scales by hand in the Scene view as needed.
public static class GameSceneBuilder
{
    private const float SegmentLength = 4f;
    private const float SegmentWidth = 4f;
    private const float GapLength = 2f;

    private struct SegmentPlan
    {
        public bool Moving;
        public bool Obstacle;
        public bool GapBefore;

        public SegmentPlan(bool moving, bool obstacle, bool gapBefore)
        {
            Moving = moving;
            Obstacle = obstacle;
            GapBefore = gapBefore;
        }
    }

    [MenuItem("Tools/Endless Runner/Build Game Scene")]
    public static void BuildScene()
    {
        GameObject root = new GameObject("Level");
        Undo.RegisterCreatedObjectUndo(root, "Build Game Scene");

        GameObject gameManagerGO = new GameObject("GameManager");
        gameManagerGO.AddComponent<GameManager>();
        Undo.RegisterCreatedObjectUndo(gameManagerGO, "Build Game Scene");

        SegmentPlan[] plan =
        {
            new SegmentPlan(moving: false, obstacle: false, gapBefore: false),
            new SegmentPlan(moving: false, obstacle: true, gapBefore: false),
            new SegmentPlan(moving: true, obstacle: false, gapBefore: true),
            new SegmentPlan(moving: false, obstacle: false, gapBefore: false),
            new SegmentPlan(moving: true, obstacle: true, gapBefore: true),
            new SegmentPlan(moving: false, obstacle: false, gapBefore: false),
            new SegmentPlan(moving: true, obstacle: false, gapBefore: true),
            new SegmentPlan(moving: false, obstacle: true, gapBefore: false),
            new SegmentPlan(moving: false, obstacle: false, gapBefore: true),
            new SegmentPlan(moving: true, obstacle: false, gapBefore: false),
        };

        Vector3 cursor = Vector3.zero;

        foreach (SegmentPlan segment in plan)
        {
            if (segment.GapBefore)
            {
                cursor.z += GapLength;
            }

            BuildFloorSegment(root.transform, segment, cursor);
            cursor.z += SegmentLength;
        }

        GameObject player = BuildPlayer();

        BuildVoidZone(cursor.z);

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            CameraFollow follow = mainCamera.GetComponent<CameraFollow>();
            if (follow == null)
            {
                follow = Undo.AddComponent<CameraFollow>(mainCamera.gameObject);
            }

            SerializedObject cameraSo = new SerializedObject(follow);
            cameraSo.FindProperty("target").objectReferenceValue = player.transform;
            cameraSo.ApplyModifiedProperties();
        }

        Selection.activeGameObject = root;
        Debug.Log("Game scene built. Play mode controls: WASD/Arrows to move, Space to jump.");
    }

    private static void BuildFloorSegment(Transform parent, SegmentPlan segment, Vector3 cursor)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = segment.Moving ? "MovingFloor" : "Floor";
        floor.transform.SetParent(parent);
        floor.transform.position = cursor + new Vector3(0f, -0.5f, SegmentLength * 0.5f);
        floor.transform.localScale = new Vector3(SegmentWidth, 1f, SegmentLength);
        Undo.RegisterCreatedObjectUndo(floor, "Build Game Scene");

        if (segment.Moving)
        {
            Rigidbody rb = floor.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            MovingPlatform platform = floor.AddComponent<MovingPlatform>();
            SerializedObject platformSo = new SerializedObject(platform);
            platformSo.FindProperty("pointA").vector3Value = Vector3.zero;
            platformSo.FindProperty("pointB").vector3Value = new Vector3(SegmentWidth * 0.6f, 0f, 0f);
            platformSo.FindProperty("speed").floatValue = 2f;
            platformSo.ApplyModifiedProperties();
        }

        if (segment.Obstacle)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = "Obstacle";
            obstacle.transform.SetParent(floor.transform);
            obstacle.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            obstacle.transform.localScale = new Vector3(1.2f / SegmentWidth, 1.5f, 1.2f / SegmentLength);
            obstacle.AddComponent<Obstacle>();
            Undo.RegisterCreatedObjectUndo(obstacle, "Build Game Scene");
        }
    }

    private static GameObject BuildPlayer()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1f, 1f);

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.linearDamping = 0.2f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        PlayerController controller = player.AddComponent<PlayerController>();

        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0f, -0.55f, 0f);

        SerializedObject controllerSo = new SerializedObject(controller);
        controllerSo.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
        controllerSo.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(player, "Build Game Scene");
        return player;
    }

    private static void BuildVoidZone(float levelLength)
    {
        GameObject voidZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        voidZone.name = "VoidZone";
        Object.DestroyImmediate(voidZone.GetComponent<MeshRenderer>());
        Object.DestroyImmediate(voidZone.GetComponent<MeshFilter>());
        voidZone.transform.position = new Vector3(0f, -10f, levelLength * 0.5f);
        voidZone.transform.localScale = new Vector3(200f, 1f, levelLength + 40f);
        voidZone.AddComponent<VoidZone>();
        Undo.RegisterCreatedObjectUndo(voidZone, "Build Game Scene");
    }
}
