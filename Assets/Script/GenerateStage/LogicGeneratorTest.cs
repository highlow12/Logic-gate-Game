using UnityEngine;

// LogicGenerator 클래스의 기능을 테스트하기 위한 클래스입니다.
public class LogicGeneratorTest : MonoBehaviour
{
    // 테스트용 로직 회로
    private LogicCircuit testCircuit;
    // 생성된 진리표를 저장할 변수
    private TruthTable truthTable;
    // 전체 UI를 위한 스크롤 위치
    private Vector2 mainScrollPosition;
    // 로그 출력을 위한 스크롤 위치
    private Vector2 logScrollPosition;
    
    // 입력 게이트, 출력 게이트, 레이어 수, 레이어 크기를 위한 변수들
    private int inputCount = 2;
    private int outputCount = 1;
    private int layerCount = 1;
    private int layerSize = 3;

    // 입력 필드용 임시 문자열 변수 추가
    private string inputCountStr;
    private string outputCountStr;
    private string layerCountStr;
    private string layerSizeStr;
    
    // 콘솔에 표시할 로그 메시지
    private string logMessage = "";
    
    // 시작 시 초기화
    private void Start()
    {
        // 로그 메시지 초기화
        logMessage = "테스트를 시작하려면 버튼을 클릭하세요.";
        // 임시 문자열 변수 초기화
        inputCountStr = inputCount.ToString();
        outputCountStr = outputCount.ToString();
        layerCountStr = layerCount.ToString();
        layerSizeStr = layerSize.ToString();
    }
    
    // OnGUI 메소드를 사용하여 UI 요소 표시
    void OnGUI()
    {
        // 화면의 크기 계산
        float paddingPercent = 0.05f; // 화면 여백 5%
        float paddingWidth = Screen.width * paddingPercent;
        float paddingHeight = Screen.height * paddingPercent;
        float mainWidth = Screen.width - (paddingWidth * 2);
        float mainHeight = Screen.height - (paddingHeight * 2);
        
        // 메인 영역 정의
        Rect mainRect = new Rect(paddingWidth, paddingHeight, mainWidth, mainHeight);
        
        // 제목과 스크롤 영역을 배치할 전체 GUILayout 시작
        GUILayout.BeginArea(mainRect);
        
        // 제목 표시 (스크롤 밖에 위치)
        GUILayout.Label("LogicGenerator 테스트", GUI.skin.box, GUILayout.Height(30));
        
        // 메인 스크롤 영역 시작
        mainScrollPosition = GUILayout.BeginScrollView(mainScrollPosition, 
            GUILayout.Width(mainWidth), 
            GUILayout.Height(mainHeight - 40)); // 제목 높이 제외
        
        // 파라미터 섹션
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("파라미터 설정", GUI.skin.box);
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("입력 게이트 수:", GUILayout.Width(150));
        // 임시 문자열 변수 사용
        inputCountStr = GUILayout.TextField(inputCountStr, GUILayout.Width(50));
        if (int.TryParse(inputCountStr, out int newInputCount))
        {
            // 파싱 성공 시에만 정수 변수 업데이트 및 클램핑
            inputCount = Mathf.Clamp(newInputCount, 1, 8);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("출력 게이트 수:", GUILayout.Width(150));
        // 임시 문자열 변수 사용
        outputCountStr = GUILayout.TextField(outputCountStr, GUILayout.Width(50));
        if (int.TryParse(outputCountStr, out int newOutputCount))
        {
            outputCount = Mathf.Clamp(newOutputCount, 1, 8);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("레이어 수:", GUILayout.Width(150));
        // 임시 문자열 변수 사용
        layerCountStr = GUILayout.TextField(layerCountStr, GUILayout.Width(50));
        if (int.TryParse(layerCountStr, out int newLayerCount))
        {
            layerCount = Mathf.Clamp(newLayerCount, 1, 5);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("레이어 크기:", GUILayout.Width(150));
        // 임시 문자열 변수 사용
        layerSizeStr = GUILayout.TextField(layerSizeStr, GUILayout.Width(50));
        if (int.TryParse(layerSizeStr, out int newLayerSize))
        {
            layerSize = Mathf.Clamp(newLayerSize, 1, 10);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        
        // 기능 버튼 섹션
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("기능 테스트", GUI.skin.box);
        
        // 로직 회로 생성 버튼
        if (GUILayout.Button("로직 회로 생성 (GenerateLogic)"))
        {
            try
            {
                testCircuit = LogicGenerator.Instance.GenerateLogic(inputCount, outputCount, layerCount, layerSize);
                logMessage = $"로직 회로 생성 완료!\n입력: {inputCount}, 출력: {outputCount}, 레이어: {layerCount}, 레이어크기: {layerSize}";
                Debug.Log(logMessage);
            }
            catch (System.Exception e)
            {
                logMessage = "로직 회로 생성 실패: " + e.Message;
                Debug.LogError(logMessage);
            }
        }
        
        // 생성된 회로가 있을 때만 다른 기능 버튼 활성화
        GUI.enabled = testCircuit != null;
        
        // 진리표 생성 버튼
        if (GUILayout.Button("진리표 생성 (GenerateTruthTable)"))
        {
            try
            {
                truthTable = LogicGenerator.Instance.GenerateTruthTable(testCircuit);
                logMessage = "진리표 생성 완료!\n" + truthTable.ToString();
                Debug.Log("진리표 생성 완료!");
            }
            catch (System.Exception e)
            {
                logMessage = "진리표 생성 실패: " + e.Message;
                Debug.LogError(logMessage);
            }
        }
        
        // 난이도 계산 버튼
        if (GUILayout.Button("난이도 계산 (GetDifficulty)"))
        {
            try
            {
                int difficulty = LogicGenerator.Instance.GetDifficulty(testCircuit);
                logMessage = $"계산된 난이도: {difficulty}";
                Debug.Log(logMessage);
            }
            catch (System.Exception e)
            {
                logMessage = "난이도 계산 실패: " + e.Message;
                Debug.LogError(logMessage);
            }
        }
        
        // JSON 변환 버튼
        if (GUILayout.Button("JSON 변환 (GetLogicJSON)"))
        {
            try
            {
                // 저장 경로 생성 (Assets 폴더 내에 LogicCircuit_{현재시간}.json 형식)
                string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"LogicCircuit_{timestamp}.json";
                string savePath = System.IO.Path.Combine(Application.dataPath, fileName);
                
                // JSON 변환 및 파일 저장
                object json = LogicGenerator.Instance.GetLogicJSON(testCircuit, savePath);
                
                // 성공 메시지 표시
                logMessage = $"JSON 변환 및 파일 저장 완료!\n저장 경로: {savePath}\n\n{json?.ToString()}";
                Debug.Log($"JSON 파일 저장 완료: {savePath}");
            }
            catch (System.Exception e)
            {
                logMessage = "JSON 변환 실패: " + e.Message;
                Debug.LogError(logMessage);
            }
        }
        
        // 불 대수 표현식 생성 버튼
        if (GUILayout.Button("불 대수 표현식 생성 (GenerateBooleanExpressions)")) // 메서드 이름 변경
        {
            try
            {
                // GenerateBooleanExpressions 메서드 호출
                System.Collections.Generic.List<string> expressions = LogicGenerator.Instance.GenerateBooleanExpressions(testCircuit);
                
                // 결과 문자열 생성
                string expressionsText = "생성된 불 대수 표현식:\n";
                for(int i = 0; i < expressions.Count; i++)
                {
                    expressionsText += $"출력 {i}: {expressions[i]}\n";
                }
                
                logMessage = expressionsText;
                Debug.Log("불 대수 표현식 생성 완료!");
            }
            catch (System.Exception e)
            {
                logMessage = "불 대수 표현식 생성 실패: " + e.Message;
                Debug.LogError(logMessage);
            }
        }
        
        // 버튼 활성화 상태 복원
        GUI.enabled = true;
        
        GUILayout.EndVertical();
        
        // 로그 출력 섹션
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("로그 출력", GUI.skin.box);
        
        // 스크롤 가능한 텍스트 영역
        logScrollPosition = GUILayout.BeginScrollView(logScrollPosition, GUILayout.Height(200));
        GUILayout.TextArea(logMessage, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
        
        GUILayout.EndVertical();
        
        // 메인 스크롤 영역 종료
        GUILayout.EndScrollView();
        
        // 전체 GUILayout 종료
        GUILayout.EndArea();
    }
}