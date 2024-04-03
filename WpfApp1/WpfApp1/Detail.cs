using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Detail
    {
        public List<ReactivePropertySlim<bool>> Values { get; } = [];

        public Detail(int size)
        {
            for (int i = 0; i < size; i++)
            {
                Values.Add(new ReactivePropertySlim<bool>(false));
            }
        }

        public void Invert(int index)
        {
            Values[index].Value = !Values[index].Value;
        }

        public void SetAll(bool value)
        {
            Values.ForEach(x => x.Value = value);
        }

        public void SetOn(int index)
        {
            Values[index].Value = true;
        }
    }
}
