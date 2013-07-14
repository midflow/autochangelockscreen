using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AutoChangeLockScreen
{

    public partial class CustomUserControl : UserControl
    {
        //private bool _isSelected = false;
        //public static readonly DependencyProperty BindingTextValueProperty = DependencyProperty.Register(
        //                                  "IsSelected",
        //                                  typeof(bool),
        //                                  typeof(CustomUserControl),
        //                                  null);
        //public bool IsSelected
        //{
        //    get;
        //    set;
        //    //get { return (bool) GetValue(BindingTextValueProperty); }
        //    //set
        //    //{
        //    //    //_isSelected = value;
        //    //    //NotifyPropertyChanged("IsSelected");
        //    //    SetSelectedValue(BindingTextValueProperty, value);
        //    //}
        //}

        //private void SetSelectedValue(DependencyProperty property, object value, 
        //    [System.Runtime.CompilerServices.CallerMemberName] string b = null)
        //{
        //    SetValue(property, value);
        //    if (PropertyChanged != null)
        //        PropertyChanged(this, new PropertyChangedEventArgs(b));
        //}

        public CustomUserControl()
        {
            InitializeComponent();
            //(this.Content as FrameworkElement).DataContext = this;
            DataContext = this;
        }

        //public event PropertyChangedEventHandler PropertyChanged;

        //// NotifyPropertyChanged will raise the PropertyChanged event passing the
        //// source property that is being updated.
        //public void NotifyPropertyChanged(string PropName)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(PropName));
        //    }
        //}  

        private void stackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //if (!IsSelected)
            //{
            //    VisualStateManager.GoToState(this, "Selected", true);
            //}
            //else
            //{
            //    VisualStateManager.GoToState(this, "Normal", true);
            //}
            //IsSelected = !IsSelected;
        }
    }
}
