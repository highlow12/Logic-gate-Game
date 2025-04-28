using UnityEngine;

/// <summary>
/// 로직 회로의 입력 게이트를 나타내는 클래스입니다.
/// 이 게이트는 외부에서 값을 설정할 수 있으며, 다른 게이트의 입력으로 사용됩니다.
/// </summary>
public class InputGate : LogicGate
{
    /// <summary>
    /// InputGate의 생성자입니다.
    /// </summary>
    public InputGate() : base() // Pass true to skip base InitPreviousGates
    {
        // State can be set directly using SetValue
    }

    /// <summary>
    /// 게이트 타입을 설정합니다.
    /// </summary>
    protected override void SetGateType()
    {
        // Consider adding an INPUT type to LogicGateType enum later
        type = LogicGenerator.LogicGateType.WIRE; // Treat as WIRE conceptually
    }

    /// <summary>
    /// 입력 및 출력 개수를 설정합니다.
    /// 입력 게이트는 입력이 없고 출력이 1개입니다.
    /// </summary>
    protected override void SetInOutCount()
    {
        InputCount = 0; // No inputs from other gates
        OutputCount = 1; // Can output to other gates
    }

    /// <summary>
    /// 논리 연산을 계산합니다.
    /// 입력 게이트의 경우 외부에서 설정된 값을 반환합니다.
    /// </summary>
    /// <returns>계산된 논리값</returns>
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

    /// <summary>
    /// 입력 게이트의 값을 설정하고 연결된 회로의 재계산을 트리거합니다.
    /// </summary>
    /// <param name="value">설정할 논리값</param>
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

    /// <summary>
    /// 이전 게이트 배열을 초기화합니다.
    /// 입력 게이트는 이전 게이트가 없습니다.
    /// </summary>
    protected override void InitPreviousGates()
    {
        // Input gates don't have previous gates connected via logic
        PreviousGates = new LogicGate[0];
    }

    /// <summary>
    /// 이전 게이트의 출력값을 가져옵니다.
    /// 입력 게이트의 경우 이전 게이트가 없으므로 빈 배열을 반환합니다.
    /// </summary>
    /// <returns>이전 게이트의 출력값 배열</returns>
    public override bool?[] GetPreviousOutput()
    {
        return new bool?[0]; // No previous outputs
    }

    /// <summary>
    /// 입력 게이트에 다른 게이트를 연결하려는 시도를 차단합니다.
    /// 입력 게이트는 외부 입력만 받아야 합니다.
    /// </summary>
    /// <param name="index">연결할 입력 인덱스</param>
    /// <param name="gate">연결할 게이트</param>
    /// <returns>항상 false 반환</returns>
    public override bool ConnectInput(int index, LogicGate gate)
    {
        Debug.LogError("Cannot connect inputs to an InputGate.");
        return false;
    }
}

/// <summary>
/// AND 논리 게이트를 나타내는 클래스입니다.
/// 모든 입력이 참일 때만 출력이 참이 됩니다.
/// </summary>
public class AND : LogicGate
{
    /// <summary>
    /// 게이트 타입을 설정합니다.
    /// </summary>
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.AND;
    }

    /// <summary>
    /// 입력 및 출력 개수를 설정합니다.
    /// AND 게이트는 2개의 입력과 1개의 출력을 가집니다.
    /// </summary>
    protected override void SetInOutCount()
    {
        InputCount = 2;
        OutputCount = 1;
    }

    /// <summary>
    /// AND 논리 연산을 계산합니다.
    /// 두 입력이 모두 참일 때만 참을 반환합니다.
    /// </summary>
    /// <returns>계산된 논리값</returns>
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

/// <summary>
/// OR 논리 게이트를 나타내는 클래스입니다.
/// 하나 이상의 입력이 참이면 출력이 참이 됩니다.
/// </summary>
public class OR : LogicGate
{
    /// <summary>
    /// 게이트 타입을 설정합니다.
    /// </summary>
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.OR;
    }

    /// <summary>
    /// 입력 및 출력 개수를 설정합니다.
    /// OR 게이트는 2개의 입력과 1개의 출력을 가집니다.
    /// </summary>
    protected override void SetInOutCount()
    {
        InputCount = 2;
        OutputCount = 1;
    }

    /// <summary>
    /// OR 논리 연산을 계산합니다.
    /// 하나 이상의 입력이 참이면 참을 반환합니다.
    /// </summary>
    /// <returns>계산된 논리값</returns>
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

/// <summary>
/// NOT 논리 게이트를 나타내는 클래스입니다.
/// 입력의 반대 값을 출력합니다.
/// </summary>
public class NOT : LogicGate
{
    /// <summary>
    /// 게이트 타입을 설정합니다.
    /// </summary>
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.NOT;
    }

    /// <summary>
    /// 입력 및 출력 개수를 설정합니다.
    /// NOT 게이트는 1개의 입력과 1개의 출력을 가집니다.
    /// </summary>
    protected override void SetInOutCount()
    {
        InputCount = 1;
        OutputCount = 1;
    }

    /// <summary>
    /// NOT 논리 연산을 계산합니다.
    /// 입력의 반대 값을 반환합니다.
    /// </summary>
    /// <returns>계산된 논리값</returns>
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

/// <summary>
/// XOR 논리 게이트를 나타내는 클래스입니다.
/// 두 입력이 서로 다른 경우에만 참을 출력합니다.
/// </summary>
public class XOR : LogicGate
{
    /// <summary>
    /// 게이트 타입을 설정합니다.
    /// </summary>
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.XOR;
    }

    /// <summary>
    /// 입력 및 출력 개수를 설정합니다.
    /// XOR 게이트는 2개의 입력과 1개의 출력을 가집니다.
    /// </summary>
    protected override void SetInOutCount()
    {
        InputCount = 2;
        OutputCount = 1;
    }

    /// <summary>
    /// XOR 논리 연산을 계산합니다.
    /// 두 입력이 서로 다른 경우에만 참을 반환합니다.
    /// </summary>
    /// <returns>계산된 논리값</returns>
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

/// <summary>
/// WIRE 게이트(버퍼)를 나타내는 클래스입니다.
/// 입력을 그대로 출력하는 단순 통과 게이트입니다.
/// </summary>
public class WIRE : LogicGate
{
    /// <summary>
    /// 게이트 타입을 설정합니다.
    /// </summary>
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.WIRE;
    }

    /// <summary>
    /// 입력 및 출력 개수를 설정합니다.
    /// WIRE 게이트는 1개의 입력과 1개의 출력을 가집니다.
    /// </summary>
    protected override void SetInOutCount()
    {
        InputCount = 1;
        OutputCount = 1;
    }

    /// <summary>
    /// WIRE 논리 연산을 계산합니다.
    /// 입력을 그대로 출력합니다.
    /// </summary>
    /// <returns>계산된 논리값</returns>
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