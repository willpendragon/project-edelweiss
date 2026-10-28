#if UNITY_INCLUDE_TESTS
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class OverworldNodeIndexOverlayTests
{
    private GameObject cameraObject;
    private Camera camera;

    [SetUp]
    public void SetUp()
    {
        cameraObject = new GameObject("NodeIndexOverlayTestCamera");
        camera = cameraObject.AddComponent<Camera>();
        camera.enabled = false;
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.aspect = 1f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100f;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(cameraObject);
    }

    [TestCase(true, true, true, true, true)]
    [TestCase(false, true, true, true, false)]
    [TestCase(true, false, true, true, false)]
    [TestCase(true, true, false, true, false)]
    [TestCase(true, true, true, false, false)]
    [TestCase(true, true, false, false, false)]
    public void VisibilityRequiresEnabledFocusedAndBothKeys(bool enabled, bool focused,
        bool shiftHeld, bool sHeld, bool expected)
    {
        MethodInfo method = GetHelper("ShouldShowNodeIndices");
        Assert.That(method.Invoke(null, new object[] { enabled, focused, shiftHeld, sHeld }),
            Is.EqualTo(expected));
    }

    [TestCase(0f, 0f, 10f, true)]
    [TestCase(0f, 0f, -10f, false)]
    [TestCase(0f, 0f, 0.01f, false)]
    [TestCase(0f, 0f, 101f, false)]
    [TestCase(6f, 0f, 10f, false)]
    [TestCase(0f, 6f, 10f, false)]
    public void ProjectionRejectsOffscreenAndClippedNodes(float x, float y, float z, bool expected)
    {
        object[] arguments = { camera, new Vector3(x, y, z), Vector2.zero };
        Assert.That(GetHelper("TryGetNodeLabelPosition").Invoke(null, arguments), Is.EqualTo(expected));
    }

    [Test]
    public void LabelPositionTracksCameraAndUsesTopLeftGuiOrigin()
    {
        camera.transform.position = new Vector3(10f, 2f, 0f);
        Vector3 worldPosition = new Vector3(11f, 3f, 10f);
        object[] arguments = { camera, worldPosition, Vector2.zero };
        Assert.That(GetHelper("TryGetNodeLabelPosition").Invoke(null, arguments), Is.EqualTo(true));
        Vector3 screenPosition = camera.WorldToScreenPoint(worldPosition);
        Assert.That(arguments[2],
            Is.EqualTo(new Vector2(screenPosition.x, Screen.height - screenPosition.y - 24f)));
    }

    private static MethodInfo GetHelper(string name)
    {
        MethodInfo method = typeof(OverworldMapGenerator).GetMethod(name,
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(method, Is.Not.Null, $"Missing overlay helper: {name}");
        return method;
    }
}
#endif
