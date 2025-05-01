using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json; // JSON.NET 사용
using System.Text; // For StringBuilder

// JSON 직렬화를 위한 데이터 구조: 로직 회로 전체
[System.Serializable]
public class LogicCircuitData
{
    public List<GateData> InputGates = new List<GateData>();
    public List<List<GateData>> HiddenLayers = new List<List<GateData>>();
    public List<GateData> OutputGates = new List<GateData>();
}

// JSON 직렬화를 위한 데이터 구조: 개별 게이트
[System.Serializable]
public class GateData
{
    public string Id; // 게이트 고유 ID (예: "input_0", "hidden_1_2")
    public string Type; // 게이트 타입 문자열 (예: "AND", "NOT")
    public List<string> Inputs = new List<string>(); // 이 게이트에 연결된 이전 게이트들의 ID 목록
}

// 로직 회로의 JSON 직렬화 및 관련 유틸리티를 담당하는 클래스
public class LogicCircuitSerializer
{
    // 로직 회로를 JSON 문자열로 변환하고, 선택적으로 파일에 저장합니다.
    public string GetLogicJSON(LogicCircuit circuit, string savePath = null)
    {
        if (circuit == null)
        {
            throw new System.ArgumentNullException(nameof(circuit), "LogicCircuit cannot be null.");
        }

        LogicCircuitData circuitData = new LogicCircuitData();

        // 입력 게이트 정보 변환
        for (int i = 0; i < circuit.InputGates.Count; i++)
        {
            var gate = circuit.InputGates[i];
            circuitData.InputGates.Add(new GateData
            {
                Id = DetermineGateId(gate, circuit), // ID 결정 헬퍼 사용
                Type = GetGateTypeName(gate) // 타입 이름 헬퍼 사용
                // Inputs는 비어있음
            });
        }

        // 출력 게이트 정보 변환
        for (int i = 0; i < circuit.OutputGates.Count; i++)
        {
             var gate = circuit.OutputGates[i];
             var connectedGates = new List<string>();
             if (gate.PreviousGates != null)
             {
                 for (int j = 0; j < gate.InputCount; j++)
                 {
                     if (j < gate.PreviousGates.Length && gate.PreviousGates[j] != null)
                     {
                         connectedGates.Add(DetermineGateId(gate.PreviousGates[j], circuit));
                     }
                     else
                     {
                         Debug.LogWarning($"Output gate {DetermineGateId(gate, circuit)} input {j} is not connected.");
                     }
                 }
             }
             circuitData.OutputGates.Add(new GateData
             {
                 Id = DetermineGateId(gate, circuit),
                 Type = GetGateTypeName(gate), // 출력 게이트 타입 명시 (예: WIRE)
                 Inputs = connectedGates
             });
        }

        // 히든 레이어 정보 변환
        circuitData.HiddenLayers = new List<List<GateData>>();
        for (int layerIdx = 0; layerIdx < circuit.HiddenLayers.Count; layerIdx++)
        {
            var layer = circuit.HiddenLayers[layerIdx];
            var layerData = new List<GateData>();
            for (int gateIdx = 0; gateIdx < layer.Count; gateIdx++)
            {
                var gate = layer[gateIdx];
                var connectedGates = new List<string>();
                 if (gate.PreviousGates != null)
                 {
                    for (int j = 0; j < gate.InputCount; j++)
                    {
                        if (j < gate.PreviousGates.Length && gate.PreviousGates[j] != null)
                        {
                            connectedGates.Add(DetermineGateId(gate.PreviousGates[j], circuit));
                        }
                        else
                        {
                            Debug.LogWarning($"Hidden gate {DetermineGateId(gate, circuit)} input {j} is not connected.");
                        }
                    }
                 }

                layerData.Add(new GateData
                {
                    Id = DetermineGateId(gate, circuit),
                    Type = GetGateTypeName(gate),
                    Inputs = connectedGates
                });
            }
            if (layerData.Count > 0)
            {
                 circuitData.HiddenLayers.Add(layerData);
            }
        }

        // JSON 직렬화 (들여쓰기 포함)
        string jsonString = JsonConvert.SerializeObject(circuitData, Formatting.Indented);

        // 파일 저장 (경로가 제공된 경우)
        if (!string.IsNullOrEmpty(savePath))
        {
            try
            {
                System.IO.File.WriteAllText(savePath, jsonString);
                Debug.Log($"JSON 파일이 성공적으로 저장되었습니다: {savePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JSON 파일 저장 중 오류 발생: {e.Message}");
            }
        }

        return jsonString; // JSON 문자열 반환
    }

    // --- Helper Methods ---

    // 게이트 타입 이름을 문자열로 반환
    public string GetGateTypeName(LogicGate gate)
    {
        if (gate is AND) return "AND";
        if (gate is OR) return "OR";
        if (gate is NOT) return "NOT";
        if (gate is XOR) return "XOR";
        if (gate is WIRE) return "WIRE";
        if (gate is InputGate) return "INPUT";
        // OutputGate 클래스가 별도로 있다면 추가
        // if (gate is OutputGate) return "OUTPUT"; // 현재는 WIRE를 출력으로 사용
        return "UNKNOWN";
    }

    // 게이트의 고유 ID 결정 (회로 내 위치 기반)
    public string DetermineGateId(LogicGate gate, LogicCircuit circuit)
    {
        if (gate == null || circuit == null) return "unknown_null";

        // 입력 게이트 확인
        for (int i = 0; i < circuit.InputGates.Count; i++)
        {
            if (circuit.InputGates[i] == gate) return $"input_{i}";
        }

        // 히든 레이어 확인
        for (int layerIdx = 0; layerIdx < circuit.HiddenLayers.Count; layerIdx++)
        {
            for (int gateIdx = 0; gateIdx < circuit.HiddenLayers[layerIdx].Count; gateIdx++)
            {
                if (circuit.HiddenLayers[layerIdx][gateIdx] == gate) return $"hidden_{layerIdx}_{gateIdx}";
            }
        }

        // 출력 게이트 확인
        for (int i = 0; i < circuit.OutputGates.Count; i++)
        {
            if (circuit.OutputGates[i] == gate) return $"output_{i}";
        }

        Debug.LogWarning($"Could not determine ID for gate of type {gate.GetType().Name}");
        return "unknown_not_found";
    }

    #if UNITY_EDITOR
    // 디버깅용 회로 구조 문자열 생성 (에디터 전용)
    public string DebugCircuitStructure(LogicCircuit circuit)
    {
        if (circuit == null) return "Circuit is null.";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== 로직 회로 구조 디버그 ===");

        sb.AppendLine($"\n입력 게이트 ({circuit.InputGates.Count}개):");
        foreach (var gate in circuit.InputGates)
            sb.AppendLine($"  ID={DetermineGateId(gate, circuit)}, 타입={GetGateTypeName(gate)}");

        sb.AppendLine($"\n히든 레이어 ({circuit.HiddenLayers.Count}개):");
        for (int i = 0; i < circuit.HiddenLayers.Count; i++)
        {
            sb.AppendLine($"  레이어 #{i} ({circuit.HiddenLayers[i].Count}개):");
            foreach (var gate in circuit.HiddenLayers[i])
            {
                sb.Append($"    ID={DetermineGateId(gate, circuit)}, 타입={GetGateTypeName(gate)}, 입력=[");
                if (gate.PreviousGates != null)
                    for(int j=0; j<gate.PreviousGates.Length; ++j)
                        sb.Append($"{(j > 0 ? ", " : "")}{DetermineGateId(gate.PreviousGates[j], circuit)}");
                sb.AppendLine("]");
            }
        }

        sb.AppendLine($"\n출력 게이트 ({circuit.OutputGates.Count}개):");
         foreach (var gate in circuit.OutputGates)
         {
             sb.Append($"  ID={DetermineGateId(gate, circuit)}, 타입={GetGateTypeName(gate)}, 입력=[");
             if (gate.PreviousGates != null)
                 for(int j=0; j<gate.PreviousGates.Length; ++j)
                     sb.Append($"{(j > 0 ? ", " : "")}{DetermineGateId(gate.PreviousGates[j], circuit)}");
             sb.AppendLine("]");
         }

        return sb.ToString();
    }
    #endif
}
