using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace TubeSheetRobot
{
    public class TubesheetModel
    {
        public ObservableCollection<TubeM> Tubes { get; set; }
        public double Diameter { get; set; }
        public double Pitch { get; set; }
        public int NumRows { get; private set; }
        public int NumCols { get; private set; }
        public Model Model { get; set; }

        public TubesheetModel(double diameter, double pitch, IList<TubeM> tubes)
        {
            Diameter = Diameter;
            Pitch = pitch;
            Tubes = new ObservableCollection<TubeM>(tubes.OrderByDescending(t => t.Row).ThenBy(t => t.Column));
            Model = Tubes.FirstOrDefault().modelT;
            Model.ConnectTubesheet(this);
            NumRows = Tubes.Max(t => t.Row);
            NumCols = Tubes.Max(t => t.Column);

            Debug.Assert(NumRows == NumCols);
        }
    }
}
