using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public class Analyzer
  {
    private const float kExcellentProbability = .01f;
    private const float kGoodProbability = .25f;
    private const float kNormalProbability = .74f;

    private int maxAnalysisDepth = 6;
    private int numSlowSolved = 0;
    private int numQuickSolved = 0;
    private int numLeafSolved = 0;
    private ActionDatabase actions = new ActionDatabase();
    private Dictionary<State, SolvedScoreNode> solvedStates;

    private uint baseStep = 1;

    public int MaxAnalysisDepth
    {
      get { return maxAnalysisDepth; }
      set { maxAnalysisDepth = value; }
    }
    public ActionDatabase Actions { get { return actions; } }
    public int NumSlowSolved { get { return numSlowSolved; } }
    public int NumQuickSolved { get { return numQuickSolved; } }
    public int NumLeafSolved { get { return numLeafSolved; } }
    public int NumStatesExamined
    {
      get
      {
        return NumSlowSolved + NumQuickSolved + NumLeafSolved;
      }
    }

    public Analyzer()
    {
      solvedStates = new Dictionary<State, SolvedScoreNode>();
    }

    public void Run(UserDecisionNode root)
    {
      // The base step allows us to resume a deep calculation from the middle of the tree, 
      // and ensure we can still travel an *additional* MaxAnalysisDepth nodes deep from
      // an arbitrary starting point.
      baseStep = root.originalState.Step;

      // There's a nasty edge case where, when you run against the actual root of the tree,
      // you can analyze some leaf (let's call it L), but be forced to stop searching that
      // particular branch because it's at MaxAnalysisDepth.  Later in the same analysis
      // you encounter the state L again, but because it was originally a leaf it was not
      // inserted into the solved score node dictionary.  If you then playback the sequence
      // all the way to the *original* L, it will try to solve it again never realizing that
      // it's already been solved.  Checking the dictionary EVERY TIME would fix it, but that
      // would be prohibitively slow.  
      //
      // Most likely we need a way to mark the node as a leaf,
      // and in the main loop double-check all leaf nodes in case they've already been solved.
      // Still, even this presents complications.  If we do find it in the solved score node 
      // lookup, then what do we do?  I suppose one solution is to replace its SolvedScoreNode
      // entry with the new one after we solve it, since theoretically it will be more accurate
      // than the other one.  It's not clear what the best strategy is.
      //
      // Unfortunately, the current approach means that replaying an entire synth from the beginning
      // will cause the entire thing to be re-evaluated and ultimately end up being slow, whereas
      // before it could re-use the previous cache for the replay operation.  We should address this
      // soon since this is important.
      solvedStates.Clear();
      numSlowSolved = numLeafSolved = numQuickSolved = 0;

      foreach (Action action in actions)
      {
        if (!action.CanUse(root.originalState))
          continue;

        PreRandomDecisionNode outcome = new PreRandomDecisionNode();
        outcome.originatingAction = action;
        outcome.originalState = root.originalState;
        root.choices.Add(outcome);
      }

      ExpandInteractions(root);
    }

    private void ExpandInteractions(UserDecisionNode interaction)
    {
      foreach (PreRandomDecisionNode choice in interaction.choices)
      {
        // This represents a user choice, so the probability hasn't changed yet.
        ExpandRandomOutcomes(choice);
      }

      // After expanding all the child user interactions, this node can be solved by
      // choosing the best child action.
      PreRandomDecisionNode optimal = interaction.OptimalAction;
      interaction.Solve(optimal.Score);
      SolvedScoreNode scoreNode = new SolvedScoreNode(optimal.Score, interaction);
      solvedStates.Add(interaction.originalState, scoreNode);
      ++numSlowSolved;
    }

    private bool TryQuickSolve(PostRandomDecisionNode node)
    {
      // If this state was already solved, just set its score and its follow-up interaction
      // tree to the followup interaction tree of the previously solved state.
      SolvedScoreNode solvedScore = null;
      if (!solvedStates.TryGetValue(node.newState, out solvedScore))
        return false;

      node.Solve(solvedScore.score);
      node.interaction = solvedScore.originalNode;
      ++numQuickSolved;
      return true;
    }

    private void ExpandRandomOutcomes(PreRandomDecisionNode preDecisionNode)
    {
      State stateBeforeOutcome = preDecisionNode.originalState;
      Action action = preDecisionNode.originatingAction;

      preDecisionNode.outcomes = ExpandRandomOutcomes(action, stateBeforeOutcome);

      double finalScore = 0.0f;
      foreach (PostRandomDecisionNode outcome in preDecisionNode.outcomes)
      {
        Debug.Assert(outcome.newState != stateBeforeOutcome);

        if (outcome.newState.Status == SynthesisStatus.IN_PROGRESS)
        {
          // If this node has been solved before, just get the previously calcuated
          // score.
          if (!TryQuickSolve(outcome))
          {
            // This is a new state that we've never seen before.  Generate the list of
            // user interactions that are valid for the current state, and attach a
            // corresponding UserDecisionNode.
            outcome.interaction = new UserDecisionNode();
            outcome.interaction.originalState = outcome.newState;
            foreach (Action newAction in actions)
            {
              if (!newAction.CanUse(outcome.newState))
                continue;

              PreRandomDecisionNode nextTransition = new PreRandomDecisionNode();
              nextTransition.originatingAction = newAction;
              nextTransition.originalState = outcome.interaction.originalState;

              outcome.interaction.choices.Add(nextTransition);
            }

            // If there are no valid actions, then we shouldn't be here because in
            // that case it should be a busted or completed synth, which would have
            // been handled earlier.
            Debug.Assert(outcome.interaction.choices.Count > 0);

            if ((outcome.newState.Step - baseStep + 1) <= MaxAnalysisDepth)
            {
              ExpandInteractions(outcome.interaction);

              // The child interaction should automatically be solved when its recursive
              // descent is complete.
              Debug.Assert(outcome.interaction.IsSolved);
              outcome.Solve(outcome.interaction.Score);
            }
          }
        }
        else
        {
          // Completed and busted synths don't need to go into the hash table, because
          // they are leafs that have no follow-up states. So leave them in the tree
          // as-is but mark them solved.  This saves expensive hash table lookups (to
          // see if they're already solved) as well as keeps the hash table smaller
          // so that important lookups will be faster.
          outcome.Solve(outcome.newState.Score);
          ++numLeafSolved;
        }

        // The outcome may not be solved if we were hit with the max depth limit.  In that case
        // recalculate the score from the state.  That score is just an estimate, but that's ok.
        if (outcome.IsSolved)
          finalScore += outcome.probability * outcome.Score;
        else
          finalScore += outcome.probability * outcome.newState.Score;
      }

      preDecisionNode.Solve(finalScore);
    }

    private void PrintTransition(State s)
    {
      Action leadingAction = s.LeadingAction;
      Debug.WriteLine("CP: {0}/{1}, Dura: {2}/{3}, Progress: {4}/{5}, Quality: {6}/{7} ==> {8}",
                        s.CP, s.MaxCP, s.Durability, s.MaxDurability, s.Progress, s.MaxProgress,
                        s.Quality, s.MaxQuality, leadingAction.Attributes.Name);
    }

    private List<PostRandomDecisionNode> ExpandRandomOutcomes(Action action, State initialState)
    {
      double successProbability = Compute.SuccessRate(action.Attributes.SuccessRate, initialState);
      State successState = action.SuccessState(initialState);
      State failureState = action.FailureState(initialState);

      List<PostRandomDecisionNode> entries = new List<PostRandomDecisionNode>();
      switch (initialState.Condition)
      {
        case Condition.Excellent:
          // Excellent can only go to poor.

          // Success
          if (successState != null)
            entries.Add(new PostRandomDecisionNode(successProbability, successState, Condition.Poor, true));

          // Failure
          if (failureState != null)
            entries.Add(new PostRandomDecisionNode(1.0f - successProbability, failureState, Condition.Poor, false));
          break;
        case Condition.Good:
        case Condition.Poor:
          // Good and poor can both only go to normal.

          // Success
          if (successState != null)
            entries.Add(new PostRandomDecisionNode(successProbability, successState, Condition.Normal, true));

          // Failure
          if (failureState != null)
            entries.Add(new PostRandomDecisionNode(1.0f - successProbability, failureState, Condition.Normal, false));
          break;
        case Condition.Normal:
          // Normal can go to normal, good, or excellent.

          // Success
          if (successState != null)
          {
            entries.Add(new PostRandomDecisionNode(kNormalProbability * successProbability, successState, Condition.Normal, true));
            entries.Add(new PostRandomDecisionNode(kGoodProbability * successProbability, successState, Condition.Good, true));
            entries.Add(new PostRandomDecisionNode(kExcellentProbability * successProbability, successState, Condition.Excellent, true));
          }

          // Failure
          if (failureState != null)
          {
            entries.Add(new PostRandomDecisionNode(kNormalProbability * (1.0f - successProbability), failureState, Condition.Normal, false));
            entries.Add(new PostRandomDecisionNode(kGoodProbability * (1.0f - successProbability), failureState, Condition.Good, false));
            entries.Add(new PostRandomDecisionNode(kExcellentProbability * (1.0f - successProbability), failureState, Condition.Excellent, false));
          }
          break;
      }
      return entries;
    }
  }
}
