using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public class ActionDatabase : IEnumerable<Action>
  {
    private Dictionary<Type, Action> actions;

    public ActionDatabase()
    {
      actions = new Dictionary<Type, Action>();
    }

    public void AddAllActions()
    {
      Assembly thisAssembly = Assembly.GetExecutingAssembly();
      foreach (Type info in thisAssembly.GetTypes())
      {
        object[] attributes = info.GetCustomAttributes(typeof(SynthActionAttribute), false);
        if (attributes.Length == 1)
        {
          SynthActionAttribute attribute = (SynthActionAttribute)attributes[0];
          if (!attribute.Disabled)
          {
            object action = info.GetConstructor(Type.EmptyTypes).Invoke(null);
            AddAction((Action)action);
          }
        }
      }
    }

    public void AddAction(Action action)
    {
      Debug.Assert(!action.Attributes.Disabled);
      actions.Add(action.GetType(), action);
    }

    public void RemoveAction(Type t)
    {
      actions.Remove(t);
    }

    public IEnumerator<Action> GetEnumerator()
    {
      return actions.Values.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return actions.Values.GetEnumerator();
    }
  }
}
