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
    Progress,
    Quality,
    Buff
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class SynthActionAttribute : Attribute
  {
    private ActionType type = ActionType.Progress;
    private int cp = 0;
    private string name = String.Empty;
    private int efficiency = 0;
    private int durability = 10;
    private int successRate = 100;
    private int buffDuration = 0;
    private bool disabled = false;

    public SynthActionAttribute(ActionType type)
    {
      this.type = type;
      if (type == ActionType.Buff)
      {
        this.durability = 0;
        this.successRate = 100;
        this.efficiency = 0;
      }
      else
      {
        this.durability = 10;
        this.successRate = 0;
        this.efficiency = 100;
      }
    }

    public ActionType Type
    {
      get { return type; }
    }

    public int CP
    {
      get { return cp; }
      set { cp = value; }
    }

    public string Name
    {
      get { return name; }
      set { name = value; }
    }

    public int Efficiency
    {
      get { return efficiency; }
      set { efficiency = value; }
    }

    public int BuffDuration
    {
      get { return buffDuration; }
      set { buffDuration = value; }
    }

    public int Durability
    {
      get { return durability; }
      set { durability = value; }
    }

    public int SuccessRate
    {
      get { return successRate; }
      set { successRate = value; }
    }

    public bool Disabled
    {
      get { return disabled; }
      set { disabled = value; }
    }
  }

  public class SynthAction<T>
  {
    public static SynthActionAttribute Attributes
    {
      get
      {
        Type info = typeof(T);
        object[] attributes = info.GetCustomAttributes(typeof(SynthActionAttribute), false);
        Debug.Assert(attributes.Length == 1);
        return (SynthActionAttribute)attributes[0];
      }
    }
  }
}
