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
    TemporaryEnhancement
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class SynthActionAttribute : Attribute
  {
    private ActionType type = ActionType.Completion;
    private string name = String.Empty;
    private uint cp = 0;
    private AbilityId id;
    private bool disabled = false;

    public SynthActionAttribute(ActionType type, AbilityId id, string name, uint cp)
    {
      this.type = type;
      this.name = name;
      this.cp = cp;
      this.id = id;
    }

    public uint CP { get { return cp; } }
    public string Name { get { return name; } }
    public AbilityId ActionId { get { return id; } }
    public ActionType ActionType { get { return type; } }

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
  public class TemporaryEnhancementAttribute : Attribute
  {
    private uint duration;
    public TemporaryEnhancementAttribute(uint duration)
    {
      this.duration = duration;
    }

    public uint Duration { get { return duration; } }
  }

  public class SynthAction<AttrType>
  {
    public static AttrType Attributes(object obj)
    {
      return Attributes(obj.GetType());
    }

    public static AttrType Attributes(Type objType)
    {
      object[] attributes = objType.GetCustomAttributes(typeof(AttrType), false);
      if (attributes.Length == 0)
        return default(AttrType);
      Debug.Assert(attributes.Length == 1);
      return (AttrType)attributes[0];
    }
  }

  public class SynthAction<AttrType, ObjType>
  {
    public static AttrType Attributes
    {
      get
      {
        return SynthAction<AttrType>.Attributes(typeof(ObjType));
      }
    }
  }
}
