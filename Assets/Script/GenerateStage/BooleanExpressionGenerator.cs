using UnityEngine;
using System.Collections.Generic;

// 부울 대수 식 생성을 담당하는 클래스
public class BooleanExpressionGenerator
{
    // ID 결정을 위해 Serializer 인스턴스가 필요
    private LogicCircuitSerializer serializer;

    // 생성자에서 Serializer 주입
    public BooleanExpressionGenerator(LogicCircuitSerializer circuitSerializer)
    {
        this.serializer = circuitSerializer;
    }

    // 로직 회로의 부울 대수 식 목록을 생성합니다.
    public List<string> GenerateBooleanExpressions(LogicCircuit circuit)
    {
        if (circuit == null || circuit.OutputGates == null || circuit.InputGates == null || serializer == null)
        {
            Debug.LogError("GenerateBooleanExpressions: Invalid arguments provided.");
            return new List<string>();
        }

        List<string> expressions = new List<string>();
        // 메모이제이션: 이미 계산된 게이트 식 저장
        Dictionary<LogicGate, string> memo = new Dictionary<LogicGate, string>();
        // 입력 게이트 ID를 A, B, C... 로 매핑
        Dictionary<string, string> inputNameMap = new Dictionary<string, string>();

        for (int i = 0; i < circuit.InputGates.Count; i++)
        {
            string gateId = serializer.DetermineGateId(circuit.InputGates[i], circuit);
            inputNameMap.Add(gateId, ((char)(65 + i)).ToString()); // A, B, C...
        }

        // 각 출력 게이트에 대해 식 생성
        foreach (var outputGate in circuit.OutputGates)
        {
            expressions.Add(GetGateExpression(outputGate, circuit, memo, inputNameMap));
        }
        return expressions;
    }

    // 특정 게이트의 부울 대수 식을 재귀적으로 계산 (메모이제이션 사용)
    private string GetGateExpression(LogicGate gate, LogicCircuit circuit, Dictionary<LogicGate, string> memo, Dictionary<string, string> inputNameMap)
    {
        if (gate == null) return "ERROR_NULL_GATE";
        if (memo.ContainsKey(gate)) return memo[gate]; // 메모된 값 반환

        string expression = "";
        string gateId = serializer.DetermineGateId(gate, circuit); // ID 결정

        // 게이트 타입별 처리
        if (gate is InputGate)
        {
            expression = inputNameMap.ContainsKey(gateId) ? inputNameMap[gateId] : gateId;
        }
        else if (gate is NOT)
        {
            if (gate.PreviousGates != null && gate.PreviousGates.Length > 0 && gate.PreviousGates[0] != null)
            {
                string innerExpr = GetGateExpression(gate.PreviousGates[0], circuit, memo, inputNameMap);
                expression = $"¬({innerExpr})"; // NOT 연산자
            }
            else expression = $"ERROR_NO_INPUT({gateId})";
        }
        else if (gate is WIRE)
        {
             if (gate.PreviousGates != null && gate.PreviousGates.Length > 0 && gate.PreviousGates[0] != null)
             {
                 expression = GetGateExpression(gate.PreviousGates[0], circuit, memo, inputNameMap); // 입력 그대로 전달
             }
             else expression = $"ERROR_NO_INPUT({gateId})";
        }
        else // AND, OR, XOR (2-input gates)
        {
            if (gate.PreviousGates != null && gate.PreviousGates.Length >= 2 && gate.PreviousGates[0] != null && gate.PreviousGates[1] != null)
            {
                string leftExpr = GetGateExpression(gate.PreviousGates[0], circuit, memo, inputNameMap);
                string rightExpr = GetGateExpression(gate.PreviousGates[1], circuit, memo, inputNameMap);
                string op = "";

                if (gate is AND) op = " ∧ ";
                else if (gate is OR) op = " ∨ ";
                else if (gate is XOR) op = " ⊕ ";
                else op = " ? "; // 알 수 없는 이진 게이트

                expression = $"({leftExpr}{op}{rightExpr})"; // 괄호로 묶음
            }
             else expression = $"ERROR_MISSING_INPUTS({gateId})";
        }

        memo[gate] = expression; // 결과 메모
        return expression;
    }
}
