using Simulator.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Simulator
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private Analyzer analyzer;
    private Stopwatch stopwatch;
    private BackgroundWorker worker;
    private AppState appState;
    private UserDecisionNode activeNode;
    private RadioButton selectedRadio;

    private State initialState = null;

    private class RadioParams
    {
      public RadioParams(bool success, Engine.Condition condition)
      {
        this.success = success;
        this.condition = condition;
      }
      public bool success;
      public Engine.Condition condition;
    }

    private enum AppState
    {
      Uninitialized,
      Idle,
      Analyzing,
      Playback
    }

    public MainWindow()
    {
      analyzer = new Analyzer();
      stopwatch = new Stopwatch();
      worker = new BackgroundWorker();
      appState = AppState.Uninitialized;
      activeNode = null;

      worker.DoWork += worker_StartAnalysis;
      worker.RunWorkerCompleted += worker_AnalysisComplete;

      InitializeComponent();
    }

    private void Window_Initialized(object sender, EventArgs e)
    {
      radioSuccessExcellent.Tag = new RadioParams(true, Engine.Condition.Excellent);
      radioSuccessGood.Tag = new RadioParams(true, Engine.Condition.Good);
      radioSuccessNormal.Tag = new RadioParams(true, Engine.Condition.Normal);
      radioSuccessPoor.Tag = new RadioParams(true, Engine.Condition.Poor);
      radioFailureExcellent.Tag = new RadioParams(false, Engine.Condition.Excellent);
      radioFailureGood.Tag = new RadioParams(false, Engine.Condition.Good);
      radioFailureNormal.Tag = new RadioParams(false, Engine.Condition.Normal);
      radioFailurePoor.Tag = new RadioParams(false, Engine.Condition.Poor);

      analyzer.Actions.AddAction(new BasicSynthesis());
      analyzer.Actions.AddAction(new BasicTouch());
      analyzer.Actions.AddAction(new MastersMend());
      analyzer.Actions.AddAction(new SteadyHand());
      analyzer.Actions.AddAction(new Observe());
      analyzer.Actions.AddAction(new Simulator.Engine.Manipulation());
      analyzer.Actions.AddAction(new TricksOfTheTrade());
      analyzer.Actions.AddAction(new StandardTouch());

      analyzer.MaxAnalysisDepth = 8;

      //txtStatusLog.Clear();
      initialState = new State();
      initialState.Condition = Simulator.Engine.Condition.Normal;
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

      UpdateUIState(initialState);

      SetAppState(AppState.Idle);
    }

    private void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
      if (appState == AppState.Idle)
      {
        activeNode = new UserDecisionNode();
        activeNode.originalState = initialState;
      }

      if (activeNode.IsSolved)
      {
        ChooseOptimalAction(false);
      }
      else
      {
        SetAppState(AppState.Analyzing);

        worker.RunWorkerAsync(activeNode);
      }
    }

    private void UpdateUIState(UserDecisionNode node)
    {
      Simulator.Engine.Action optimalAction = node.OptimalAction.originatingAction;
      State state = activeNode.originalState;
      UpdateUIState(state);

      lblBestAction.Content = optimalAction.Attributes.Name;

      switch (activeNode.originalState.Condition)
      {
        case Engine.Condition.Poor:
        case Engine.Condition.Good:
          radioFailureExcellent.IsEnabled = false;
          radioFailureGood.IsEnabled = false;
          radioFailurePoor.IsEnabled = false;
          radioSuccessExcellent.IsEnabled = false;
          radioSuccessGood.IsEnabled = false;
          radioSuccessPoor.IsEnabled = false;

          radioFailureNormal.IsEnabled = true;
          radioSuccessNormal.IsEnabled = true;

          if (selectedRadio != null)
            selectedRadio.IsChecked = false;
          break;
        case Engine.Condition.Excellent:
          radioFailurePoor.IsEnabled = true;
          radioSuccessPoor.IsEnabled = true;

          radioFailureExcellent.IsEnabled = false;
          radioFailureGood.IsEnabled = false;
          radioFailureNormal.IsEnabled = false;
          radioSuccessExcellent.IsEnabled = false;
          radioSuccessGood.IsEnabled = false;
          radioSuccessNormal.IsEnabled = false;

          if (selectedRadio != null)
            selectedRadio.IsChecked = false;
          break;
        case Engine.Condition.Normal:
          radioFailureExcellent.IsEnabled = true;
          radioFailureGood.IsEnabled = true;
          radioFailureNormal.IsEnabled = true;
          radioSuccessExcellent.IsEnabled = true;
          radioSuccessGood.IsEnabled = true;
          radioSuccessNormal.IsEnabled = true;

          radioFailurePoor.IsEnabled = false;
          radioSuccessPoor.IsEnabled = false;

          if (selectedRadio != null)
            selectedRadio.IsChecked = false;
          break;
      }
    }

    private void UpdateUIState(State state)
    {
      //txtStatusLog.AppendText(String.Format("Condition={9}, Progress {0}/{1}, Quality={2}/{3}, CP={4}/{5}, Dura={6}/{7}.  Best Action = {8}\n",
      //                        state.Progress, state.MaxProgress, state.Quality, state.MaxQuality,
      //                        state.CP, state.MaxCP, state.Durability, state.MaxDurability,
      //                        optimalAction.Attributes.Name, state.Condition));
      lblQuality.Content = progressQuality.Value = state.Quality;
      lblMaxQuality.Content = progressQuality.Maximum = state.MaxQuality;

      lblProgress.Content = progressProgress.Value = state.Progress;
      lblMaxProgress.Content = progressProgress.Maximum = state.MaxProgress;
      lblCondition.Content = state.Condition.ToString();
      //lblCP.Content = String.Format("{0}/{1}", state.CP, state.MaxCP);
      lblDurability.Content = state.Durability;
      lblMaxDurability.Content = state.MaxDurability;
      lblStep.Content = state.Step;
      lblFailureChance.Content = state.FailureProbability.ToString("P2");
      lblCurrentScore.Content = state.Score.ToString("F3");
      lblBestAction.Content = "<Unknown>";
    }

    private void ChooseOptimalAction(bool initializing)
    {
      PreRandomDecisionNode optimalAction = activeNode.OptimalAction;

      RadioParams selectedParams = (RadioParams)selectedRadio.Tag;
      string statusString = (selectedParams.success) ? "Success" : "Failure";
      string conditionString = selectedParams.condition.ConditionString();
      //txtStatusLog.AppendText(String.Format("{0}!  New condition = {1}.\n", statusString, conditionString));
      UserDecisionNode newActiveNode = optimalAction.FindMatchingOutcome(selectedParams.success, selectedParams.condition);
      if (newActiveNode == null)
        SetAppState(AppState.Idle);
      else
      {
        activeNode = newActiveNode;
        UpdateUIState(newActiveNode);
        if (!newActiveNode.IsSolved)
        {
          SetAppState(AppState.Analyzing);
          worker.RunWorkerAsync(activeNode);
        }
      }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
      if (appState == AppState.Idle)
        Close();
      else
        SetAppState(AppState.Idle);
    }

    void worker_StartAnalysis(object sender, DoWorkEventArgs e)
    {
 	    UserDecisionNode root = (UserDecisionNode)e.Argument;
      stopwatch.Reset();
      stopwatch.Start();
      analyzer.Run(root);
      e.Result = root;
    }
    
    void worker_AnalysisComplete(object sender, RunWorkerCompletedEventArgs e)
    {
      stopwatch.Stop();
      State state = activeNode.originalState;

      //txtStatusLog.AppendText(String.Format("Solved {0} states in {1} seconds.\n", analyzer.NumStatesExamined, stopwatch.Elapsed.TotalSeconds));
      activeNode = (UserDecisionNode)e.Result;

      SetAppState(AppState.Playback);
    }

    private void radio_Checked(object sender, RoutedEventArgs e)
    {
      selectedRadio = (RadioButton)sender;
      ChooseOptimalAction(false);
    }

    private void SetAppState(AppState newState)
    {
      if (appState == newState)
        return;

      switch (newState)
      {
        case AppState.Idle:
          btnAccept.Content = "Solve!";
          btnAccept.IsEnabled = true;
          btnCancel.Content = "Exit";
          btnCancel.IsEnabled = true;

          radioFailureExcellent.IsEnabled = false;
          radioFailureGood.IsEnabled = false;
          radioFailureNormal.IsEnabled = false;
          radioFailurePoor.IsEnabled = false;
          radioSuccessExcellent.IsEnabled = false;
          radioSuccessGood.IsEnabled = false;
          radioSuccessNormal.IsEnabled = false;
          radioSuccessPoor.IsEnabled = false;
          break;
        case AppState.Analyzing:
          btnAccept.Content = "Analyzing...";
          btnAccept.IsEnabled = false;
          btnCancel.Content = "Cancel Analysis";
          btnCancel.IsEnabled = true;

          radioFailureExcellent.IsEnabled = false;
          radioFailureGood.IsEnabled = false;
          radioFailureNormal.IsEnabled = false;
          radioFailurePoor.IsEnabled = false;
          radioSuccessExcellent.IsEnabled = false;
          radioSuccessGood.IsEnabled = false;
          radioSuccessNormal.IsEnabled = false;
          radioSuccessPoor.IsEnabled = false;
          break;
        case AppState.Playback:
          btnAccept.IsEnabled = false;
          btnAccept.Content = "Playing Back";
          btnCancel.Content = "Cancel Playback";
          btnCancel.IsEnabled = true;

          radioFailureExcellent.IsEnabled = true;
          radioFailureGood.IsEnabled = true;
          radioFailureNormal.IsEnabled = true;
          radioFailurePoor.IsEnabled = true;
          radioSuccessExcellent.IsEnabled = true;
          radioSuccessGood.IsEnabled = true;
          radioSuccessNormal.IsEnabled = true;
          radioSuccessPoor.IsEnabled = true;

          UpdateUIState(activeNode);
          break;
      }

      appState = newState;
    }
  }
}
