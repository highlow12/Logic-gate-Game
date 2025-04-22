using UnityEngine;
using System.Collections.Generic; // Required for Lists
using System; // Required for System.Random
using System.Linq; // Required for LINQ methods like OrderBy

// Represents a primary input to the circuit

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

    // --- GenerateLogic Implementation ---
    // layerCount: 레이어의 개수
    // layerSize: 레이어에 있는 게이트의 개수
    public LogicCircuit GenerateLogic(int inputCount, int outputCount, int layerCount, int layerSize)
    {
        // upper layer is close to the output layer
        // lower layer is close to the input layer
        LogicCircuit circuit = new LogicCircuit();
        System.Random random = new System.Random();
        List<LogicGate> upperLayerOutputs = new List<LogicGate>();
        var enumValues = System.Enum.GetValues(enumType:typeof(LogicGateType));
        // outputGate 생성
        for (int i = 0; i < outputCount; i++)
        {
            LogicGate outputGate = CreateGateByType(LogicGateType.WIRE);
            circuit.OutputGates.Add(outputGate);
            upperLayerOutputs.Add(outputGate); // Add to previous layer outputs
        }
        // 레이어 생성
        for (int layerIndex = 0; layerIndex < layerCount; layerIndex++)
        {
            List<LogicGate> lowerLayerOutputs = new List<LogicGate>();
            for (int gateIndex = 0; gateIndex < layerSize; gateIndex++)
            {
                LogicGate gate = CreateGateByType((LogicGateType)enumValues.GetValue(random.Next(0, enumValues.Length))); // Randomly select gate type
                lowerLayerOutputs.Add(gate);
            }
            ConnectGateInputs(lowerLayerOutputs,upperLayerOutputs, random); // Connect gates in the current layer to the previous layer
            
            circuit.HiddenLayers.Add(lowerLayerOutputs); // Add the current layer to the circuit
            
            upperLayerOutputs = lowerLayerOutputs; // Update the previous layer outputs for the next iteration
            // Removed Clear() call to avoid clearing the hidden layer already added to the circuit
        }
        // 인풋 게이트 생성
        for (int i = 0; i < inputCount; i++)
        {
            InputGate inputGate = new InputGate(); // Pass true to skip InitPreviousGates
            circuit.InputGates.Add(inputGate);
        }
        ConnectGateInputs(circuit.InputGates, upperLayerOutputs, random); // Connect input gates to the last layer
        return circuit;
    }

    // Helper to create gate instance by type
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

    // Helper to connect gate inputs randomly to the previous layer
    private void ConnectGateInputs(List<LogicGate> gate, List<LogicGate> previousLayer, System.Random random)
    {
        int inputCount = 0;
        List<LogicGate> gates = new();

        foreach (var pl in previousLayer)
        {
            inputCount += pl.InputCount; // Count total inputs in the previous layer
        }

        for (int i = 0; i < MathF.Ceiling((float)inputCount / gate.Count); i++)
        {
            gates.AddRange(gate); // Add gates to the list for random selection
        }
        gates = gates.OrderBy(x => random.Next()).ToList(); // Shuffle the gates

        foreach(var previousGate in previousLayer)
        {
            for (int i = 0; i < previousGate.InputCount; i++)
            {
                previousGate.ConnectInput(i, gates[0]); 
                gates.RemoveAt(0); // Remove the gate from the list after connecting
            }
        }
        
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

public abstract class LogicGate
{
    public int InputCount { get; protected set; }
    public int OutputCount { get; protected set; }
    protected bool? _state = null; // Renamed for clarity (backing field)
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
    protected LogicGate[] PreviousGates = null;
    protected LogicGenerator.LogicGateType type;
    protected List<LogicGate> OutputConnections = new List<LogicGate>(); // Gates connected TO this gate's output

    // Modified constructor to allow skipping InitPreviousGates
    protected LogicGate(bool skipInit = false)
    {
        SetInOutCount(); // Must be called first
        SetGateType();
        if (!skipInit)
        {
            InitPreviousGates();
        }
    }
    public abstract bool CalculateLogic();
    protected abstract void SetGateType();
    protected abstract void SetInOutCount();

    // Make virtual to allow override (e.g., InputGate)
    protected virtual void InitPreviousGates()
    {
        PreviousGates = new LogicGate[InputCount];
    }

    // Make virtual to allow override
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

    // 게이트 타입 반환
    public LogicGenerator.LogicGateType GetLogicGateType()
    {
        return type;
    }

    // 이전 게이트 연결 - Make virtual and update output connections
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

    // 상태 재설정 - Make virtual for recursion
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