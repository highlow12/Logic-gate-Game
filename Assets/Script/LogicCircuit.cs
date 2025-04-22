using System.Collections.Generic;

// Represents the entire generated circuit
public class LogicCircuit
{
    public List<LogicGate> InputGates { get; private set; }
    public List<LogicGate> OutputGates { get; private set; }
    public List<List<LogicGate>> HiddenLayers { get; private set; }
    public List<LogicGate> AllGates { get; private set; } // Flat list of all gates

    public LogicCircuit()
    {
        InputGates = new List<LogicGate>();
        OutputGates = new List<LogicGate>();
        HiddenLayers = new List<List<LogicGate>>();
        AllGates = new List<LogicGate>();
    }
}