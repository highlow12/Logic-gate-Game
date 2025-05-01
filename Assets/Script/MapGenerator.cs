using UnityEngine;
using System.Collections.Generic; // Required for Dictionary and List
using System.IO; // Required for Path if using file paths directly

public class MapGenerator : MonoBehaviour
{
    [Header("Data Source")]
    public TextAsset jsonFile; // Inspector에서 할당할 JSON 파일

    [Header("Layout Settings")]
    public float horizontalSpacing = 5.0f; // 열(레이어) 간 가로 간격
    public float verticalSpacing = 2.0f;   // 행(게이트) 간 세로 간격
    public Transform gateParent; // 생성된 게이트들을 담을 부모 Transform (선택적)

    // Prefab 경로 설정 (Resources 폴더 기준)
    private const string PREFAB_FOLDER_PATH = "Prefabs/";
    private const string CONNECTION_LINE_PREFAB_NAME = "ConnectionLine"; // 연결선 프리팹 이름

    // 미리 로드된 프리팹을 저장할 딕셔너리
    private Dictionary<string, GameObject> loadedPrefabs = new Dictionary<string, GameObject>();
    // 로드해야 할 기본 프리팹 타입 목록
    private List<string> requiredPrefabTypes = new List<string>
    {
        "INPUT", "OUTPUT", "AND", "OR", "NOT", "XOR", "WIRE", CONNECTION_LINE_PREFAB_NAME
    };

    // 생성된 게이트 오브젝트 관리 (ID -> GameObject)
    private Dictionary<string, GameObject> instantiatedGates = new Dictionary<string, GameObject>();
    private LogicCircuitSerializer serializer = new LogicCircuitSerializer(); // Serializer 인스턴스

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

        // 1. JSON 로드 및 파싱
        LogicCircuitData circuitData = serializer.LoadLogicCircuitFromJson(jsonFile.text);
        if (circuitData == null)
        {
            Debug.LogError("JSON 데이터를 로드하거나 파싱하는 데 실패했습니다.");
            return;
        }

        instantiatedGates.Clear(); // 딕셔너리 초기화

        // 2. 게이트 오브젝트 생성 및 배치
        // Input Gates
        for (int i = 0; i < circuitData.InputGates.Count; i++)
        {
            PlaceGate(circuitData.InputGates[i], 0, i);
        }

        // Hidden Layers
        for (int layerIndex = 0; layerIndex < circuitData.HiddenLayers.Count; layerIndex++)
        {
            List<GateData> layer = circuitData.HiddenLayers[layerIndex];
            for (int gateIndex = 0; gateIndex < layer.Count; gateIndex++)
            {
                // 열 인덱스는 layerIndex + 1 (Input 다음부터 시작)
                PlaceGate(layer[gateIndex], layerIndex + 1, gateIndex);
            }
        }

        // Output Gates
        // 출력 게이트의 열 인덱스는 히든 레이어 개수 + 1
        int outputLayerIndex = circuitData.HiddenLayers.Count + 1;
        for (int i = 0; i < circuitData.OutputGates.Count; i++)
        {
            PlaceGate(circuitData.OutputGates[i], outputLayerIndex, i);
        }

        // 3. 연결선 생성 (다음 단계에서 구현)
        // CreateConnections(circuitData);

        Debug.Log("맵 생성이 완료되었습니다.");
    }

    // 게이트 하나를 배치하는 함수
    void PlaceGate(GateData gateData, int layerIndex, int gateIndexInLayer)
    {
        // 위치 계산
        float xPos = layerIndex * horizontalSpacing;
        // Y 위치는 위에서 아래로 배치 (음수 값 사용)
        float yPos = -gateIndexInLayer * verticalSpacing;
        Vector3 position = new Vector3(xPos, yPos, 0);

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

    // 맵 초기화 함수 (선택적)
    public void ClearMap()
    {
        // 방법 1: Dictionary에 저장된 오브젝트만 삭제
        foreach (var pair in instantiatedGates)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value);
            }
        }
        instantiatedGates.Clear();

        // 방법 2: gateParent의 모든 자식 오브젝트 삭제 (gateParent 사용 시)
        // if (gateParent != null)
        // {
        //     foreach (Transform child in gateParent)
        //     {
        //         Destroy(child.gameObject);
        //     }
        // }
        // instantiatedGates.Clear(); // 딕셔너리도 비워야 함
    }

    // 연결선 생성 함수 (다음 단계에서 구현)
    // void CreateConnections(LogicCircuitData circuitData)
    // {
    //     // 미리 로드된 연결선 프리팹 가져오기
    //     if (!loadedPrefabs.TryGetValue(CONNECTION_LINE_PREFAB_NAME.ToUpper(), out GameObject linePrefab) || linePrefab == null)
    //     {
    //          Debug.LogError($"연결선 프리팹 '{CONNECTION_LINE_PREFAB_NAME}'을 미리 로드하지 못했습니다.");
    //          return;
    //     }
    //     // ... 연결선 생성 로직 ...
    // }

    // Update is called once per frame
    void Update()
    {
        // 현재는 비워둠
    }
}
