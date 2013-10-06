using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulator.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Tests
{
  [TestClass]
  public class PerfTests
  {
    //[TestMethod]
    public void TestBaselinePerformance()
    {
      Analyzer analyzer = new Analyzer();
      analyzer.Actions.AddAction(new BasicSynthesis());
      analyzer.Actions.AddAction(new BasicTouch());
      analyzer.Actions.AddAction(new MastersMend());
      analyzer.Actions.AddAction(new SteadyHand());
      analyzer.Actions.AddAction(new Observe());

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

      analyzer.Run(root);

      int baselineLeafSolved = 501426;
      int baselineSlowSolved = 228390;
      //int baselineQuickSolved = 724119;
      Assert.IsTrue(analyzer.NumSlowSolved <= baselineSlowSolved, "The number of slow solutions has regressed.");
      Assert.IsTrue(analyzer.NumSlowSolved + analyzer.NumLeafSolved <= baselineSlowSolved + baselineLeafSolved, "The total number of nodes solved has regressed.");
    }
  }
}
