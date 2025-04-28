using UnityEngine;
using System.Collections.Generic;

public abstract class LogicGate
{
    // 게이트의 입력 개수입니다.
    public int InputCount { get; protected set; }

    // 게이트의 출력 개수입니다.
    public int OutputCount { get; protected set; }

    // 게이트의 내부 상태 값입니다.
    protected bool? _state = null; // Renamed for clarity (backing field)

    // 게이트의 상태(계산 결과)를 가져오거나 설정합니다.
    // 값이 null인 경우 계산을 수행합니다.
    public bool? State
    {
        get
        {
            if (_state == null)
            {
                _state = CalculateLogic(); // Calculate and store if null
            }
            return _state;
        }
        // Make setter protected to control updates internally
        protected set
        {
             _state = value;
        }
    }

    // 이 게이트에 입력을 제공하는 이전 게이트들의 배열입니다.
    public LogicGate[] PreviousGates = null;

    // 현재 게이트의 타입입니다.
    protected LogicGenerator.LogicGateType type;

    // 이 게이트의 출력에 연결된 게이트 목록입니다.
    protected List<LogicGate> OutputConnections = new List<LogicGate>(); // Gates connected TO this gate's output

    // LogicGate의 생성자입니다.
    // 매개변수:
    //   skipInit: 이전 게이트 초기화를 건너뛸지 여부
    protected LogicGate(bool skipInit = false)
    {
        SetInOutCount(); // Must be called first
        SetGateType();
        if (!skipInit)
        {
            InitPreviousGates();
        }
    }

    // 논리 연산을 계산합니다.
    // 각 게이트 타입에 맞는 논리 연산을 구현해야 합니다.
    // 반환값: 계산된 논리값
    public abstract bool CalculateLogic();

    // 게이트 타입을 설정합니다.
    // 각 게이트 클래스에서 구현해야 합니다.
    protected abstract void SetGateType();

    // 게이트의 입력 및 출력 개수를 설정합니다.
    // 각 게이트 클래스에서 구현해야 합니다.
    protected abstract void SetInOutCount();

    // 이전 게이트 배열을 초기화합니다.
    protected virtual void InitPreviousGates()
    {
        PreviousGates = new LogicGate[InputCount];
    }

    // 이전 게이트들의 출력값을 가져옵니다.
    // 반환값: 이전 게이트들의 출력값 배열
    public virtual bool?[] GetPreviousOutput()
    {
        if (PreviousGates == null) // Check if array itself is null
        {
             return new bool?[0];
        }
        if (PreviousGates.Length == 0) // Check if array is empty
        {
            return new bool?[0];
        }

        bool?[] previousOutputs = new bool?[PreviousGates.Length];
        for (int i = 0; i < PreviousGates.Length; i++)
        {
            if (PreviousGates[i] != null)
            {
                previousOutputs[i] = PreviousGates[i].State; // Get state from previous gate
            }
            else
            {
                // If a required input is not connected, treat as null
                previousOutputs[i] = null;
                Debug.LogWarning($"Gate {this.GetLogicGateType()} is missing input connection at index {i}.");
            }
        }
        return previousOutputs;
    }

    // 현재 게이트의 타입을 반환합니다.
    // 반환값: 게이트 타입
    public LogicGenerator.LogicGateType GetLogicGateType()
    {
        return type;
    }

    // 지정된 인덱스의 입력에 소스 게이트를 연결합니다.
    // 매개변수:
    //   index: 연결할 입력 인덱스
    //   sourceGate: 연결할 소스 게이트
    // 반환값: 연결 성공 여부
    public virtual bool ConnectInput(int index, LogicGate sourceGate)
    {
        if (index < 0 || index >= InputCount)
        {
            Debug.LogError($"유효하지 않은 입력 인덱스: {index}, 최대 인덱스: {InputCount - 1} for gate type {type}");
            return false;
        }
        if (sourceGate == null)
        {
             Debug.LogError($"Cannot connect null gate to input index {index} for gate type {type}");
             return false;
        }

        PreviousGates[index] = sourceGate;

        return true;
    }

    // 게이트의 상태를 재설정하고, 연결된 게이트의 상태도 재귀적으로 재설정합니다.
    public virtual void ResetState()
    {
        // Only reset and propagate if state was actually calculated
        // Avoids infinite loops in potential cyclic graphs (though our generator is acyclic)
        if (_state != null)
        {
             _state = null;
             // Recursively reset subsequent gates
             // Create a temporary list to iterate over, as the collection might be modified indirectly
             List<LogicGate> connectionsToReset = new List<LogicGate>(OutputConnections);
             foreach (var connection in connectionsToReset)
             {
                 connection.ResetState();
             }
        }
    }
}

// 로직 회로의 입력 게이트를 나타내는 클래스입니다.
// 이 게이트는 외부에서 값을 설정할 수 있으며, 다른 게이트의 입력으로 사용됩니다.
public class InputGate : LogicGate
{
    // InputGate의 생성자입니다.
    public InputGate() : base() // Pass true to skip base InitPreviousGates
    {
        // State can be set directly using SetValue
    }

    // 게이트 타입을 설정합니다.
    protected override void SetGateType()
    {
        // Consider adding an INPUT type to LogicGateType enum later
        type = LogicGenerator.LogicGateType.WIRE; // Treat as WIRE conceptually
    }

    // 입력 및 출력 개수를 설정합니다.
    // 입력 게이트는 입력이 없고 출력이 1개입니다.
    protected override void SetInOutCount()
    {
        InputCount = 0; // No inputs from other gates
        OutputCount = 1; // Can output to other gates
    }

    // 논리 연산을 계산합니다.
    // 입력 게이트의 경우 외부에서 설정된 값을 반환합니다.
    // 반환값: 계산된 논리값
    public override bool CalculateLogic()
    {
        // Input gate's state is set externally, not calculated from inputs
        if (State == null)
        {
             Debug.LogWarning("InputGate state accessed before being set.");
             return false; // Default value
        }
        return (bool)State;
    }

    // 입력 게이트의 값을 설정하고 연결된 회로의 재계산을 트리거합니다.
    // 매개변수:
    //   value: 설정할 논리값
    public void SetValue(bool value)
    {
        if (_state != value) // Only reset if value actually changes
        {
            _state = value; // Set internal state directly
            // Reset dependents to force recalculation
            foreach (var connection in OutputConnections)
            {
                connection.ResetState();
            }
        }
    }

    // 이전 게이트 배열을 초기화합니다.
    // 입력 게이트는 이전 게이트가 없습니다.
    protected override void InitPreviousGates()
    {
        // Input gates don't have previous gates connected via logic
        PreviousGates = new LogicGate[0];
    }

    // 이전 게이트의 출력값을 가져옵니다.
    // 입력 게이트의 경우 이전 게이트가 없으므로 빈 배열을 반환합니다.
    // 반환값: 이전 게이트의 출력값 배열
    public override bool?[] GetPreviousOutput()
    {
        return new bool?[0]; // No previous outputs
    }

    // 입력 게이트에 다른 게이트를 연결하려는 시도를 차단합니다.
    // 입력 게이트는 외부 입력만 받아야 합니다.
    // 매개변수:
    //   index: 연결할 입력 인덱스
    //   gate: 연결할 게이트
    // 반환값: 항상 false 반환
    public override bool ConnectInput(int index, LogicGate gate)
    {
        Debug.LogError("Cannot connect inputs to an InputGate.");
        return false;
    }
}

// AND 논리 게이트를 나타내는 클래스입니다.
// 모든 입력이 참일 때만 출력이 참이 됩니다.
public class AND : LogicGate
{
    // 게이트 타입을 설정합니다.
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.AND;
    }

    // 입력 및 출력 개수를 설정합니다.
    // AND 게이트는 2개의 입력과 1개의 출력을 가집니다.
    protected override void SetInOutCount()
    {
        InputCount = 2;
        OutputCount = 1;
    }

    // AND 논리 연산을 계산합니다.
    // 두 입력이 모두 참일 때만 참을 반환합니다.
    // 반환값: 계산된 논리값
    public override bool CalculateLogic()
    {
        bool?[] inputs = GetPreviousOutput();
        
        // 입력 검증
        if (inputs == null || inputs.Length < 2)
        {
            Debug.LogWarning($"AND 게이트 입력 오류: 필요한 입력 수는 2개입니다. 현재: {inputs?.Length ?? 0}");
            return false;
        }
        
        if (inputs[0] == null || inputs[1] == null)
        {
            Debug.LogWarning("AND 게이트 입력 오류: null 입력이 존재합니다.");
            return false;
        }

        bool result = (bool)inputs[0] && (bool)inputs[1];
        State = result;
        return result;
    }
}

// OR 논리 게이트를 나타내는 클래스입니다.
// 하나 이상의 입력이 참이면 출력이 참이 됩니다.
public class OR : LogicGate
{
    // 게이트 타입을 설정합니다.
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.OR;
    }

    // 입력 및 출력 개수를 설정합니다.
    // OR 게이트는 2개의 입력과 1개의 출력을 가집니다.
    protected override void SetInOutCount()
    {
        InputCount = 2;
        OutputCount = 1;
    }

    // OR 논리 연산을 계산합니다.
    // 하나 이상의 입력이 참이면 참을 반환합니다.
    // 반환값: 계산된 논리값
    public override bool CalculateLogic()
    {
        bool?[] inputs = GetPreviousOutput();
        
        // 입력 검증
        if (inputs == null || inputs.Length < 2)
        {
            Debug.LogWarning($"OR 게이트 입력 오류: 필요한 입력 수는 2개입니다. 현재: {inputs?.Length ?? 0}");
            return false;
        }
        
        if (inputs[0] == null || inputs[1] == null)
        {
            Debug.LogWarning("OR 게이트 입력 오류: null 입력이 존재합니다.");
            return false;
        }

        bool result = (bool)inputs[0] || (bool)inputs[1];
        State = result;
        return result;
    }
}

// NOT 논리 게이트를 나타내는 클래스입니다.
// 입력의 반대 값을 출력합니다.
public class NOT : LogicGate
{
    // 게이트 타입을 설정합니다.
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.NOT;
    }

    // 입력 및 출력 개수를 설정합니다.
    // NOT 게이트는 1개의 입력과 1개의 출력을 가집니다.
    protected override void SetInOutCount()
    {
        InputCount = 1;
        OutputCount = 1;
    }

    // NOT 논리 연산을 계산합니다.
    // 입력의 반대 값을 반환합니다.
    // 반환값: 계산된 논리값
    public override bool CalculateLogic()
    {
        bool?[] inputs = GetPreviousOutput();
        
        // 입력 검증
        if (inputs == null || inputs.Length < 1)
        {
            Debug.LogWarning($"NOT 게이트 입력 오류: 필요한 입력 수는 1개입니다. 현재: {inputs?.Length ?? 0}");
            return false;
        }
        
        if (inputs[0] == null)
        {
            Debug.LogWarning("NOT 게이트 입력 오류: null 입력이 존재합니다.");
            return false;
        }

        bool result = !(bool)inputs[0];
        State = result;
        return result;
    }
}

// XOR 논리 게이트를 나타내는 클래스입니다.
// 두 입력이 서로 다른 경우에만 참을 출력합니다.
public class XOR : LogicGate
{
    // 게이트 타입을 설정합니다.
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.XOR;
    }

    // 입력 및 출력 개수를 설정합니다.
    // XOR 게이트는 2개의 입력과 1개의 출력을 가집니다.
    protected override void SetInOutCount()
    {
        InputCount = 2;
        OutputCount = 1;
    }

    // XOR 논리 연산을 계산합니다.
    // 두 입력이 서로 다른 경우에만 참을 반환합니다.
    // 반환값: 계산된 논리값
    public override bool CalculateLogic()
    {
        bool?[] inputs = GetPreviousOutput();
        
        // 입력 검증
        if (inputs == null || inputs.Length < 2)
        {
            Debug.LogWarning($"XOR 게이트 입력 오류: 필요한 입력 수는 2개입니다. 현재: {inputs?.Length ?? 0}");
            return false;
        }
        
        if (inputs[0] == null || inputs[1] == null)
        {
            Debug.LogWarning("XOR 게이트 입력 오류: null 입력이 존재합니다.");
            return false;
        }

        bool result = (bool)inputs[0] ^ (bool)inputs[1];
        State = result;
        return result;
    }
}

// WIRE 게이트(버퍼)를 나타내는 클래스입니다.
// 입력을 그대로 출력하는 단순 통과 게이트입니다.
public class WIRE : LogicGate
{
    // 게이트 타입을 설정합니다.
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.WIRE;
    }

    // 입력 및 출력 개수를 설정합니다.
    // WIRE 게이트는 1개의 입력과 1개의 출력을 가집니다.
    protected override void SetInOutCount()
    {
        InputCount = 1;
        OutputCount = 1;
    }

    // WIRE 논리 연산을 계산합니다.
    // 입력을 그대로 출력합니다.
    // 반환값: 계산된 논리값
    public override bool CalculateLogic()
    {
        bool?[] inputs = GetPreviousOutput();
        
        // 입력 검증
        if (inputs == null || inputs.Length < 1)
        {
            Debug.LogWarning($"WIRE 게이트 입력 오류: 필요한 입력 수는 1개입니다. 현재: {inputs?.Length ?? 0}");
            return false;
        }
        
        if (inputs[0] == null)
        {
            Debug.LogWarning("WIRE 게이트 입력 오류: null 입력이 존재합니다.");
            return false;
        }

        bool result = (bool)inputs[0];
        State = result;
        return result;
    }
}