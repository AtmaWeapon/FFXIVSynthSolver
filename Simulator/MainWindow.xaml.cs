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
    private State currentState;
    private RadioButton selectedRadio;

    private State initialState = null;

    private List<CheckBox> abilityChecks = null;

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
      currentState = null;
      abilityChecks = new List<CheckBox>();

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

      analyzer.MaxAnalysisDepth = 8;

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
      currentState = new State(initialState);

      UpdateUIState(initialState);

      txtAnalysisDepth.Text = analyzer.MaxAnalysisDepth.ToString();
      txtControl.Text = initialState.Control.ToString();
      txtCP.Text = initialState.MaxCP.ToString();
      txtCrafterLevel.Text = initialState.CrafterLevel.ToString();
      txtCraftsmanship.Text = initialState.Craftsmanship.ToString();
      txtInitialQuality.Text = initialState.Quality.ToString();
      txtRecipeDurability.Text = initialState.Durability.ToString();
      txtRecipeDifficulty.Text = initialState.MaxProgress.ToString();
      txtRecipeLevel.Text = initialState.SynthLevel.ToString();
      txtRecipeQuality.Text = initialState.MaxQuality.ToString();

      SetAppState(AppState.Idle);
    }

    private void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
      if (appState == AppState.Idle)
      {
        currentState = new State(initialState);
      }

      Simulator.Engine.Ability bestAction;
      if (analyzer.TryGetOptimalAction(currentState, out bestAction))
      {
        SetAppState(AppState.Playback);
        AdvancePlayback();
      }
      else
      {
        SetAppState(AppState.Analyzing);

        worker.RunWorkerAsync(currentState);
      }
    }

    private void UpdateUIStateForPlayback(State state, Simulator.Engine.Ability bestAction)
    {
      UpdateUIState(state);

      if (bestAction != null)
        lblBestAction.Content = bestAction.Name;
      else
        lblBestAction.Content = String.Empty;

      switch (state.Condition)
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
          radioSuccessExcellent.IsEnabled = false;
          radioSuccessGood.IsEnabled = false;
          radioFailureNormal.IsEnabled = false;
          radioSuccessNormal.IsEnabled = false;

          if (selectedRadio != null)
            selectedRadio.IsChecked = false;
          break;
        case Engine.Condition.Normal:
          radioFailurePoor.IsEnabled = false;
          radioSuccessPoor.IsEnabled = false;

          radioFailureExcellent.IsEnabled = true;
          radioFailureGood.IsEnabled = true;
          radioSuccessExcellent.IsEnabled = true;
          radioSuccessGood.IsEnabled = true;
          radioFailureNormal.IsEnabled = true;
          radioSuccessNormal.IsEnabled = true;

          if (selectedRadio != null)
            selectedRadio.IsChecked = false;
          break;
      }

      if (bestAction == null || state.Status != SynthesisStatus.IN_PROGRESS)
      {
        radioFailureExcellent.IsEnabled = false;
        radioFailureGood.IsEnabled = false;
        radioFailureNormal.IsEnabled = false;
        radioFailurePoor.IsEnabled = false;
        radioSuccessExcellent.IsEnabled = false;
        radioSuccessGood.IsEnabled = false;
        radioSuccessNormal.IsEnabled = false;
        radioSuccessPoor.IsEnabled = false;
      }
      else if (!bestAction.CanFail)
      {
        radioFailureExcellent.IsEnabled = false;
        radioFailureGood.IsEnabled = false;
        radioFailureNormal.IsEnabled = false;
        radioFailurePoor.IsEnabled = false;
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

      lblCP.Content = progressCP.Value = state.CP;
      lblMaxCP.Content = progressCP.Maximum = state.MaxCP;
      
      lblDurability.Content = state.Durability;
      lblMaxDurability.Content = state.MaxDurability;

      lblCondition.Content = state.Condition.ToString();
      lblStep.Content = state.Step;
      lblFailureChance.Content = state.FailureProbability.ToString("P2");
      lblCurrentScore.Content = state.Score.ToString("F3");
      lblBestAction.Content = "<Unknown>";
    }

    private void AdvancePlayback()
    {
      Simulator.Engine.Ability optimalAction = analyzer.OptimalAction(currentState);

      RadioParams selectedParams = (RadioParams)selectedRadio.Tag;
      string statusString = (selectedParams.success) ? "Success" : "Failure";
      string conditionString = selectedParams.condition.ConditionString();

      currentState = optimalAction.Activate(currentState, selectedParams.success);
      currentState.Condition = selectedParams.condition;

      if (currentState.Status == SynthesisStatus.IN_PROGRESS)
      {
        if (!analyzer.TryGetOptimalAction(currentState, out optimalAction))
        {
          // We hit a leaf.  Kick off a new analysis.
          SetAppState(AppState.Analyzing);
          worker.RunWorkerAsync(currentState);
        }
        else
          UpdateUIStateForPlayback(currentState, optimalAction);
      }
      else
        UpdateUIStateForPlayback(currentState, null);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
      if (appState == AppState.Idle)
        Close();
      else
      {
        SetAppState(AppState.Idle);
        currentState = initialState;
        UpdateUIState(currentState);
      }
    }

    void worker_StartAnalysis(object sender, DoWorkEventArgs e)
    {
      State state = (State)e.Argument;
      stopwatch.Reset();
      stopwatch.Start();
      analyzer.Run(state);
      e.Result = state;
    }
    
    void worker_AnalysisComplete(object sender, RunWorkerCompletedEventArgs e)
    {
      stopwatch.Stop();
      State state = (State)e.Result;

      lblStatus.Content = String.Format("Solved {0:N0} states in {1:F2} seconds.", analyzer.NumStatesExamined, stopwatch.Elapsed.TotalSeconds);
      currentState = state;

      SetAppState(AppState.Playback);
    }

    private void radio_Checked(object sender, RoutedEventArgs e)
    {
      selectedRadio = (RadioButton)sender;
      AdvancePlayback();
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

          // Re-enable parameter entry.
          txtInitialQuality.IsEnabled = true;
          txtCraftsmanship.IsEnabled = true;
          txtCrafterLevel.IsEnabled = true;
          txtCP.IsEnabled = true;
          txtControl.IsEnabled = true;
          txtAnalysisDepth.IsEnabled = true;
          txtRecipeDifficulty.IsEnabled = true;
          txtRecipeDurability.IsEnabled = true;
          txtRecipeLevel.IsEnabled = true;
          txtRecipeQuality.IsEnabled = true;
          comboBoxCraft.IsEnabled = false;
          comboBoxRecipe.IsEnabled = false;

          // Disable random outcome entry.
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

          // Disable parameter entry.
          txtInitialQuality.IsEnabled = false;
          txtCraftsmanship.IsEnabled = false;
          txtCrafterLevel.IsEnabled = false;
          txtCP.IsEnabled = false;
          txtControl.IsEnabled = false;
          txtAnalysisDepth.IsEnabled = false;
          comboBoxCraft.IsEnabled = false;
          comboBoxRecipe.IsEnabled = false;
          txtRecipeDifficulty.IsEnabled = false;
          txtRecipeDurability.IsEnabled = false;
          txtRecipeLevel.IsEnabled = false;
          txtRecipeQuality.IsEnabled = false;

          // Disable random outcome entry.
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

          // Disable parameter entry.
          txtInitialQuality.IsEnabled = false;
          txtCraftsmanship.IsEnabled = false;
          txtCrafterLevel.IsEnabled = false;
          txtCP.IsEnabled = false;
          txtControl.IsEnabled = false;
          txtAnalysisDepth.IsEnabled = false;
          comboBoxCraft.IsEnabled = false;
          comboBoxRecipe.IsEnabled = false;
          txtRecipeDifficulty.IsEnabled = false;
          txtRecipeDurability.IsEnabled = false;
          txtRecipeLevel.IsEnabled = false;
          txtRecipeQuality.IsEnabled = false;

          // Enable random outcome entry.
          radioFailureExcellent.IsEnabled = true;
          radioFailureGood.IsEnabled = true;
          radioFailureNormal.IsEnabled = true;
          radioFailurePoor.IsEnabled = true;
          radioSuccessExcellent.IsEnabled = true;
          radioSuccessGood.IsEnabled = true;
          radioSuccessNormal.IsEnabled = true;
          radioSuccessPoor.IsEnabled = true;

          Simulator.Engine.Ability bestAction = analyzer.OptimalAction(currentState);
          UpdateUIStateForPlayback(currentState, bestAction);
          break;
      }

      appState = newState;
    }

    private void txtInitialQuality_TextChanged(object sender, TextChangedEventArgs e)
    {
      uint value;
      if (uint.TryParse(txtInitialQuality.Text, out value))
      {
        initialState.Quality = value;
        lblQuality.Content = value;
        progressQuality.Value = value;
      }
    }

    private void txtCP_TextChanged(object sender, TextChangedEventArgs e)
    {
      uint value;
      if (uint.TryParse(txtCP.Text, out value))
      {
        initialState.CP = initialState.MaxCP = value;
        lblMaxCP.Content = lblCP.Content = value;
        progressCP.Maximum = progressCP.Value = value;
      }
    }

    private void txtRecipeDurability_TextChanged(object sender, TextChangedEventArgs e)
    {
      uint value;
      if (uint.TryParse(txtRecipeDurability.Text, out value))
      {
        initialState.Durability = initialState.MaxDurability = value;
        lblMaxDurability.Content = value;
        lblDurability.Content = value;
      }
    }

    private void txtRecipeQuality_TextChanged(object sender, TextChangedEventArgs e)
    {
      uint value;
      if (uint.TryParse(txtRecipeQuality.Text, out value))
      {
        initialState.MaxQuality = value;
        lblMaxQuality.Content = value;
        progressQuality.Maximum = value;
      }
    }

    private void txtRecipeDifficulty_TextChanged(object sender, TextChangedEventArgs e)
    {
      uint value;
      if (uint.TryParse(txtRecipeDifficulty.Text, out value))
      {
        initialState.MaxProgress = value;
        lblMaxProgress.Content = value;
        progressProgress.Maximum = value;
      }
    }

    private void txtAnalysisDepth_TextChanged(object sender, TextChangedEventArgs e)
    {
      uint value;
      if (uint.TryParse(txtAnalysisDepth.Text, out value))
      {
        analyzer.MaxAnalysisDepth = (int)value;
      }
    }

    private void AbilityCheck_CheckChanged(object sender, RoutedEventArgs e)
    {
      CheckBox check = (CheckBox)sender;
      if (check.Tag != null)
      {
        Simulator.Engine.Ability action = (Simulator.Engine.Ability)check.Tag;
        if (action != null)
        {
          if (check.IsChecked.Value)
            analyzer.Actions.AddAction(action);
          else
            analyzer.Actions.RemoveAction(action.GetType());
        }
      }
    }

    private void AbilityCheck_Loaded(object sender, RoutedEventArgs e)
    {
      abilityChecks.Add((CheckBox)sender);
    }
  }
}
