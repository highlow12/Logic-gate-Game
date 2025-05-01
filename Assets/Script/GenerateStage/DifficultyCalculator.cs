using UnityEngine;

// 로직 회로의 난이도 계산을 담당하는 클래스
public class DifficultyCalculator
{
    // 로직 회로의 난이도를 계산합니다.
    public int GetDifficulty(LogicCircuit circuit)
    {
        if (circuit == null || circuit.AllGates == null)
        {
            Debug.LogError("GetDifficulty: Invalid LogicCircuit provided.");
            return -1; // 오류 값
        }

        int totalDifficulty = 0;

        // 모든 게이트를 순회하며 난이도 합산
        foreach (var gate in circuit.AllGates)
        {
            // InputGate는 난이도 계산에서 제외
            if (gate is InputGate) continue;

            // 게이트 타입별 난이도 점수 부여
            if (gate is WIRE) totalDifficulty += 0;
            else if (gate is NOT || gate is AND || gate is OR) totalDifficulty += 1;
            else if (gate is XOR) totalDifficulty += 2;
            // 다른 타입 게이트는 무시 (또는 경고)
        }

        return totalDifficulty;
    }
}
