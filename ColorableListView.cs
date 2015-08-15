using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileSaver
{
    class ColorableListView : ListView
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            var lvi = (ListViewItem)element;
            var model = (FileViewModel)item;
            lvi.Background = model.Background;
        }
    }
}
