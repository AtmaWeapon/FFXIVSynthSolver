using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public class FixedDurationEnhancementAction : Action
  {
    private delegate void FieldSetter(uint offset, uint numBits, uint value);
    private delegate uint FieldGetter(uint offset, uint numBits);
    private uint numBitsRequired;
    private uint offset;

    private FieldSetter setter;
    private FieldGetter getter;

    private uint duration;

    public FixedDurationEnhancementAction()
    {
      FixedDurationEnhancementAttribute attributes = SynthAction<FixedDurationEnhancementAttribute>.Attributes(this);

      duration = attributes.Duration;
      numBitsRequired = Compute.NumBitsRequired(duration);
    }

    public uint GetTurnsRemaining(State state)
    {
      return 0;
    }

    public uint SetTurnsRemaining(State state)
    {

    }

    protected override void ApplyAction(State oldState, State newState, bool success)
    {
      base.ApplyAction(oldState, newState, success);
    }
  }

  [SynthAction(ActionType.FixedDurationEnhancement, "Steady Hand", 22)]
  [FixedDurationEnhancement(5)]
  public class SteadyHand : FixedDurationEnhancementAction
  {
  }

  // TODO
  [SynthAction(ActionType.FixedDurationEnhancement, "Manipulation", 88)]
  [FixedDurationEnhancement(3)]
  public class Manipulation : FixedDurationEnhancementAction
  {
  }

  [SynthAction(ActionType.FixedDurationEnhancement, "Ingenuity", 24)]
  [FixedDurationEnhancement(3)]
  public class Ingenuity : FixedDurationEnhancementAction
  {
  }

  [SynthAction(ActionType.FixedDurationEnhancement, "Great Strides", 32)]
  [FixedDurationEnhancement(3)]
  public class GreatStrides : FixedDurationEnhancementAction
  {
  }
}
