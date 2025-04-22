using UnityEngine;
public class InputGate : LogicGate
{
    // Input gates have no previous gates, their state is set externally
    public InputGate() : base() // Pass true to skip base InitPreviousGates
    {
        // State can be set directly using SetValue
    }

    protected override void SetGateType()
    {
        // Consider adding an INPUT type to LogicGateType enum later
        type = LogicGenerator.LogicGateType.WIRE; // Treat as WIRE conceptually
    }

    protected override void SetInOutCount()
    {
        InputCount = 0; // No inputs from other gates
        OutputCount = 1; // Can output to other gates
    }

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

    // Method to set the input value and trigger recalculation in the circuit
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

    // Override InitPreviousGates for InputGate
    protected override void InitPreviousGates()
    {
        // Input gates don't have previous gates connected via logic
        PreviousGates = new LogicGate[0];
    }

    // Override GetPreviousOutput for InputGate
    public override bool?[] GetPreviousOutput()
    {
        return new bool?[0]; // No previous outputs
    }

    // Override ConnectInput to prevent connections to InputGate
    public override bool ConnectInput(int index, LogicGate gate)
    {
        Debug.LogError("Cannot connect inputs to an InputGate.");
        return false;
    }
}

// AND 게이트
public class AND : LogicGate
{
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.AND;
    }

    protected override void SetInOutCount()
    {
        InputCount = 2;
        OutputCount = 1;
    }

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

// OR 게이트
public class OR : LogicGate
{
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.OR;
    }

    protected override void SetInOutCount()
    {
        InputCount = 2;
        OutputCount = 1;
    }

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

// NOT 게이트
public class NOT : LogicGate
{
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.NOT;
    }

    protected override void SetInOutCount()
    {
        InputCount = 1;
        OutputCount = 1;
    }

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

// XOR 게이트
public class XOR : LogicGate
{
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.XOR;
    }

    protected override void SetInOutCount()
    {
        InputCount = 2;
        OutputCount = 1;
    }

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

// WIRE 게이트 (버퍼)
public class WIRE : LogicGate
{
    protected override void SetGateType()
    {
        type = LogicGenerator.LogicGateType.WIRE;
    }

    protected override void SetInOutCount()
    {
        InputCount = 1;
        OutputCount = 1;
    }

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