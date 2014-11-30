using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using Telerik.Windows.Media;
using System.Windows;
namespace AutoChangeLockScreen
{
    public class CustomUISelector : ImageEditorToolUISelector
    {
        public DataTemplate NokiaToolUI
        {
            get;
            set;
        }

        public override System.Windows.DataTemplate SelectTemplate(object tool, System.Windows.DependencyObject container)
        {
            if (tool is NokiaTool)
            {
                return this.NokiaToolUI;
            }

            return base.SelectTemplate(tool, container);
        }
    }
}
