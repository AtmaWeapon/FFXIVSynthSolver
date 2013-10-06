using Simulator.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeImpactTest
{
  class Program
  {
    static void Main(string[] args)
    {
      NodeImpactTest();
    }

    private static void NodeImpactTest()
    {
      Analyzer baselineAnalyzer = new Analyzer();
      baselineAnalyzer.Actions.AddAllActions();

      State initialState = new State();
      initialState.Condition = Condition.Normal;
      initialState.Control = 102;
      initialState.Craftsmanship = 105;
      initialState.CP = 213;
      initialState.MaxCP = 213;
      initialState.MaxDurability = 60;
      initialState.Durability = 60;
      initialState.MaxProgress = 54;
      initialState.MaxQuality = 751;
      initialState.SynthLevel = 14;
      initialState.CrafterLevel = 15;
      UserDecisionNode root = new UserDecisionNode();
      root.originalState = initialState;

      baselineAnalyzer.Run(root);

      NodeImpactTest<BasicTouch>(baselineAnalyzer, initialState);
      NodeImpactTest<MastersMend>(baselineAnalyzer, initialState);
      NodeImpactTest<SteadyHand>(baselineAnalyzer, initialState);
      NodeImpactTest<Observe>(baselineAnalyzer, initialState);
      NodeImpactTest<TricksOfTheTrade>(baselineAnalyzer, initialState);
      NodeImpactTest<HastyTouch>(baselineAnalyzer, initialState);

      System.Console.WriteLine("Press any key to exit...");
      System.Console.ReadLine();
    }

    private static void NodeImpactTest<T>(Analyzer baselineAnalyzer, State initialState)
    {
      Type testImpactType = typeof(T);

      Analyzer analyzer = new Analyzer();
      analyzer.Actions.AddAllActions();
      analyzer.Actions.RemoveAction(testImpactType);

      UserDecisionNode root = new UserDecisionNode();
      root.originalState = initialState;
      analyzer.Run(root);

      int baselineSlow = baselineAnalyzer.NumSlowSolved;
      int testSlow = analyzer.NumSlowSolved;

      int baselineTotal = baselineAnalyzer.NumStatesExamined;
      int testTotal = analyzer.NumStatesExamined;

      float slowImpact = ((float)baselineSlow - (float)testSlow) / (float)baselineSlow;
      float totalImpact = ((float)baselineTotal - (float)testTotal) / (float)baselineTotal;

      SynthActionAttribute attributes = SynthAction<T>.Attributes;
      Console.WriteLine("Action \"{0}\", Slow Impact = {1}, Total Impact = {2}", attributes.Name, slowImpact, totalImpact);
    }
  }
}
