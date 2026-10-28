using System.Collections.Generic;
using UnityEngine;

public partial class OverworldMapGenerator
{
    private struct AuthoredEncounter
    {
        public MapData map;
        public NodeType type;
    }

    private Domain requestedDomain;
    private MapData authoredTeleportMap;
    private bool rebuilding;
    private int rebuildPlayerNode;
    private readonly HashSet<int> rebuildClearedNodes = new HashSet<int>();
    private InputState lastAttempt;
    private InputState lastSuccessful;
    private bool hasAttempt;
    private bool hasSuccessfulGeneration;
    private bool hasPreviewReset;

    // Store exact reference/scalar inputs. Comparing these does not serialize assets or allocate each frame.
    private readonly List<object> observedInputs = new List<object>();
    private int inputCursor;
    private bool inputsDiffer;

    private struct InputState
    {
        public bool authored;
        public Domain domain;
        public Level[] levels;
        public MapData[] maps;
        public MapData.LevelType[] types;
    }

    private bool Observe<T>(T value)
    {
        bool changed = inputCursor >= observedInputs.Count ||
            (observedInputs[inputCursor] == null
                ? !EqualityComparer<T>.Default.Equals(default, value)
                : !(observedInputs[inputCursor] is T previous && EqualityComparer<T>.Default.Equals(previous, value)));
        if (changed)
        {
            if (inputCursor < observedInputs.Count) observedInputs[inputCursor] = value;
            else observedInputs.Add(value);
            inputsDiffer = true;
        }
        inputCursor++;
        return changed;
    }

    private bool LiveInputsChanged()
    {
        inputCursor = 0;
        inputsDiffer = false;
        Observe(authoredMode);
        Observe(requestedDomain);
        Observe(config);
        Observe(mapNode);
        if (mapNode != null)
        {
            Observe(mapNode.GetComponent<EnemySelection>());
            Observe(mapNode.GetComponentInChildren<MapNodeController>(true));
            Observe(mapNode.GetComponentInChildren<MeshRenderer>(true));
        }
        Observe(mapNodeTransform);
        if (mapNodeTransform != null) Observe(mapNodeTransform.position);
        Observe(lineVerticalOffset);
        Observe(pathLineMaterial);
        if (config != null)
        {
            Observe(config.mapWidth);
            Observe(config.mapDepth);
            Observe(config.minDistanceApart);
            Observe(config.randomSeed);
            Observe(config.randomizeSeedOnGeneration);
            Observe(config.fullyRandomizeRules);
            Observe(config.mapWidthRange);
            Observe(config.minDistanceRange);
            Observe(config.regularWeightRange);
            Observe(config.puzzleWeightRange);
            Observe(config.minibossWeightRange);
            Observe(config.puzzleThresholdRange);
            Observe(config.minibossThresholdRange);
            Observe(config.regularBattleWeight);
            Observe(config.puzzleBattleWeight);
            Observe(config.minibossBattleWeight);
            Observe(config.bossBattleWeight);
            Observe(config.puzzleBattleThreshold);
            Observe(config.minibossBattleThreshold);
            Observe(config.enforceChokepointProgressionRule);
            Observe(config.chokepointCompletionPercentageRequired);
        }
        if (requestedDomain != null && requestedDomain.levelList != null)
        {
            Observe(requestedDomain.levelList.Length);
            foreach (Level level in requestedDomain.levelList)
            {
                Observe(level);
                if (level == null) continue;
                Observe(level.map);
                if (level.map != null) Observe(level.map.levelType);
                Observe(level.enemyPartyData);
                Observe(level.levelNumber);
                Observe(level.conversationTitle ?? "");
            }
        }
        Observe(debugMapDataPopulationIsActive);
        if (debugNodeMapOverrides != null)
        {
            Observe(debugNodeMapOverrides.Count);
            foreach (var entry in debugNodeMapOverrides)
            {
                Observe(entry != null);
                if (entry == null) continue;
                Observe(entry.nodeIndex);
                Observe(entry.mapData);
                if (entry.mapData != null) Observe(entry.mapData.levelType);
            }
        }
        Observe(authoredTeleportMap);
        if (authoredTeleportMap != null) Observe(authoredTeleportMap.levelType);
        if (!authoredMode)
        {
            Observe(enforceDifficultyProgression);
            Observe(maxEasyNodeIndex);
            Observe(maxMediumNodeIndex);
            ObservePool(regularMaps);
            ObservePool(puzzleMaps);
            ObservePool(minibossMaps);
            ObservePool(bossMaps);
        }
        if (inputCursor != observedInputs.Count)
        {
            observedInputs.RemoveRange(inputCursor, observedInputs.Count - inputCursor);
            inputsDiffer = true;
        }
        return inputsDiffer || !hasAttempt;
    }

    private void ObservePool(List<MapData> pool)
    {
        Observe(pool != null ? pool.Count : -1);
        if (pool == null) return;
        foreach (var map in pool)
        {
            Observe(map);
            if (map != null) Observe(map.difficultyLevel);
        }
    }

    private InputState GetDistributionState()
    {
        Level[] levels = requestedDomain != null && requestedDomain.levelList != null
            ? (Level[])requestedDomain.levelList.Clone() : new Level[0];
        var state = new InputState
        {
            authored = authoredMode,
            domain = requestedDomain,
            levels = levels,
            maps = new MapData[levels.Length],
            types = new MapData.LevelType[levels.Length]
        };
        for (int i = 0; i < levels.Length; i++)
        {
            if (!authoredMode || levels[i] == null) continue;
            state.maps[i] = GetEffectiveAuthoredMap(levels[i], i);
            if (state.maps[i] != null) state.types[i] = state.maps[i].levelType;
        }
        return state;
    }

    private static bool SameDistribution(InputState left, InputState right)
    {
        if (left.authored != right.authored || left.domain != right.domain ||
            left.levels.Length != right.levels.Length) return false;
        for (int i = 0; i < left.levels.Length; i++)
            if (left.levels[i] != right.levels[i] || left.maps[i] != right.maps[i] ||
                left.types[i] != right.types[i]) return false;
        return true;
    }

    private MapData GetEffectiveAuthoredMap(Level level, int index)
    {
        if (authoredTeleportMap != null && index == requestedDomain.levelList.Length - 1)
            return authoredTeleportMap;
        MapData debugMap = GetDebugMapOverrideForNode(index);
        return debugMap != null ? debugMap : level.map;
    }

    private static bool TryGetNodeType(MapData.LevelType type, out NodeType nodeType)
    {
        switch (type)
        {
            case MapData.LevelType.Regular: nodeType = NodeType.RegularBattle; return true;
            case MapData.LevelType.Puzzle: nodeType = NodeType.PuzzleBattle; return true;
            case MapData.LevelType.Miniboss: nodeType = NodeType.MinibossBattle; return true;
            case MapData.LevelType.Boss: nodeType = NodeType.BossBattle; return true;
            default: nodeType = default; return false;
        }
    }

    private bool TryResolveEncounters(out AuthoredEncounter[] encounters)
    {
        encounters = null;
        if (config == null || mapNode == null || mapNodeTransform == null)
            return GenerationError("Assign Config, Map Node prefab and Map Node Transform.");
        if (requestedDomain == null || requestedDomain.levelList == null || requestedDomain.levelList.Length == 0)
            return GenerationError("Assign a Domain with at least one Level.");
        if (mapNode.GetComponent<EnemySelection>() == null ||
            mapNode.GetComponentInChildren<MapNodeController>(true) == null ||
            mapNode.GetComponentInChildren<MeshRenderer>(true) == null)
            return GenerationError("Map Node requires EnemySelection on its root, MapNodeController and MeshRenderer.");

        if (authoredMode) encounters = new AuthoredEncounter[requestedDomain.levelList.Length];
        for (int i = 0; i < requestedDomain.levelList.Length; i++)
        {
            Level level = requestedDomain.levelList[i];
            if (level == null)
                return GenerationError($"Domain '{requestedDomain.name}', node {i}: missing Level.");
            if (!authoredMode) continue;
            if (level.map == null || level.enemyPartyData == null)
                return GenerationError($"Node {i}, Level '{level.name}': assign Map and Enemy Party Data.");
            MapData map = GetEffectiveAuthoredMap(level, i);
            if (!TryGetNodeType(map.levelType, out NodeType type))
                return GenerationError($"Node {i}, Map '{map.name}': unsupported level type {map.levelType}.");
            encounters[i] = new AuthoredEncounter { map = map, type = type };
        }
        return true;
    }

    private bool GenerationError(string message)
    {
        Debug.LogError($"[MapGenerator] {message} Keeping the last valid map.", this);
        return false;
    }

    private bool TryGenerateLevel(bool liveRebuild)
    {
        LiveInputsChanged();
        hasAttempt = true;
        lastAttempt = GetDistributionState();
        if (!TryResolveEncounters(out var encounters)) return false;

        rebuilding = liveRebuild && hasSuccessfulGeneration;
        rebuildClearedNodes.Clear();
        bool reset = rebuilding && !SameDistribution(lastAttempt, lastSuccessful);
        hasPreviewReset |= reset;
        rebuildPlayerNode = reset ? 0 : Mathf.Clamp(currentNodeId, 0, requestedDomain.levelList.Length - 1);
        if (rebuilding && !reset)
        {
            for (int i = 0; i < spawnedNodes.Count; i++)
                if (spawnedNodes[i].GetComponentInChildren<MapNodeController>().currentLockStatus ==
                    MapNodeController.LockStatus.levelCleared)
                    rebuildClearedNodes.Add(i);
        }
        if (hasSuccessfulGeneration)
        {
            StopAllCoroutines();
            isMoving = false;
            ClearMap();
        }
        GenerateLevelCore(requestedDomain, encounters);
        rebuilding = false;
        hasSuccessfulGeneration = true;
        lastSuccessful = lastAttempt;
        if (liveRebuild)
            foreach (var deity in FindObjectsOfType<RoamingDeityController>())
                if (deity.mapGenerator == this) deity.RefreshAfterMapRebuild();
        return true;
    }

    public void CommitPreviewNodeForBattle()
    {
        if (!hasPreviewReset) return;
        // Explicit battle entry, unlike a preview rebuild, must identify the node that completion will save.
        SaveStateManager.saveData.currentNodeId = currentNodeId;
        SaveStateManager.SaveGame(SaveStateManager.saveData);
    }

    private void OnDestroy()
    {
        if (runtimeConfig != null) DestroyGeneratedObject(runtimeConfig);
    }

    private static void DestroyGeneratedObject(UnityEngine.Object instance)
    {
        if (Application.isPlaying) Destroy(instance);
        else DestroyImmediate(instance);
    }
}
