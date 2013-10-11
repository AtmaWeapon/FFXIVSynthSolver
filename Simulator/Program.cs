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
      analyzer.Actions.AddAction(new StandardTouch());


      State initialState = new State();
      initialState.Condition = Condition.Normal;
      initialState.Control = 119;
      initialState.Craftsmanship = 131;
      initialState.CP = 254;
      initialState.MaxCP = 254;
      initialState.MaxDurability = 70;
      initialState.Durability = 70;
      initialState.MaxProgress = 74;
      initialState.Quality = 284;
      initialState.MaxQuality = 1053;
      initialState.SynthLevel = 20;
      initialState.CrafterLevel = 19;

      analyzer.MaxAnalysisDepth = 8;

      SolutionPlayer player = new SolutionPlayer(analyzer);
      player.Play(initialState);
      System.Console.WriteLine("Press any key to exit...");
      System.Console.ReadKey();

    }
  }
}
