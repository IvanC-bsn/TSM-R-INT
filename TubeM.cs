using System;
using System.ComponentModel;

namespace TubeSheetRobot
{
    public class TubeM : INotifyPropertyChanged
    {
        private double diameter;
        private double pitch;
        public Model modelT;
        public TubeM(double diameter, double pitch, Model model)
        {
            this.diameter = diameter;
            this.pitch = pitch;
            this.modelT = model;
        }
        public int Row { get; set; }
        public int Column { get; set; }
        private string _ColorProp;

        public event PropertyChangedEventHandler PropertyChanged;

        public string ColorProp
        {
            get { return _ColorProp; }
            set { _ColorProp = Convert(value); OnPropertyChanged("ColorProp"); }
        }
        public string Convert(string property)
        {
            switch (property)
            {
                case "Unknown":
                    return "Gray";
                case "Plugged":
                    return "Black";
                case "Critical":
                    return "Red";
                default:
                    return property;
            }
        }
        public string ToolTip
        {
            get
            {
                return String.Format("R{0}C{1}", Row, Column);
            }
        }
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
