using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

// 로직 회로 생성을 담당하는 클래스
public class LogicCircuitFactory
{
    private System.Random random = new System.Random();

    // 로직 회로를 생성합니다.
    public LogicCircuit GenerateLogic(int inputCount, int outputCount, int layerCount, int layerSize)
    {
        LogicCircuit circuit = new LogicCircuit();
        // 입력/출력 타입을 제외한 게이트 타입 목록 가져오기
        var enumValues = System.Enum.GetValues(typeof(LogicGateType)).Cast<LogicGateType>()
                             .Where(t => t != LogicGateType.INPUT && t != LogicGateType.OUTPUT)
                             .ToArray();

        // 1. Input Gate 생성
        List<LogicGate> currentLayerOutputs = new List<LogicGate>();
        for (int i = 0; i < inputCount; i++)
        {
            InputGate inputGate = new InputGate();
            circuit.InputGates.Add(inputGate);
            currentLayerOutputs.Add(inputGate);
        }

        // 2. Hidden Layer 생성
        for (int layerIndex = 0; layerIndex < layerCount; layerIndex++)
        {
            List<LogicGate> nextLayerGates = new List<LogicGate>();
            for (int gateIndex = 0; gateIndex < layerSize; gateIndex++)
            {
                LogicGateType randomType = enumValues[random.Next(0, enumValues.Length)];
                LogicGate gate = CreateGateByType(randomType);
                nextLayerGates.Add(gate);
            }
            ConnectGateInputs(nextLayerGates, currentLayerOutputs, random);
            circuit.HiddenLayers.Add(nextLayerGates);
            currentLayerOutputs = nextLayerGates;
        }

        // 3. Intermediate Layer 생성 (출력 게이트 수에 맞춰 줄여나감)
        while (currentLayerOutputs.Count > outputCount)
        {
            int nextLayerSize = (int)Math.Ceiling(currentLayerOutputs.Count / 2.0);
            nextLayerSize = Math.Max(outputCount, nextLayerSize); // 최소 출력 수 보장

            List<LogicGate> nextLayerGates = new List<LogicGate>();
            for (int gateIndex = 0; gateIndex < nextLayerSize; gateIndex++)
            {
                 LogicGateType randomType = enumValues[random.Next(0, enumValues.Length)];
                 // 마지막 레이어 그룹으로 가기 전에는 WIRE/NOT 피하기 (선택적 로직)
                 if (nextLayerSize > outputCount) // 아직 최종 출력으로 가는 단계가 아니면
                 {
                     while(randomType == LogicGateType.WIRE || randomType == LogicGateType.NOT)
                     {
                         randomType = enumValues[random.Next(0, enumValues.Length)];
                     }
                 }
                 LogicGate gate = CreateGateByType(randomType);
                 nextLayerGates.Add(gate);
            }
            ConnectGateInputs(nextLayerGates, currentLayerOutputs, random);
            circuit.HiddenLayers.Add(nextLayerGates);
            currentLayerOutputs = nextLayerGates;
        }

        // 4. Output Gate 생성 및 최종 연결 (WIRE 사용)
        List<LogicGate> outputGates = new List<LogicGate>();
        for (int i = 0; i < outputCount; i++)
        {
            // 출력 게이트는 WIRE 타입 사용 (별도 OutputGate 클래스 대신)
            LogicGate outputGate = CreateGateByType(LogicGateType.WIRE);
            outputGates.Add(outputGate);
            circuit.OutputGates.Add(outputGate);
        }
        ConnectGateInputs(outputGates, currentLayerOutputs, random);

        // 모든 게이트 리스트 채우기
        circuit.PopulateAllGates(); // LogicCircuit 클래스에 추가된 메서드 호출

        return circuit;
    }

    // 타입에 따라 게이트 인스턴스 생성
    private LogicGate CreateGateByType(LogicGateType type)
    {
        switch (type)
        {
            case LogicGateType.AND: return new AND();
            case LogicGateType.OR: return new OR();
            case LogicGateType.NOT: return new NOT();
            case LogicGateType.XOR: return new XOR();
            case LogicGateType.WIRE: return new WIRE();
            // INPUT, OUTPUT 타입은 여기서 직접 생성하지 않음
            default:
                throw new System.ArgumentException("Invalid gate type specified for creation: " + type);
        }
    }

    // 게이트 입력 연결 (이전 레이어 출력과)
    private void ConnectGateInputs(List<LogicGate> currentLayerGates, List<LogicGate> previousLayerOutputs, System.Random random)
    {
        if (previousLayerOutputs == null || previousLayerOutputs.Count == 0)
        {
            Debug.LogError("ConnectGateInputs: 이전 레이어 출력이 비어 있습니다.");
            return;
        }

        int previousCount = previousLayerOutputs.Count;

        foreach (var currentGate in currentLayerGates)
        {
            if (currentGate is InputGate) continue; // InputGate는 연결 안 함

            int inputNeeded = currentGate.InputCount;
            if (inputNeeded == 0) continue;

            // 이전 레이어 출력을 섞어서 사용 (중복 최소화 시도)
            List<LogicGate> availableSources = previousLayerOutputs.OrderBy(x => random.Next()).ToList();
            List<LogicGate> sourcesToConnect = new List<LogicGate>();

            if (inputNeeded <= previousCount)
            {
                // 필요한 입력 수가 충분하면 중복 없이 선택
                sourcesToConnect.AddRange(availableSources.Take(inputNeeded));
            }
            else
            {
                // 부족하면 중복 허용하여 선택
                sourcesToConnect.AddRange(availableSources);
                int remainingNeeded = inputNeeded - previousCount;
                for (int i = 0; i < remainingNeeded; i++)
                {
                    sourcesToConnect.Add(availableSources[i % previousCount]);
                }
                 // ID 결정 로직이 여기 없으므로 간단한 경고만 표시
                 Debug.LogWarning($"Gate ({currentGate.GetType().Name}) needs {inputNeeded} inputs but previous layer only has {previousCount}. Inputs will be duplicated.");
            }

            // 선택된 소스를 게이트 입력에 연결
            for (int i = 0; i < inputNeeded; i++)
            {
                if (i < sourcesToConnect.Count)
                {
                    LogicGate sourceGate = sourcesToConnect[i];
                    if (!currentGate.ConnectInput(i, sourceGate))
                    {
                         // ID 결정 로직이 여기 없으므로 간단한 경고만 표시
                         Debug.LogWarning($"Failed to connect input {i} of gate ({currentGate.GetType().Name}) from source ({sourceGate.GetType().Name}).");
                    }
                }
                else
                {
                     Debug.LogError($"Logic error: Not enough source gates selected for gate ({currentGate.GetType().Name}). Needed {inputNeeded}, selected {sourcesToConnect.Count}");
                }
            }
        }
    }
}
