using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq; // For FindIndex

public class MapGenerator : MonoBehaviour
{
    [Header("Data Source")]
    public TextAsset jsonFile; // Inspector에서 할당할 JSON 파일

    [Header("Layout Settings")]
    public float horizontalSpacing = 3f; // 열(레이어) 간 가로 간격
    public float verticalSpacing = 1f;   // 행(게이트) 간 세로 간격
    public Transform gateParent; // 생성된 게이트들을 담을 부모 Transform (선택적)

    // Prefab 경로 설정 (Resources 폴더 기준)
    private const string PREFAB_FOLDER_PATH = "Prefabs/LogicGates/";

    // 미리 로드된 프리팹을 저장할 딕셔너리
    private Dictionary<string, GameObject> loadedPrefabs = new Dictionary<string, GameObject>();
    // 로드해야 할 기본 프리팹 타입 목록
    private List<string> requiredPrefabTypes = new()
    {
        "INPUT", "OUTPUT", "AND", "OR", "NOT", "XOR", "WIRE"
    };

    // 생성된 게이트 오브젝트 관리 (ID -> GameObject)
    private Dictionary<string, GameObject> instantiatedGates = new Dictionary<string, GameObject>();
    private List<GameObject> connectionLines = new List<GameObject>(); // 생성된 연결선 오브젝트 관리 리스트 추가
    private LogicCircuitSerializer serializer = new(); // Serializer 인스턴스

    // 각 레이어의 수직(Z) 시작 오프셋을 저장할 딕셔너리
    private Dictionary<int, float> layerVerticalOffsets = new Dictionary<int, float>();

    [Header("Line Renderer Settings")]
    public Material lineMaterial; // Inspector에서 할당할 라인 머티리얼
    public float lineWidth = 0.1f;
    public Color lineColor = Color.white;
    public string outputPointName = "OutputPoint"; // 출력 연결점 이름 규칙
    public string inputPointPrefix = "InputPoint_"; // 입력 연결점 이름 규칙 접두사

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 1. 프리팹 미리 로드
        PreloadPrefabs();

        // 2. 맵 생성 시작
        if (jsonFile != null)
        {
            GenerateMap();
        }
        else
        {
            Debug.LogError("JSON 파일이 MapGenerator에 할당되지 않았습니다.");
        }
    }

    // 필요한 프리팹들을 미리 로드하는 함수
    void PreloadPrefabs()
    {
        loadedPrefabs.Clear(); // 기존 로드된 프리팹 초기화
        Debug.Log("프리팹 미리 로드를 시작합니다...");

        foreach (string typeName in requiredPrefabTypes)
        {
            GameObject prefab = LoadPrefabFromResources(typeName);
            if (prefab != null)
            {
                // 로드 성공 시 딕셔너리에 추가 (대문자 키 사용 통일)
                loadedPrefabs[typeName.ToUpper()] = prefab;
                Debug.Log($" - 프리팹 로드 성공: {typeName}");
            }
            else
            {
                // 로드 실패 시 경고 (LoadPrefabFromResources에서 이미 로그 출력)
                Debug.LogWarning($" - 프리팹 로드 실패: {typeName}");
            }
        }
        Debug.Log("프리팹 미리 로드가 완료되었습니다.");
    }

    public void GenerateMap()
    {
        // 0. 기존 맵 오브젝트 삭제 (필요시)
        ClearMap();

        // gateParent가 할당되지 않았으면 새로 생성
        if (gateParent == null)
        {
            GameObject generatedParentObj = new GameObject("Generated Gates");
            gateParent = generatedParentObj.transform;
            Debug.Log("gateParent가 설정되지 않아 'Generated Gates' 오브젝트를 생성하여 사용합니다.");
        }

        // gateParent의 위치를 원점으로 초기화 (중심 계산을 위해)
        gateParent.position = Vector3.zero;
        gateParent.rotation = Quaternion.identity;

        // 1. JSON 로드 및 파싱
        LogicCircuitData circuitData = serializer.LoadLogicCircuitFromJson(jsonFile.text);
        if (circuitData == null)
        {
            Debug.LogError("JSON 데이터를 로드하거나 파싱하는 데 실패했습니다.");
            return;
        }

        instantiatedGates.Clear(); // 딕셔너리 초기화
        layerVerticalOffsets.Clear(); // 레이어 오프셋 초기화
        connectionLines.Clear(); // 연결선 리스트 초기화

        // 1.5 각 레이어의 수직 중앙 정렬을 위한 오프셋 계산
        CalculateLayerOffsets(circuitData);

        // 2. 게이트 오브젝트 생성 및 배치
        // Input Gates
        float inputOffset = layerVerticalOffsets.ContainsKey(0) ? layerVerticalOffsets[0] : 0f;
        for (int i = 0; i < circuitData.InputGates.Count; i++)
        {
            PlaceGate(circuitData.InputGates[i], 0, i, inputOffset);
        }

        // Hidden Layers
        for (int layerIndex = 0; layerIndex < circuitData.HiddenLayers.Count; layerIndex++)
        {
            List<GateData> layer = circuitData.HiddenLayers[layerIndex];
            int currentLayerMapIndex = layerIndex + 1; // 실제 맵에서의 레이어 인덱스
            float hiddenOffset = layerVerticalOffsets.ContainsKey(currentLayerMapIndex) ? layerVerticalOffsets[currentLayerMapIndex] : 0f;
            for (int gateIndex = 0; gateIndex < layer.Count; gateIndex++)
            {
                PlaceGate(layer[gateIndex], currentLayerMapIndex, gateIndex, hiddenOffset);
            }
        }

        // Output Gates
        int outputLayerMapIndex = circuitData.HiddenLayers.Count + 1;
        float outputOffset = layerVerticalOffsets.ContainsKey(outputLayerMapIndex) ? layerVerticalOffsets[outputLayerMapIndex] : 0f;
        for (int i = 0; i < circuitData.OutputGates.Count; i++)
        {
            PlaceGate(circuitData.OutputGates[i], outputLayerMapIndex, i, outputOffset);
        }

        // 3. 전체 레이아웃 수평 중앙 정렬
        CenterLayoutHorizontally();

        // 4. 연결선 생성
        CreateConnections(circuitData);

        Debug.Log("맵 생성이 완료되었습니다.");
    }

    // 각 레이어의 수직(Z) 시작 오프셋 계산
    void CalculateLayerOffsets(LogicCircuitData circuitData)
    {
        // Input Layer (Index 0)
        int inputCount = circuitData.InputGates.Count;
        if (inputCount > 0)
        {
            float inputHeight = (inputCount - 1) * verticalSpacing;
            layerVerticalOffsets[0] = inputHeight / 2.0f; // 중앙 정렬을 위한 시작 Z 위치
        }

        // Hidden Layers (Index 1 to N)
        for (int i = 0; i < circuitData.HiddenLayers.Count; i++)
        {
            int hiddenCount = circuitData.HiddenLayers[i].Count;
            if (hiddenCount > 0)
            {
                float hiddenHeight = (hiddenCount - 1) * verticalSpacing;
                layerVerticalOffsets[i + 1] = hiddenHeight / 2.0f;
            }
        }

        // Output Layer (Index N+1)
        int outputCount = circuitData.OutputGates.Count;
        if (outputCount > 0)
        {
            int outputLayerIndex = circuitData.HiddenLayers.Count + 1;
            float outputHeight = (outputCount - 1) * verticalSpacing;
            layerVerticalOffsets[outputLayerIndex] = outputHeight / 2.0f;
        }
    }

    // 게이트 하나를 배치하는 함수
    void PlaceGate(GateData gateData, int layerIndex, int gateIndexInLayer, float layerVerticalOffset)
    {
        // 위치 계산 (XZ 평면)
        float xPos = layerIndex * horizontalSpacing;
        // Z 위치: 레이어의 시작 오프셋에서 아래로 배치
        float zPos = layerVerticalOffset - gateIndexInLayer * verticalSpacing;
        Vector3 position = new Vector3(xPos, 0, zPos);

        // 미리 로드된 프리팹 가져오기 (대문자 키 사용)
        if (!loadedPrefabs.TryGetValue(gateData.Type.ToUpper(), out GameObject prefabToSpawn) || prefabToSpawn == null)
        {
            Debug.LogError($"타입 '{gateData.Type}'에 대한 프리팹이 미리 로드되지 않았거나 유효하지 않습니다. (ID: {gateData.Id}) Resources 폴더와 프리팹 이름을 확인하세요.");
            return; // 프리팹 없으면 생성 불가
        }

        // 프리팹 인스턴스화
        GameObject spawnedGate = Instantiate(prefabToSpawn, position, Quaternion.identity);
        spawnedGate.name = $"{gateData.Type}_{gateData.Id}"; // 오브젝트 이름 설정 (디버깅 용이)

        // 부모 설정 (선택적)
        if (gateParent != null)
        {
            spawnedGate.transform.SetParent(gateParent);
        }

        // 생성된 게이트 딕셔너리에 추가
        if (!instantiatedGates.ContainsKey(gateData.Id))
        {
            instantiatedGates.Add(gateData.Id, spawnedGate);
        }
        else
        {
            Debug.LogWarning($"중복된 게이트 ID '{gateData.Id}'가 감지되었습니다. 이전 게이트를 덮어씁니다.");
            instantiatedGates[gateData.Id] = spawnedGate;
        }
    }

    // Resources 폴더에서 프리팹 로드하는 함수
    GameObject LoadPrefabFromResources(string prefabName)
    {
        // prefabName이 비어있거나 null인 경우 처리
        if (string.IsNullOrEmpty(prefabName))
        {
            Debug.LogError("프리팹 이름이 비어있거나 null입니다.");
            return null;
        }
        string path = PREFAB_FOLDER_PATH + prefabName; // 예: "Prefabs/AND"
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"Resources 폴더에서 프리팹을 로드할 수 없습니다: '{path}'. 경로와 프리팹 이름(대소문자 포함)을 확인하세요.");
        }
        return prefab;
    }

    // 생성된 게이트 레이아웃을 수평(X축) 중앙에 배치하는 함수
    void CenterLayoutHorizontally()
    {
        if (instantiatedGates.Count == 0)
        {
            Debug.LogWarning("배치된 게이트가 없어 중앙 정렬을 건너뛰었습니다.");
            return;
        }

        Bounds totalBounds = new Bounds();
        bool firstGate = true;

        // 모든 생성된 게이트의 위치를 포함하는 경계 계산
        foreach (var pair in instantiatedGates)
        {
            if (pair.Value != null)
            {
                if (firstGate)
                {
                    // 첫 번째 게이트 위치로 초기 경계 설정
                    totalBounds = new Bounds(pair.Value.transform.position, Vector3.zero);
                    firstGate = false;
                }
                else
                {
                    // 기존 경계에 현재 게이트 위치 포함
                    totalBounds.Encapsulate(pair.Value.transform.position);
                }
            }
        }

        // 계산된 경계의 X축 중심점
        float centerX = totalBounds.center.x;

        // X축 중심점을 원점으로 이동시키기 위한 오프셋 계산
        Vector3 offset = new Vector3(-centerX, 0, 0); // X축만 고려

        // gateParent를 사용하여 전체 그룹 이동 (gateParent가 설정된 경우)
        if (gateParent != null)
        {
            gateParent.position += offset;
            Debug.Log($"레이아웃 수평 중앙 정렬 완료. 오프셋: {offset}");
        }
        else
        {
            // gateParent가 없으면 개별 게이트 이동 (비효율적일 수 있음)
            Debug.LogWarning("gateParent가 설정되지 않아 개별 게이트를 이동합니다.");
            foreach (var pair in instantiatedGates)
            {
                if (pair.Value != null)
                {
                    pair.Value.transform.position += offset;
                }
            }
            Debug.Log($"레이아웃 수평 중앙 정렬 완료 (개별 이동). 오프셋: {offset}");
        }
    }

    // 맵 초기화 함수 (연결선 삭제 추가)
    public void ClearMap()
    {
        // 1. 연결선 오브젝트 삭제
        foreach (GameObject lineObj in connectionLines)
        {
            if (lineObj != null)
            {
                #if UNITY_EDITOR
                DestroyImmediate(lineObj);
                #else
                Destroy(lineObj);
                #endif
            }
        }
        connectionLines.Clear();

        // 2. 게이트 오브젝트 삭제 (gateParent 자식 삭제 방식 유지)
        if (gateParent != null)
        {
            #if UNITY_EDITOR
            List<Transform> children = new List<Transform>();
            foreach (Transform child in gateParent)
            {
                children.Add(child);
            }
            foreach (Transform child in children)
            {
                DestroyImmediate(child.gameObject);
            }
            #else
            foreach (Transform child in gateParent)
            {
                Destroy(child.gameObject);
            }
            #endif
        }
        instantiatedGates.Clear();
    }

    // 연결선 생성 함수 (LineRenderer 사용)
    void CreateConnections(LogicCircuitData circuitData)
    {
        Debug.Log("연결선 생성을 시작합니다...");
        if (lineMaterial == null)
        {
            Debug.LogError("Line Material이 할당되지 않았습니다. 연결선을 생성할 수 없습니다.");
            return;
        }

        // 1. 모든 연결 정보를 미리 수집 (sourceX, targetX 기준으로 그룹화)
        var allConnections = new List<(string sourceId, string targetId, int inputIndex, Transform sourcePoint, Transform targetPoint, float sourceX, float targetX)>();

        // Hidden Layers
        foreach (var layer in circuitData.HiddenLayers)
        {
            foreach (var targetGateData in layer)
            {
                for (int inputIndex = 0; inputIndex < targetGateData.Inputs.Count; inputIndex++)
                {
                    string sourceId = targetGateData.Inputs[inputIndex];
                    if (!instantiatedGates.TryGetValue(sourceId, out GameObject sourceGateObject)) continue;
                    if (!instantiatedGates.TryGetValue(targetGateData.Id, out GameObject targetGateObject)) continue;
                    Transform sourcePoint = FindConnectionPoint(sourceGateObject, true, 0);
                    Transform targetPoint = FindConnectionPoint(targetGateObject, false, inputIndex);
                    if (sourcePoint == null || targetPoint == null) continue;
                    float sourceX = sourcePoint.position.x;
                    float targetX = targetPoint.position.x;
                    allConnections.Add((sourceId, targetGateData.Id, inputIndex, sourcePoint, targetPoint, sourceX, targetX));
                }
            }
        }
        // Output Gates
        foreach (var targetGateData in circuitData.OutputGates)
        {
            for (int inputIndex = 0; inputIndex < targetGateData.Inputs.Count; inputIndex++)
            {
                string sourceId = targetGateData.Inputs[inputIndex];
                if (!instantiatedGates.TryGetValue(sourceId, out GameObject sourceGateObject)) continue;
                if (!instantiatedGates.TryGetValue(targetGateData.Id, out GameObject targetGateObject)) continue;
                Transform sourcePoint = FindConnectionPoint(sourceGateObject, true, 0);
                Transform targetPoint = FindConnectionPoint(targetGateObject, false, inputIndex);
                if (sourcePoint == null || targetPoint == null) continue;
                float sourceX = sourcePoint.position.x;
                float targetX = targetPoint.position.x;
                allConnections.Add((sourceId, targetGateData.Id, inputIndex, sourcePoint, targetPoint, sourceX, targetX));
            }
        }

        // 2. (sourceX, targetX) 쌍별로 그룹화하여, 각 그룹 내에서 등분된 bendX를 할당
        var grouped = allConnections.GroupBy(c => (c.sourceX, c.targetX));
        foreach (var group in grouped)
        {
            var connList = group.ToList();
            int count = connList.Count;
            float minX = group.Key.sourceX;
            float maxX = group.Key.targetX;
            if (minX > maxX) { var tmp = minX; minX = maxX; maxX = tmp; }
            float margin = horizontalSpacing * 0.15f; // 구간 양쪽에 여유
            minX += margin;
            maxX -= margin;
            for (int i = 0; i < count; i++)
            {
                var (sourceId, targetId, inputIndex, sourcePoint, targetPoint, sourceX, targetX) = connList[i];
                Vector3 source = sourcePoint.position;
                Vector3 target = targetPoint.position;
                // 구간 내 등분된 bendX
                float t = (count == 1) ? 0.5f : (i + 1f) / (count + 1f);
                float bendX = Mathf.Lerp(minX, maxX, t);
                Vector3 bend1 = new Vector3(bendX, source.y, source.z);
                Vector3 bend2 = new Vector3(bendX, source.y, target.z);

                GameObject lineObj = new GameObject($"Connection_{sourceId}_to_{targetId}");
                lineObj.transform.SetParent(gateParent);
                LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(lineMaterial);
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;
                lineRenderer.startColor = lineColor;
                lineRenderer.endColor = lineColor;
                lineRenderer.positionCount = 4;
                lineRenderer.useWorldSpace = true;
                lineRenderer.SetPosition(0, source);
                lineRenderer.SetPosition(1, bend1);
                lineRenderer.SetPosition(2, bend2);
                lineRenderer.SetPosition(3, target);
                connectionLines.Add(lineObj);
            }
        }
        Debug.Log($"연결선 {connectionLines.Count}개 생성이 완료되었습니다.");
    }

    // 게이트 오브젝트에서 연결점 Transform 찾기 (In1, In2, Out 이름 사용)
    Transform FindConnectionPoint(GameObject gateObject, bool isOutput, int index)
    {
        string pointName;
        if (isOutput)
        {
            pointName = "Out";
        }
        else
        {
            // 인풋 인덱스 0 -> In1, 1 -> In2
            pointName = index == 0 ? "In1" : "In2";
        }
        Transform point = gateObject.transform.Find(pointName);

        if (point == null)
        {
            // 하위 자식까지 검색 (혹시 모를 계층 구조 대비)
            point = gateObject.GetComponentsInChildren<Transform>(true)
                              .FirstOrDefault(t => t.name == pointName);
            if (point == null)
            {
                Debug.LogError($"게이트 '{gateObject.name}'에서 연결점 '{pointName}'을(를) 찾을 수 없습니다. 프리팹의 자식 오브젝트 이름 규칙을 확인하세요.");
            }
        }
        return point;
    }

    // Update is called once per frame
    void Update()
    {
        // 현재는 비워둠
    }
}
