using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWindows.Utils
{
    public class LazyData<T>
    {
        private Func<T> GetData;
        protected T _Data;
        public bool IsLoaded { get; private set; } = false;
        public virtual T Data { get => IsLoaded?_Data:(_Data=GetData.Invoke()); }

        public LazyData(Func<T> Action)
        {
            if (Action == null) throw new Exception("不能让Action为null");
            this.GetData= Action;
        }


    }
}
