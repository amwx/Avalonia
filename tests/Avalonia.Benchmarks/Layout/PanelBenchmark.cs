using System.Collections.Generic;
using Avalonia.Controls;
using BenchmarkDotNet.Attributes;

namespace Avalonia.Benchmarks.Layout
{
    [MemoryDiagnoser]
    public class PanelBenchmark
    {
        private readonly Panel _panel;
        private readonly List<List<IControl>> _controls;
        private readonly List<int> _counts;

        public PanelBenchmark()
        {
            _panel = new Panel();

            _counts = new List<int>
            {
                1,
                5,
                10,
                20,
                50,
                100
            };

            _controls = new List<List<IControl>>();

            foreach (int count in _counts)
            {
                var controls = new List<IControl>(count);

                for (int i = 0; i < count; i++)
                {
                    controls.Add(new Button());
                }

                _controls.Add(controls);
            }
        }

        public IEnumerable<int> ControlIndex()
        {
            for (int i = 0; i < _counts.Count; i++)
            {
                yield return i;
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(ControlIndex))]
        public void PanelAddSingle(int index)
        {
            _panel.Children.Clear();

            var controls = _controls[index];

            foreach (IControl control in controls)
            {
                _panel.Children.Add(control);
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(ControlIndex))]
        public void PanelAddRange(int index)
        {
            _panel.Children.Clear();

            var controls = _controls[index];

            _panel.Children.AddRange(controls);
        }
    }
}
