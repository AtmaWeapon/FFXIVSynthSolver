using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public enum ActionType
  {
    Completion,
    OneTimeEnhancement,
    FixedDurationEnhancement,
    PermanentEnhancement
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class SynthActionAttribute : Attribute
  {
    private ActionType type = ActionType.Completion;
    private string name = String.Empty;
    private uint cp = 0;
    private bool disabled = false;

    public SynthActionAttribute(ActionType type, string name, uint cp)
    {
      this.type = type;
      this.name = name;
      this.cp = cp;
    }

    public uint CP { get { return cp; } }
    public string Name { get { return name; } }

    public bool Disabled
    {
      get { return disabled; }
      set { disabled = value; }
    }
  }

  [Flags]
  public enum CompletionFlags
  {
    Progress = 0x1,
    Quality = 0x2,
    TouchAction = 0x4
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class CompletionActionAttribute : Attribute
  {
    private CompletionFlags completionFlags;
    private uint efficiency;
    private uint successRate;

    public CompletionActionAttribute(CompletionFlags completionFlags, uint efficiency, uint successRate)
    {
      this.completionFlags = completionFlags;
      this.efficiency = efficiency;
      this.successRate = successRate;
    }

    public CompletionFlags CompletionFlags { get { return completionFlags; } }
    public uint Efficiency { get { return efficiency; } }
    public uint SuccessRate { get { return successRate; } }
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class OneTimeEnhancementAttribute : Attribute
  {
    
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class FixedDurationEnhancementAttribute : Attribute
  {
    private uint duration;
    public FixedDurationEnhancementAttribute(uint duration)
    {
      this.duration = duration;
    }

    public uint Duration { get { return duration; } }
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class PermanentEnhancementAttribute : Attribute
  {
  }

  public class SynthAction<AttrType>
  {
    public static AttrType Attributes(object obj)
    {
      Type info = obj.GetType();
      object[] attributes = info.GetCustomAttributes(typeof(AttrType), false);
      Debug.Assert(attributes.Length == 1);
      return (AttrType)attributes[0];
    }
  }
}
