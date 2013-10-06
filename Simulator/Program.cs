using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simulator.Engine;
using System.Diagnostics;

namespace Simulator
{
  class Program
  {

    static void Main(string[] args)
    {
      Analyzer analyzer = new Analyzer();
      analyzer.Actions.AddAction(new BasicSynthesis());
      analyzer.Actions.AddAction(new BasicTouch());
      analyzer.Actions.AddAction(new MastersMend());
      analyzer.Actions.AddAction(new SteadyHand());
      analyzer.Actions.AddAction(new Observe());
      analyzer.Actions.AddAction(new Manipulation());
      analyzer.Actions.AddAction(new TricksOfTheTrade());


      State initialState = new State();
      initialState.Condition = Condition.Normal;
      initialState.Control = 102;
      initialState.Craftsmanship = 105;
      initialState.CP = 233;
      initialState.MaxCP = 233;
      initialState.MaxDurability = 70;
      initialState.Durability = 70;
      initialState.MaxProgress = 63;
      initialState.MaxQuality = 866;
      initialState.SynthLevel = 16;
      initialState.CrafterLevel = 16;

      analyzer.MaxAnalysisDepth = 12;

      SolutionPlayer player = new SolutionPlayer(analyzer);
      player.Play(initialState);
      System.Console.WriteLine("Press any key to exit...");
      System.Console.ReadKey();

    }
  }
}
