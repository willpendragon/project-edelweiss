#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class MapEditorWindow
{
    public class HeightPaintingTests
    {
        private MapEditorWindow window;
        private readonly List<GameObject> testObjects = new List<GameObject>();
        private string prefabPath;
        private GameObject prefab;
        private Scene originalScene;
        private Scene testScene;

        [SetUp]
        public void SetUp()
        {
            originalScene = SceneManager.GetActiveScene();
            testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(testScene);
            window = CreateInstance<MapEditorWindow>();
            window.clearOnClose = false;
            window.gridWidth = 2;
            window.gridHeight = 2;
            window.gridDepth = 6;
            window.tilePrefab = null;
            window.inBetweenTilesXOffset = 0f;
            window.inBetweenTilesZOffset = 0f;
            window.tiles.Clear();
            window.decorations.Clear();
            window.spawnedUnits.Clear();
            window.spawnedEnemies.Clear();
            window.spawnedInteractables.Clear();
            window.spawnedDeityShards.Clear();
            window.SetMode();
            window.SetPaintingHeight(false, 0);
        }

        [TearDown]
        public void TearDown()
        {
            if (prefab != null)
            {
                foreach (GameObject instance in FindPrefabInstances())
                    DestroyImmediate(instance);
            }
            foreach (GameObject instance in testObjects)
            {
                if (instance != null) DestroyImmediate(instance);
            }
            testObjects.Clear();
            if (window != null) DestroyImmediate(window);
            if (prefabPath != null) AssetDatabase.DeleteAsset(prefabPath);
            if (originalScene.IsValid() && originalScene.isLoaded)
                SceneManager.SetActiveScene(originalScene);
            if (testScene.IsValid() && testScene.isLoaded)
                EditorSceneManager.CloseScene(testScene, true);
            prefabPath = null;
            prefab = null;
        }

        [Test]
        public void FixedHeightProjectsOntoPlaneUsingTileSizeAndOffsets()
        {
            GameObject tile = CreateTestObject();
            tile.AddComponent<BoxCollider>().size = new Vector3(2f, 2f, 2f);
            window.tilePrefab = tile;
            window.inBetweenTilesXOffset = 0.5f;
            window.inBetweenTilesZOffset = 1f;
            window.SetPaintingHeight(true, 3);

            Ray ray = new Ray(new Vector3(0f, 10f, 0f), new Vector3(2.5f, -4f, 3f));
            Assert.That(window.TryGetFixedHeightTarget(ray, out Vector3Int target), Is.True);
            Assert.That(target, Is.EqualTo(new Vector3Int(1, 3, 1)));
            Assert.That(window.GridToWorld(target, window.GetTileWorldSize3D()),
                Is.EqualTo(new Vector3(2.5f, 6f, 3f)));
        }

        [Test]
        public void FixedHeightDoesNotRequireColliders()
        {
            window.SetPaintingHeight(true, 4);
            Ray ray = new Ray(new Vector3(1f, 10f, 1f), Vector3.down);
            Assert.That(window.TryGetFixedHeightTarget(ray, out Vector3Int target), Is.True);
            Assert.That(target, Is.EqualTo(new Vector3Int(1, 4, 1)));
        }

        [Test]
        public void FixedHeightRejectsParallelAndBackwardRays()
        {
            window.SetPaintingHeight(true, 2);
            Assert.That(window.TryGetFixedHeightTarget(
                new Ray(Vector3.up * 10f, Vector3.right), out _), Is.False);
            Assert.That(window.TryGetFixedHeightTarget(
                new Ray(Vector3.up * 10f, Vector3.up), out _), Is.False);
        }

        [Test]
        public void HeightClampsAndClearsPreviousStroke()
        {
            window.SetPaintingHeight(true, 2);
            window.lastGridPosition = window.lastPaintedPosition = new Vector3Int(1, 2, 1);
            window.SetPaintingHeight(true, 100);
            Assert.That(window.paintingLayer, Is.EqualTo(5));
            Assert.That(window.lastGridPosition.x, Is.EqualTo(-1));
            Assert.That(window.lastPaintedPosition.x, Is.EqualTo(-1));

            window.gridDepth = 2;
            window.SetPaintingHeight(true, window.paintingLayer);
            Assert.That(window.paintingLayer, Is.EqualTo(1));
            window.SetPaintingHeight(true, -10);
            Assert.That(window.paintingLayer, Is.Zero);
        }

        [Test]
        public void SwitchingPlacementModeClearsPreviousStroke()
        {
            window.SetPaintingHeight(true, 2);
            window.lastPaintedPosition = new Vector3Int(1, 2, 1);
            window.SetPaintingHeight(false, 2);
            Assert.That(window.lastPaintedPosition.x, Is.EqualTo(-1));
        }

        [Test]
        public void WheelStepsAccumulateFractionalInputAndRespectBounds()
        {
            window.SetPaintingHeight(true, 2);
            for (int i = 0; i < 3; i++) window.AdjustPaintingHeight(-0.75f);
            Assert.That(window.paintingLayer, Is.EqualTo(2));
            window.AdjustPaintingHeight(-0.75f);
            Assert.That(window.paintingLayer, Is.EqualTo(3));
            window.AdjustPaintingHeight(3f);
            Assert.That(window.paintingLayer, Is.EqualTo(2));
            window.AdjustPaintingHeight(-30f);
            Assert.That(window.paintingLayer, Is.EqualTo(5));
            window.AdjustPaintingHeight(3f);
            Assert.That(window.paintingLayer, Is.EqualTo(4));
            window.AdjustPaintingHeight(30f);
            Assert.That(window.paintingLayer, Is.Zero);
        }

        [TestCase("tile")]
        [TestCase("decoration")]
        [TestCase("player")]
        [TestCase("enemy")]
        [TestCase("interactable")]
        [TestCase("shard")]
        public void FixedHeightBucketPaintsEveryCategoryWithoutSupport(string category)
        {
            CreateTestPrefab();
            window.tilePrefab = window.decorationPrefab = window.unitPrefab = prefab;
            window.enemyPrefab = window.interactablePrefab = window.deityShardPrefab = prefab;
            window.decorationPrefabs = new List<GameObject> { prefab };
            window.SetMode(tile: category == "tile", deco: category == "decoration",
                unit: category == "player", enemy: category == "enemy",
                interactable: category == "interactable", deityShard: category == "shard");
            window.SetPaintingHeight(true, 3);
            window.ApplyBucketFill(new Vector3Int(0, 3, 0));

            Vector3[] actual = FindPrefabInstances().Select(instance => instance.transform.position).ToArray();
            CollectionAssert.AreEquivalent(new[]
            {
                new Vector3(0f, 6f, 0f), new Vector3(2f, 6f, 0f),
                new Vector3(0f, 6f, 2f), new Vector3(2f, 6f, 2f)
            }, actual);
        }

        [Test]
        public void SurfaceBucketRetainsSupportRequirement()
        {
            CreateTestPrefab();
            window.tilePrefab = window.unitPrefab = prefab;
            window.SetMode(unit: true);
            window.SetPaintingHeight(false, 3);
            window.ApplyBucketFill(new Vector3Int(0, 3, 0));

            GameObject[] instances = FindPrefabInstances();
            Assert.That(instances.Length, Is.EqualTo(1));
            Assert.That(instances[0].transform.position, Is.EqualTo(new Vector3(0f, 6f, 0f)));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BucketDeletionIsLayerConfinedOnlyInFixedHeightMode(bool fixedHeight)
        {
            GameObject first = CreateTestObject();
            GameObject adjacent = CreateTestObject();
            GameObject above = CreateTestObject();
            window.tiles[new Vector3Int(0, 2, 0)] = first;
            window.tiles[new Vector3Int(1, 2, 0)] = adjacent;
            window.tiles[new Vector3Int(0, 3, 0)] = above;
            window.SetMode(delete: true);
            window.SetPaintingHeight(fixedHeight, 2);
            window.ApplyBucketFill(new Vector3Int(0, 2, 0));

            Assert.That(first == null, Is.True);
            Assert.That(adjacent == null, Is.True);
            Assert.That(above != null, Is.EqualTo(fixedHeight));
        }

        private GameObject CreateTestObject()
        {
            GameObject instance = new GameObject("HeightPaintingTest");
            testObjects.Add(instance);
            return instance;
        }

        private void CreateTestPrefab()
        {
            GameObject source = CreateTestObject();
            source.AddComponent<BoxCollider>().size = Vector3.one * 2f;
            prefabPath = AssetDatabase.GenerateUniqueAssetPath("Assets/HeightPaintingTest.prefab");
            prefab = PrefabUtility.SaveAsPrefabAsset(source, prefabPath);
            DestroyImmediate(source);
        }

        private GameObject[] FindPrefabInstances()
        {
            return GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(instance => PrefabUtility.GetCorrespondingObjectFromSource(instance) == prefab)
                .ToArray();
        }
    }
}
#endif
