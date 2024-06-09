using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace system9
{
    public partial class MainWindow : Window
    {
        private Mutex mutex = new Mutex();
        private int[] dataArray;
        private bool isModified = false;
        private int maxValue = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ModifiedArrayOutput.Clear();
            MaxValueOutput.Clear();
            isModified = false;

            dataArray = new int[] { 1, 2, 3, 4, 5 };

            Task modifyTask = Task.Run(() => ModifyArray());
            Task findMaxTask = Task.Run(() => FindMaxValue());

            Task.WhenAll(modifyTask, findMaxTask).ContinueWith(t =>
            {
                Dispatcher.Invoke(() =>
                {
                    ModifiedArrayOutput.AppendText("Модифікований масив:\n");
                    foreach (var item in dataArray)
                    {
                        ModifiedArrayOutput.AppendText(item + "\n");
                    }
                    MaxValueOutput.Text = $"Максимальне значення в масиві: {maxValue}";
                });
            });
        }

        private void ModifyArray()
        {
            Random random = new Random();
            mutex.WaitOne();

            for (int i = 0; i < dataArray.Length; i++)
            {
                int randomValue = random.Next(1, 10);
                dataArray[i] += randomValue;
            }

            isModified = true;
            mutex.ReleaseMutex();
        }

        private void FindMaxValue()
        {
            while (true)
            {
                mutex.WaitOne();

                if (isModified)
                {
                    maxValue = dataArray[0];
                    for (int i = 1; i < dataArray.Length; i++)
                    {
                        if (dataArray[i] > maxValue)
                        {
                            maxValue = dataArray[i];
                        }
                    }
                    mutex.ReleaseMutex();
                    break;
                }

                mutex.ReleaseMutex();
                Thread.Sleep(100);
            }
        }
    }
}
