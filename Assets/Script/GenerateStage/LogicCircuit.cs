using System;
using System.Collections.Generic;

// 전체 로직 회로를 나타내는 클래스입니다.
// 입력 게이트, 출력 게이트 및 중간 레이어의 게이트들을 포함합니다.
public class LogicCircuit
{
    // 회로의 입력 게이트 목록입니다.
    // 이 게이트들은 외부에서 값을 입력받습니다.
    public List<LogicGate> InputGates { get; private set; }

    // 회로의 출력 게이트 목록입니다.
    // 이 게이트들은 회로의 최종 계산 결과를 나타냅니다.
    public List<LogicGate> OutputGates { get; private set; }

    // 회로의 중간 레이어 게이트 목록입니다.
    // 각 레이어는 게이트의 목록으로 표현됩니다.
    public List<List<LogicGate>> HiddenLayers { get; private set; }

    // 회로의 모든 게이트를 포함하는 단일 목록입니다.
    public List<LogicGate> AllGates { get; private set; } // Flat list of all gates

    // LogicCircuit의 생성자입니다.
    // 필요한 게이트 목록을 초기화합니다.
    public LogicCircuit()
    {
        InputGates = new List<LogicGate>();
        OutputGates = new List<LogicGate>();
        HiddenLayers = new List<List<LogicGate>>();
        AllGates = new List<LogicGate>();
    }

    internal void PopulateAllGates()
    {
        AllGates.Clear(); // 기존 목록을 비웁니다.

        // 입력 게이트 추가
        AllGates.AddRange(InputGates);

        // 숨겨진 레이어의 게이트 추가
        foreach (var layer in HiddenLayers)
        {
            AllGates.AddRange(layer);
        }

        // 출력 게이트 추가
        AllGates.AddRange(OutputGates);
    }
}