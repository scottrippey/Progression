using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Progression.Extras;

namespace Progression.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Start a background thread:
            Action background = BackgroundTask;
            background.BeginInvoke(null, null);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.isExiting = true;
        }

        private ETACalculator eta1 = new ETACalculator(2,3.0);
        private ETACalculator eta2 = new ETACalculator(2,3.0);
        private ETACalculator eta3 = new ETACalculator(2,3.0);

        public void ProgressChanged(ProgressChangedInfo p)
        {
            var c = p.CurrentTask;
            switch (c.TaskKey)
            {
                case "Primary":
                    eta2.Reset();
                    break;
                case "Second":
                    eta3.Reset();
                    break;
                case "Third":
                    break;
            }
            var ui = new[] {
                               new {ETA = eta1, Progress = progressBar1, Label = label1},
                               new {ETA = eta2, Progress = progressBar2, Label = label2},
                               new {ETA = eta3, Progress = progressBar3, Label = label3},
                           };
            for (int i = 0; i < p.Count; i++)
            {
                ui[i].ETA.Update(p[i].Progress);
                ui[i].Progress.Value = p[i].Progress*100d;
                ui[i].Label.Content = ui[i].ETA.ETAIsAvailable ? string.Format("{0:00}% done\t{1:0.0} seconds remaining\t(ETA is {2:h:mm:ss})", p[i].Progress * 100f, ui[i].ETA.ETR.TotalSeconds, ui[i].ETA.ETA) : "Calculating...";
            }

        }
        public void ProgressReset()
        {
            eta1.Reset();
            eta2.Reset();
            eta3.Reset();
        }

        private bool isExiting = false;
        public void BackgroundTask()
        {
            const int primary = 10;
            const int second = 10;
            const int third = 10;
            while (true)
            {
                Dispatcher.Invoke((Action)ProgressReset);
                using (Progress.BeginFixedTask(primary).SetTaskKey("Primary").SetCallback((p)=> Dispatcher.Invoke((ProgressChangedHandler)ProgressChanged, p)))
                {
                    for (int i = 0; i < primary; i++)
                    {
                        Progress.NextStep();


                        using (Progress.BeginFixedTask(second).SetTaskKey("Second"))
                        {
                            for (int j = 0; j < second; j++)
                            {
                                Progress.NextStep();



                                using (Progress.BeginFixedTask(third).SetTaskKey("Third"))
                                {
                                    for (int k = 0; k < third; k++)
                                    {
                                        Progress.NextStep();


                                        if (isExiting) return;


                                        System.Threading.Thread.Sleep(delay);
                                    }
                                    Progress.EndTask();
                                }





                            }
                            Progress.EndTask();
                        }



                    }
                    Progress.EndTask();
                }
            }
        }

        private int delay = 10;
        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.delay = (int) ((2000 - slider1.Value));
        }

    }
}
