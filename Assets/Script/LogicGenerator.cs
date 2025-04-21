using UnityEngine;


public class LogicGenerator : Singleton<LogicGenerator>
{
    //로직 게이트 타입을 선언하는 enum
    public enum LogicGateType
    {
        AND,
        OR,
        NOT,
        XOR,
        WIRE,
    }
    public object GenerateLogic(int input, int output, int layerCount, int layerSize)
    {
        return null;
    }
    //진리표를 반환하는 함수
    public object GenerateTruthTable(object logic)
    {
        return null;
    }
    public int GetDifficulty(object logic)
    {
        return 0;
    }
    public object GetLogicJSON(object logic, string savePath = null)
    {
        return null;
    }
    //불 대수 식을 반환하는 함수
    public object GetLogicExpression(object logic)
    {
        return null;
    }
}

abstract class LogicGate
{
    public int InputCount { get; protected set; }
    public int OutputCount { get; protected set; }
    private bool? _state = null; // 게이트의 출력 상태 (true, false, null: 초기화되지 않음)
    public bool? State 
    { 
        get
        {
            if (_state == null)
            {
                return CalculateLogic(); // 상태가 null이면 계산 수행
            }
            return _state; // 값 반환
        } 
        protected set
        {
            _state = value;
        }
    }
    protected LogicGate[] PreviousGates = null;
    protected LogicGenerator.LogicGateType type;
    public LogicGate()
    {
        SetInOutCount();
        SetGateType();
        InitPreviousGates();
    }
    public abstract bool CalculateLogic();
    protected abstract void SetGateType();
    protected abstract void SetInOutCount();
    protected void InitPreviousGates()
    {
        PreviousGates = new LogicGate[InputCount];
    }
    // 이전 단계의 출력을 가져오는 함수
    public bool?[] GetPreviousOutput()
    {
        if (PreviousGates == null || PreviousGates.Length == 0)
        {
            return null; // 이전 게이트가 없으면 null 반환
        }

        bool?[] previousOutputs = new bool?[PreviousGates.Length];
        for (int i = 0; i < PreviousGates.Length; i++)
        {
            if (PreviousGates[i] != null)
            {
                previousOutputs[i] = PreviousGates[i].State; // 이전 게이트의 상태를 가져옴
            }
            else
            {
                previousOutputs[i] = null; // 이전 게이트가 연결되지 않았으면 null 반환
            }
        }
        return previousOutputs;
    }

    // 게이트 타입 반환
    public LogicGenerator.LogicGateType GetLogicGateType()
    {
        return type;
    }

    // 이전 게이트 연결
    public bool ConnectInput(int index, LogicGate gate)
    {
        if (index < 0 || index >= InputCount)
        {
            Debug.LogError($"유효하지 않은 입력 인덱스: {index}, 최대 인덱스: {InputCount - 1}");
            return false;
        }
        
        PreviousGates[index] = gate;
        return true;
    }

    // 상태 재설정 - 재계산 강제
    public void ResetState()
    {
        State = null;
    }
}