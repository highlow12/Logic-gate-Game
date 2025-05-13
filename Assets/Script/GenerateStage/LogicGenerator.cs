using UnityEngine;
using System.Collections.Generic; // Required for Lists
using System; // Required for System.Random
using System.Linq; // Required for LINQ methods like OrderBy
using Newtonsoft.Json; // Required for JSON serialization

// 진리표를 표현하기 위한 클래스입니다
public class TruthTable
{
    // 입력 조합 목록
    public List<bool[]> Inputs { get; private set; }
    
    // 각 입력 조합에 대한 출력값 목록
    public List<bool[]> Outputs { get; private set; }
    
    // 입력 게이트의 수
    public int InputCount { get; private set; }
    
    // 출력 게이트의 수
    public int OutputCount { get; private set; }
    
    // 진리표 생성자
    public TruthTable(int inputCount, int outputCount)
    {
        InputCount = inputCount;
        OutputCount = outputCount;
        Inputs = new List<bool[]>();
        Outputs = new List<bool[]>();
    }
    
    // 진리표에 입력과 출력 조합 추가
    public void AddRow(bool[] input, bool[] output)
    {
        if (input.Length != InputCount || output.Length != OutputCount)
        {
            Debug.LogError($"입출력 크기가 맞지 않습니다. 입력: {input.Length}/{InputCount}, 출력: {output.Length}/{OutputCount}");
            return;
        }
        
        Inputs.Add(input);
        Outputs.Add(output);
    }
    
    // 진리표를 문자열로 변환
    public override string ToString()
    {
        string result = "| ";
        
        // 입력 열 헤더
        for (int i = 0; i < InputCount; i++)
        {
            result += $"In{i} | ";
        }
        
        // 출력 열 헤더
        for (int i = 0; i < OutputCount; i++)
        {
            result += $"Out{i} | ";
        }
        
        result += "\n";
        
        // 구분선
        for (int i = 0; i < InputCount + OutputCount; i++)
        {
            result += "|----|";
        }
        
        result += "\n";
        
        // 데이터 행
        for (int i = 0; i < Inputs.Count; i++)
        {
            result += "| ";
            
            // 입력 값
            for (int j = 0; j < InputCount; j++)
            {
                result += $" {(Inputs[i][j] ? "1" : "0")}  | ";
            }
            
            // 출력 값
            for (int j = 0; j < OutputCount; j++)
            {
                result += $" {(Outputs[i][j] ? "1" : "0")}  | ";
            }
            
            result += "\n";
        }
        
        return result;
    }
}

// 로직 게이트 회로를 생성하고 관리하는 싱글톤 클래스입니다.
// 다양한 로직 게이트를 사용하여 회로를 생성하고 계산합니다.
public class LogicGenerator : Singleton<LogicGenerator>
{
    #region 로직 게이트 타입 정의
    // 로직 게이트 타입을 정의하는 열거형입니다.
    public enum LogicGateType
    {
        // AND 논리 게이트
        AND,
        // OR 논리 게이트
        OR,
        // NOT 논리 게이트
        NOT,
        // XOR 논리 게이트
        XOR,
        // WIRE 게이트(버퍼)
        WIRE,
    }
    #endregion

    #region 로직 회로 생성
    // 로직 회로를 생성합니다.
    // 매개변수:
    //   inputCount: 입력 게이트의 개수
    //   outputCount: 출력 게이트의 개수
    //   layerCount: 회로의 층 개수
    //   layerSize: 각 층의 게이트 개수
    // 반환값: 생성된 로직 회로
    public LogicCircuit GenerateLogic(int inputCount, int outputCount, int layerCount, int layerSize)
    {
        LogicCircuit circuit = new LogicCircuit();
        System.Random random = new System.Random();
        var enumValues = System.Enum.GetValues(typeof(LogicGateType));

        // 1. Input Gate 생성
        List<LogicGate> currentLayerOutputs = new List<LogicGate>();
        for (int i = 0; i < inputCount; i++)
        {
            InputGate inputGate = new InputGate();
            circuit.InputGates.Add(inputGate);
            currentLayerOutputs.Add(inputGate); // 입력 게이트를 첫 레이어의 출력으로 시작
        }

        // 2. Hidden Layer 생성 (layerCount 개, 각 레이어 크기는 layerSize)
        for (int layerIndex = 0; layerIndex < layerCount; layerIndex++)
        {
            List<LogicGate> nextLayerGates = new List<LogicGate>();
            for (int gateIndex = 0; gateIndex < layerSize; gateIndex++)
            {
                // 무작위 게이트 타입 선택
                LogicGateType randomType = (LogicGateType)enumValues.GetValue(random.Next(0, enumValues.Length));
                LogicGate gate = CreateGateByType(randomType);
                nextLayerGates.Add(gate);
            }
            ConnectGateInputs(nextLayerGates, currentLayerOutputs, random); // 새 레이어의 입력을 이전 레이어의 출력에 연결
            circuit.HiddenLayers.Add(nextLayerGates); // 새 레이어를 회로에 추가
            currentLayerOutputs = nextLayerGates; // 다음 반복을 위해 현재 레이어 출력을 업데이트
        }

        // 3. Intermediate Layer 생성 (마지막 히든 레이어 크기 > outputCount 인 경우)
        while (currentLayerOutputs.Count > outputCount)
        {
            // 다음 레이어 크기 계산 (현재 크기의 절반, 올림)
            int nextLayerSize = (int)Math.Ceiling(currentLayerOutputs.Count / 2.0);
            // 만약 계산된 크기가 outputCount보다 작아지면 outputCount로 설정 (이론상 발생하지 않아야 함)
            nextLayerSize = Math.Max(outputCount, nextLayerSize);

            List<LogicGate> nextLayerGates = new List<LogicGate>();
            for (int gateIndex = 0; gateIndex < nextLayerSize; gateIndex++)
            {
                 LogicGateType randomType = (LogicGateType)enumValues.GetValue(random.Next(0, enumValues.Length));
                
                while(randomType is LogicGateType.WIRE || randomType is LogicGateType.NOT)
                {
                    // 마지막 게이트에는 wire과 not이 오지 않습니다
                    randomType = (LogicGateType)enumValues.GetValue(random.Next(0, enumValues.Length));
                }
                 LogicGate gate = CreateGateByType(randomType);
                 nextLayerGates.Add(gate);
            }
            ConnectGateInputs(nextLayerGates, currentLayerOutputs, random); // 중간 레이어 연결
            circuit.HiddenLayers.Add(nextLayerGates); // 중간 레이어를 히든 레이어 목록에 추가
            currentLayerOutputs = nextLayerGates; // 다음 중간 레이어 생성을 위해 업데이트
        }

        // 4. Output Gate 생성 및 최종 연결
        List<LogicGate> outputGates = new List<LogicGate>();
        for (int i = 0; i < outputCount; i++)
        {
            // 출력 게이트는 일반적으로 WIRE 타입 사용
            LogicGate outputGate = CreateGateByType(LogicGateType.WIRE);
            outputGates.Add(outputGate);
            circuit.OutputGates.Add(outputGate);
        }
        // 최종 출력 게이트들의 입력을 마지막 hidden/intermediate 레이어의 출력에 연결
        ConnectGateInputs(outputGates, currentLayerOutputs, random);

        // (선택적) 모든 게이트를 AllGates 리스트에 추가
        circuit.AllGates.AddRange(circuit.InputGates);
        foreach(var layer in circuit.HiddenLayers)
        {
            circuit.AllGates.AddRange(layer);
        }
        circuit.AllGates.AddRange(circuit.OutputGates);

        return circuit;
    }

    // 타입에 따른 게이트 인스턴스를 생성합니다.
    // 매개변수:
    //   type: 생성할 게이트의 타입
    // 반환값: 생성된 로직 게이트
    // 예외:
    //   System.ArgumentException: 유효하지 않은 게이트 타입이 지정된 경우 발생
    private LogicGate CreateGateByType(LogicGateType type)
    {
        switch (type)
        {
            case LogicGateType.AND: return new AND();
            case LogicGateType.OR: return new OR();
            case LogicGateType.NOT: return new NOT();
            case LogicGateType.XOR: return new XOR();
            case LogicGateType.WIRE: return new WIRE();
            default:
                throw new System.ArgumentException("Invalid gate type specified: " + type);
        }
    }

    // 게이트 입력을 이전 레이어의 출력에 무작위로 연결합니다.
    // 수정: 한 게이트 내에서 입력 소스가 중복되지 않도록 시도하고, 모든 입력 연결을 보장합니다.
    // 매개변수:
    //   currentLayerGates: 연결할 현재 레이어의 게이트 목록
    //   previousLayerOutputs: 이전 레이어의 게이트 목록 (출력 소스)
    //   random: 무작위 수 생성기
    private void ConnectGateInputs(List<LogicGate> currentLayerGates, List<LogicGate> previousLayerOutputs, System.Random random)
    {
        var connectNeeded = previousLayerOutputs.OrderBy(x => random.Next()).ToList();

        foreach(var gate in currentLayerGates)
        {
            // InputGate는 입력을 받지 않으므로 건너뜁니다.
            if (gate is InputGate) continue;

            int inputNeeded = gate.InputCount;
            // 입력이 필요 없는 게이트(예: 일부 특수 게이트)는 건너뜁니다.
            if (inputNeeded == 0) continue;

            for (int i = 0; i < gate.InputCount; i++)
            {
                // 연결할 소스 게이트를 무작위로 선택
                LogicGate sourceGate = connectNeeded[0];
                // ConnectInput 메서드를 호출하여 연결 시도
                if (!gate.ConnectInput(i, sourceGate))
                {
                    // 연결 실패 시 경고 로그
                    Debug.LogWarning($"게이트 {DetermineGateId(gate, null)}의 입력 {i}를 게이트 {DetermineGateId(sourceGate, null)}에 연결하는 데 실패했습니다.");
                }
                // 연결된 소스 게이트를 목록에서 제거
                connectNeeded.RemoveAt(0); // 연결된 소스 제거

                if (connectNeeded.Count == 0)
                {
                    connectNeeded = previousLayerOutputs.OrderBy(x => random.Next()).ToList();
                }
            }
        }
    }
    #endregion
    
    #region 진리표 생성
    // 로직 회로의 진리표를 생성합니다.
    // 매개변수:
    //   logic: 진리표를 생성할 로직 회로
    // 반환값: 생성된 진리표 (TruthTable 객체)
    public TruthTable GenerateTruthTable(LogicCircuit logic)
    {
        int inputCount = logic.InputGates.Count;
        int outputCount = logic.OutputGates.Count;

        // 진리표 객체 생성
        TruthTable truthTable = new TruthTable(inputCount, outputCount);

        // 가능한 모든 입력 조합 수 계산 (2^n)
        int totalCombinations = 1 << inputCount;

        // 모든 가능한 입력 조합에 대해 반복
        for (int i = 0; i < totalCombinations; i++)
        {
            // 현재 조합에 대한 입력 배열 생성
            bool[] currentInputs = new bool[inputCount];

            // i의 이진 표현을 사용하여 현재 입력 조합 설정 및 입력 게이트 값 설정
            for (int j = 0; j < inputCount; j++)
            {
                // i의 j번째 비트가 1인지 확인
                currentInputs[j] = ((i >> j) & 1) == 1;

                // 입력 게이트의 값을 설정 (상태 초기화는 SetValue 내부 또는 State 접근 시 처리될 수 있음)
                // InputGate는 ResetState가 필요 없을 수 있지만, 명확성을 위해 호출하거나 SetValue가 처리하도록 보장
                logic.InputGates[j].ResetState(); // InputGate의 상태도 명시적으로 리셋
                ((InputGate)logic.InputGates[j]).SetValue(currentInputs[j]);
            }

            // 중요: 현재 입력 조합에 대한 출력을 계산하기 전에
            // 모든 히든 레이어 및 출력 게이트의 상태를 초기화합니다.
            // 이렇게 하면 이전 입력 조합의 계산 결과가 다음 계산에 영향을 주지 않습니다.
            foreach (var layer in logic.HiddenLayers)
            {
                foreach (var gate in layer)
                {
                    gate.ResetState();
                }
            }
            foreach (var gate in logic.OutputGates)
            {
                gate.ResetState();
            }


            // 현재 입력에 대한 출력 계산
            bool[] currentOutputs = new bool[outputCount];

            for (int j = 0; j < outputCount; j++)
            {
                // 출력 게이트의 상태를 가져옵니다.
                // State 프로퍼티 getter는 필요에 따라 재귀적으로 상태를 계산해야 합니다.
                bool? outputState = logic.OutputGates[j].State;

                // null 체크 후 결과 저장
                if (outputState.HasValue)
                {
                    currentOutputs[j] = outputState.Value;
                }
                else
                {
                    // 상태 계산에 실패했거나 로직 오류가 있을 경우
                    Debug.LogWarning($"출력 게이트 {j}의 상태를 계산할 수 없습니다 (결과: null). 입력 조합: {i}");
                    currentOutputs[j] = false; // 기본값 또는 오류 값으로 처리
                }
            }

            // 진리표에 현재 행 추가
            truthTable.AddRow(currentInputs, currentOutputs);
        }

        // 구조화된 TruthTable 객체 반환
        return truthTable;
    }
    #endregion

    #region 난이도 계산
    // 로직 회로의 난이도를 계산합니다.
    // 각 게이트 타입에 따라 미리 정의된 점수를 합산합니다.
    // 매개변수:
    //   logic: 난이도를 계산할 로직 회로 (LogicCircuit 타입이어야 함)
    // 반환값: 계산된 총 난이도. 유효하지 않은 입력 시 -1 반환.
    // => 중복 구현 제거: DifficultyCalculator 클래스를 사용하세요.
    // public int GetDifficulty(object logic) { ... }
    #endregion

    #region 부울 대수 식 생성
    // 로직 회로를 부울 대수 식으로 변환합니다.
    // => 중복 구현 제거: BooleanExpressionGenerator 클래스를 사용하세요.
    // public List<string> GenerateBooleanExpressions(LogicCircuit circuit) { ... }
    #endregion

    #region JSON 변환 및 직렬화
    // 로직 회로를 JSON 형식으로 변환합니다.
    // => 중복 구현 제거: LogicCircuitSerializer 클래스를 사용하세요.
    // public object GetLogicJSON(object logic, string savePath = null) { ... }
    #endregion

    #if UNITY_EDITOR
    #region 디버깅 유틸리티
    // 회로 구조를 디버그하기 위한 메서드
    private string DebugCircuitStructure(LogicCircuit circuit)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        sb.AppendLine("=== 로직 회로 구조 디버그 ===");
        
        // 입력 게이트 표시
        sb.AppendLine($"\n입력 게이트 ({circuit.InputGates.Count}개):");
        for (int i = 0; i < circuit.InputGates.Count; i++)
        {
            var gate = circuit.InputGates[i];
            sb.AppendLine($"  입력 #{i}: 타입={gate.GetType().Name}, 입력 수={gate.InputCount}");
        }
        
        // 히든 레이어 표시
        sb.AppendLine($"\n히든 레이어 ({circuit.HiddenLayers.Count}개):");
        for (int layerIdx = 0; layerIdx < circuit.HiddenLayers.Count; layerIdx++)
        {
            var layer = circuit.HiddenLayers[layerIdx];
            sb.AppendLine($"  레이어 #{layerIdx} ({layer.Count}개 게이트):");
            
            for (int gateIdx = 0; gateIdx < layer.Count; gateIdx++)
            {
                var gate = layer[gateIdx];
                sb.Append($"    게이트 #{gateIdx}: 타입={gate.GetType().Name}, 입력 수={gate.InputCount}");
                
                // 이 게이트의 입력 연결 표시
                sb.Append(" 입력=[");
                for (int i = 0; i < gate.InputCount; i++)
                {
                    var prevGate = gate.PreviousGates[i];
                    string prevGateId = prevGate != null ? DetermineGateId(prevGate, circuit) : "null";
                    sb.Append($"{prevGateId}");
                    if (i < gate.InputCount - 1) sb.Append(", ");
                }
                sb.AppendLine("]");
            }
        }
        
        // 출력 게이트 표시
        sb.AppendLine($"\n출력 게이트 ({circuit.OutputGates.Count}개):");
        for (int i = 0; i < circuit.OutputGates.Count; i++)
        {
            var gate = circuit.OutputGates[i];
            sb.Append($"  출력 #{i}: 타입={gate.GetType().Name}, 입력 수={gate.InputCount}");
            
            // 이 게이트의 입력 연결 표시
            sb.Append(" 입력=[");
            for (int j = 0; j < gate.InputCount; j++)
            {
                var prevGate = gate.PreviousGates[j];
                string prevGateId = prevGate != null ? DetermineGateId(prevGate, circuit) : "null";
                sb.Append($"{prevGateId}");
                if (j < gate.InputCount - 1) sb.Append(", ");
            }
            sb.AppendLine("]");
        }
        
        return sb.ToString();
    }
    
    // 게이트 타입 이름 반환
    private string GetGateTypeName(LogicGate gate)
    {
        if (gate is AND) return "AND";
        if (gate is OR) return "OR";
        if (gate is NOT) return "NOT";
        if (gate is XOR) return "XOR";
        if (gate is WIRE) return "WIRE";
        if (gate is InputGate) return "INPUT";
        
        return "UNKNOWN";
    }
    
    // 게이트의 ID 결정
    private string DetermineGateId(LogicGate gate, LogicCircuit circuit)
    {
        // 입력 게이트인 경우
        for (int i = 0; i < circuit.InputGates.Count; i++)
        {
            if (circuit.InputGates[i] == gate)
            {
                return $"input_{i}";
            }
        }
        
        // 히든 레이어 게이트인 경우
        for (int layerIdx = 0; layerIdx < circuit.HiddenLayers.Count; layerIdx++)
        {
            var layer = circuit.HiddenLayers[layerIdx];
            for (int gateIdx = 0; gateIdx < layer.Count; gateIdx++)
            {
                if (layer[gateIdx] == gate)
                {
                    return $"hidden_{layerIdx}_{gateIdx}";
                }
            }
        }
        
        // 출력 게이트인 경우
        for (int i = 0; i < circuit.OutputGates.Count; i++)
        {
            if (circuit.OutputGates[i] == gate)
            {
                return $"output_{i}";
            }
        }
        
        return "unknown";
    }
    #endregion
    #endif
}

// 모든 로직 게이트의 기본 클래스입니다.
// 논리 연산을 수행하고 게이트 간 연결을 관리합니다.
