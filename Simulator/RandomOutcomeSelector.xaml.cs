using Simulator.Engine;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Simulator
{
  /// <summary>
  /// Interaction logic for RandomOutcomeSelector.xaml
  /// </summary>
  public partial class RandomOutcomeSelector : Window
  {
    private bool success;
    private Engine.Condition condition;
    private State initialState;
    private Simulator.Engine.Action nextAction;

    public RandomOutcomeSelector(State state, Simulator.Engine.Action nextAction)
    {
      this.initialState = state;
      this.nextAction = nextAction;
      InitializeComponent();
    }

    public bool ResultSuccess
    {
      get { return success; }
    }

    public Engine.Condition ResultCondition
    {
      get { return condition; }
    }

    private void radio_Checked(object sender, RoutedEventArgs e)
    {
      if (sender == radioSuccessPoor)
      {
        success = true;
        condition = Engine.Condition.Poor;
      }
      else if (sender == radioSuccessNormal)
      {
        success = true;
        condition = Engine.Condition.Normal;
      }
      else if (sender == radioSuccessGood)
      {
        success = true;
        condition = Engine.Condition.Good;
      }
      else if (sender == radioSuccessExcellent)
      {
        success = true;
        condition = Engine.Condition.Excellent;
      }
      else if (sender == radioFailurePoor)
      {
        success = false;
        condition = Engine.Condition.Poor;
      }
      else if (sender == radioFailureNormal)
      {
        success = false;
        condition = Engine.Condition.Normal;
      }
      else if (sender == radioFailureGood)
      {
        success = false;
        condition = Engine.Condition.Good;
      }
      else if (sender == radioFailureExcellent)
      {
        success = false;
        condition = Engine.Condition.Excellent;
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      switch (initialState.Condition)
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

          radioSuccessNormal.IsChecked = true;
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

          radioSuccessPoor.IsChecked = true;
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

          radioSuccessNormal.IsChecked = true;
          break;
      }
      lblQuality.Content = String.Format("{0}/{1}", initialState.Quality, initialState.MaxQuality);
      lblProgress.Content = String.Format("{0}/{1}", initialState.Progress, initialState.MaxProgress);
      lblCondition.Content = initialState.Condition.ToString();
      lblCP.Content = String.Format("{0}/{1}", initialState.CP, initialState.MaxCP);
      lblDurability.Content = String.Format("{0}/{1}", initialState.Durability, initialState.MaxDurability);
      lblAction.Content = nextAction.Attributes.Name;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
      this.Close();
    }
  }
}
