using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator.Engine;
using System.Diagnostics;

namespace Simulator
{
  class SolutionPlayer
  {
    Analyzer analyzer;

    enum PlaybackControl
    {
      RESTART_PLAYBACK,
      EXIT_PLAYBACK,
    }

    public SolutionPlayer(Analyzer analyzer)
    {
      this.analyzer = analyzer;
    }
    public void Play(State initialState)
    {
      UserDecisionNode root = new UserDecisionNode();
      root.originalState = initialState;

      PlaybackControl control = PlaybackControl.RESTART_PLAYBACK;

      while (control == PlaybackControl.RESTART_PLAYBACK)
        control = PlaySingleSequence(root);
    }

    private PlaybackControl PlaySingleSequence(UserDecisionNode root)
    {
      while (root.originalState.Status == SynthesisStatus.IN_PROGRESS)
      {
        if (!root.IsSolved)
        {
          System.Console.WriteLine("Encountered unsolved state.  Analyzing...");
          Stopwatch stopwatch = new Stopwatch();
          stopwatch.Start();
          analyzer.Run(root);
          stopwatch.Stop();
          System.Console.WriteLine("Solved {0} states in {1} seconds.", analyzer.NumStatesExamined, stopwatch.Elapsed.TotalSeconds);
        }

        PreRandomDecisionNode optimalAction = root.OptimalAction;

        PrintPlaybackStatus(root.originalState, optimalAction);

        bool success = false;
        Condition newCondition = Condition.Normal;
        string result = String.Empty;
        while (!ParseResultEntry(result, root.originalState, out success, out newCondition))
        {
          System.Console.Write("Enter result as two letters: ");
          result = System.Console.ReadLine();
          if (result.Length == 0)
          {
            while (result.ToUpper() != "Y" && result.ToUpper() != "N")
            {
              System.Console.Write("Quit this playback session and replay from the beginning?  (Y/N) ");
              result = System.Console.ReadLine();
              if (result.ToUpper() == "Y")
                return PlaybackControl.RESTART_PLAYBACK;
            }
          }
        }

        PostRandomDecisionNode outcomeNode = optimalAction.FindMatchingOutcome(success, newCondition);
        root = outcomeNode.interaction;
        if (root == null)
        {
          System.Console.WriteLine("Could not find a matching entry for this outcome.  Quit this playback session and replay from the beginning?  (Y/N) ");
          result = System.Console.ReadLine();
          if (result.ToUpper() == "Y")
            return PlaybackControl.RESTART_PLAYBACK;
          else
            return PlaybackControl.EXIT_PLAYBACK;
        }
      }
      return PlaybackControl.EXIT_PLAYBACK;
    }

    private void PrintPlaybackStatus(State state, PreRandomDecisionNode bestChoice)
    {
      System.Console.WriteLine("Condition={9}, Progress {0}/{1}, Quality={2}/{3}, CP={4}/{5}, Dura={6}/{7}.  Best Action = {8}",
                               state.Progress, state.MaxProgress, state.Quality, state.MaxQuality,
                               state.CP, state.MaxCP, state.Durability, state.MaxDurability,
                               bestChoice.originatingAction.Attributes.Name, state.Condition);
    }

    private bool ParseResultEntry(string result, State currentState, out bool success, out Condition condition)
    {
      success = false;
      condition = Condition.Normal;

      if (result.Length != 2)
        return false;

      char successCode = result[0];
      char conditionCode = result[1];

      switch (successCode)
      {
        case 's':
        case 'S':
          success = true;
          break;
        case 'f':
        case 'F':
          success = false;
          break;
        default:
          return false;
      }

      switch (conditionCode)
      {
        case 'g':
        case 'G':
          condition = Condition.Good;
          break;
        case 'p':
        case 'P':
          condition = Condition.Poor;
          break;
        case 'e':
        case 'E':
          condition = Condition.Excellent;
          break;
        case 'n':
        case 'N':
          condition = Condition.Normal;
          break;
        default:
          return false;
      }

      if (!IsValidConditionTransition(currentState.Condition, condition))
        return false;
      return true;
    }

    private bool IsValidConditionTransition(Condition oldCondition, Condition newCondition)
    {
      if (oldCondition == Condition.Poor && newCondition != Condition.Normal)
        return false;
      if (oldCondition == Condition.Good && newCondition != Condition.Normal)
        return false;
      if (oldCondition == Condition.Excellent && newCondition != Condition.Poor)
        return false;
      if (oldCondition == Condition.Normal && newCondition == Condition.Poor)
        return false;
      return true;
    }
  }
}
