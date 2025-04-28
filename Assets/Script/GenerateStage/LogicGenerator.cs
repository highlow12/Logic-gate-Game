using UnityEngine;
using System.Collections.Generic; // Required for Lists
using System; // Required for System.Random
using System.Linq; // Required for LINQ methods like OrderBy

/// <summary>
/// 로직 게이트 회로를 생성하고 관리하는 싱글톤 클래스입니다.
/// 다양한 로직 게이트를 사용하여 회로를 생성하고 계산합니다.
/// </summary>
public class LogicGenerator : Singleton<LogicGenerator>
{
    /// <summary>
    /// 로직 게이트 타입을 정의하는 열거형입니다.
    /// </summary>
    public enum LogicGateType
    {
        /// <summary>AND 논리 게이트</summary>
        AND,
        /// <summary>OR 논리 게이트</summary>
        OR,
        /// <summary>NOT 논리 게이트</summary>
        NOT,
        /// <summary>XOR 논리 게이트</summary>
        XOR,
        /// <summary>WIRE 게이트(버퍼)</summary>
        WIRE,
    }

    /// <summary>
    /// 로직 회로를 생성합니다.
    /// </summary>
    /// <param name="inputCount">입력 게이트의 개수</param>
    /// <param name="outputCount">출력 게이트의 개수</param>
    /// <param name="layerCount">회로의 층 개수</param>
    /// <param name="layerSize">각 층의 게이트 개수</param>
    /// <returns>생성된 로직 회로</returns>
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

    /// <summary>
    /// 타입에 따른 게이트 인스턴스를 생성합니다.
    /// </summary>
    /// <param name="type">생성할 게이트의 타입</param>
    /// <returns>생성된 로직 게이트</returns>
    /// <exception cref="System.ArgumentException">유효하지 않은 게이트 타입이 지정된 경우 발생</exception>
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

    /// <summary>
    /// 게이트 입력을 이전 레이어의 출력에 무작위로 연결합니다.
    /// </summary>
    /// <param name="gate">연결할 게이트 목록</param>
    /// <param name="previousLayer">이전 레이어의 게이트 목록</param>
    /// <param name="random">무작위 수 생성기</param>
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
            gates.AddRange(gate.OrderBy(x => random.Next()).ToList()); // Add gates to the list for random selection
        }

        foreach(var previousGate in previousLayer)
        {
            for (int i = 0; i < previousGate.InputCount; i++)
            {
                previousGate.ConnectInput(i, gates[0]); 
                gates.RemoveAt(0); // Remove the gate from the list after connecting
            }
        }
        
    }

    /// <summary>
    /// 로직 회로의 진리표를 생성합니다.
    /// </summary>
    /// <param name="logic">진리표를 생성할 로직 회로</param>
    /// <returns>생성된 진리표</returns>
    public object GenerateTruthTable(object logic)
    {
        return null;
    }

    /// <summary>
    /// 로직 회로의 난이도를 계산합니다.
    /// </summary>
    /// <param name="logic">난이도를 계산할 로직 회로</param>
    /// <returns>계산된 난이도</returns>
    public int GetDifficulty(object logic)
    {
        return 0;
    }

    /// <summary>
    /// 로직 회로를 JSON 형식으로 변환합니다.
    /// </summary>
    /// <param name="logic">변환할 로직 회로</param>
    /// <param name="savePath">저장할 경로 (선택적)</param>
    /// <returns>JSON 형식의 로직 회로</returns>
    public object GetLogicJSON(object logic, string savePath = null)
    {
        return null;
    }

    /// <summary>
    /// 로직 회로의 불 대수 표현식을 생성합니다.
    /// </summary>
    /// <param name="logic">표현식을 생성할 로직 회로</param>
    /// <returns>생성된 불 대수 표현식</returns>
    public object GetLogicExpression(object logic)
    {
        return null;
    }
}

/// <summary>
/// 모든 로직 게이트의 기본 클래스입니다.
/// 논리 연산을 수행하고 게이트 간 연결을 관리합니다.
/// </summary>
public abstract class LogicGate
{
    /// <summary>
    /// 게이트의 입력 개수입니다.
    /// </summary>
    public int InputCount { get; protected set; }

    /// <summary>
    /// 게이트의 출력 개수입니다.
    /// </summary>
    public int OutputCount { get; protected set; }

    /// <summary>
    /// 게이트의 내부 상태 값입니다.
    /// </summary>
    protected bool? _state = null; // Renamed for clarity (backing field)

    /// <summary>
    /// 게이트의 상태(계산 결과)를 가져오거나 설정합니다.
    /// 값이 null인 경우 계산을 수행합니다.
    /// </summary>
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

    /// <summary>
    /// 이 게이트에 입력을 제공하는 이전 게이트들의 배열입니다.
    /// </summary>
    protected LogicGate[] PreviousGates = null;

    /// <summary>
    /// 현재 게이트의 타입입니다.
    /// </summary>
    protected LogicGenerator.LogicGateType type;

    /// <summary>
    /// 이 게이트의 출력에 연결된 게이트 목록입니다.
    /// </summary>
    protected List<LogicGate> OutputConnections = new List<LogicGate>(); // Gates connected TO this gate's output

    /// <summary>
    /// LogicGate의 생성자입니다.
    /// </summary>
    /// <param name="skipInit">이전 게이트 초기화를 건너뛸지 여부</param>
    protected LogicGate(bool skipInit = false)
    {
        SetInOutCount(); // Must be called first
        SetGateType();
        if (!skipInit)
        {
            InitPreviousGates();
        }
    }

    /// <summary>
    /// 논리 연산을 계산합니다.
    /// 각 게이트 타입에 맞는 논리 연산을 구현해야 합니다.
    /// </summary>
    /// <returns>계산된 논리값</returns>
    public abstract bool CalculateLogic();

    /// <summary>
    /// 게이트 타입을 설정합니다.
    /// 각 게이트 클래스에서 구현해야 합니다.
    /// </summary>
    protected abstract void SetGateType();

    /// <summary>
    /// 게이트의 입력 및 출력 개수를 설정합니다.
    /// 각 게이트 클래스에서 구현해야 합니다.
    /// </summary>
    protected abstract void SetInOutCount();

    /// <summary>
    /// 이전 게이트 배열을 초기화합니다.
    /// </summary>
    protected virtual void InitPreviousGates()
    {
        PreviousGates = new LogicGate[InputCount];
    }

    /// <summary>
    /// 이전 게이트들의 출력값을 가져옵니다.
    /// </summary>
    /// <returns>이전 게이트들의 출력값 배열</returns>
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

    /// <summary>
    /// 현재 게이트의 타입을 반환합니다.
    /// </summary>
    /// <returns>게이트 타입</returns>
    public LogicGenerator.LogicGateType GetLogicGateType()
    {
        return type;
    }

    /// <summary>
    /// 지정된 인덱스의 입력에 소스 게이트를 연결합니다.
    /// </summary>
    /// <param name="index">연결할 입력 인덱스</param>
    /// <param name="sourceGate">연결할 소스 게이트</param>
    /// <returns>연결 성공 여부</returns>
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

    /// <summary>
    /// 게이트의 상태를 재설정하고, 연결된 게이트의 상태도 재귀적으로 재설정합니다.
    /// </summary>
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