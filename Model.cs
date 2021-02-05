using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace TubeSheetRobot
{
    public class Model : INotifyPropertyChanged
    {
        private string _StartingNode;
        private ObservableCollection<TubeM> Tubes { get; set; }
        public string StartingNode
        {
            get
            {
                return _StartingNode;
            }
            set
            {
                _StartingNode = value;
                OnPropertyChanged("StartingNode");
            }
        }
        private string _EndNode;
        private TubeM StNode;
        private TubeM EnNode;
        public string EndNode
        {
            get
            {
                return _EndNode;
            }
            set
            {
                _EndNode = value;
                OnPropertyChanged("EndNode");
            }
        }
        private string _Path;
        public string Path
        {
            get
            {
                return _Path;
            }
            set
            {
                _Path = value;
                OnPropertyChanged("Path");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public Model()
        {
            StartingNode = "None";
            EndNode = "None";
            StNode = null;
            EnNode = null;
        }        

        public void SetCoordinates(TubeM node)
        {
            if (!(StNode != node && node.ColorProp != "Black"))
            {
                return;
            }
            if (StartingNode == "None")
            {
                StNode = node;
                StartingNode = node.ToolTip;
            }
            else
            {
                EnNode = node;
                EndNode = node.ToolTip;
                this.FindPath();
            }
        }
        private void FindPath()
        {
            TubeM currentPos = StNode;
            int row = currentPos.Row;
            int column = currentPos.Column;
            List<TubeM> pathList = new List<TubeM>();
            ResetColor();
            Pathfinder(currentPos, row, column, pathList);
            string pathlist = null;
            foreach (var item in pathList)
            {
                pathlist += item.ToolTip + " ";
            }
            pathlist.Trim();
            Path = pathlist;            
        }

        private void ResetColor()
        {
            var blueTubes = Tubes.Where(x => x.ColorProp == "Blue");
            foreach (var item in blueTubes)
            {
                item.ColorProp = "Gray";
            }
        }

        private void Pathfinder(TubeM currentPos, int row, int column, List<TubeM> pathList)
        {
            bool rS = false;
            bool cS = false;
            if ((EnNode.Row - currentPos.Row) > 0)
            {
                row += 1;
                rS = true;
            }
            else if ((EnNode.Row - currentPos.Row) < 0)
            {
                row -= 1;
                rS = true;
            }
            if ((EnNode.Column - currentPos.Column) > 0)
            {
                column += 1;
                cS = true;
            }
            else if ((EnNode.Column - currentPos.Column) < 0)
            {
                column -= 1;
                cS = true;
            }
            var currentPos1 = Tubes.Where(x => x.Row == row).Where(y => y.Column == column).Where(x => x.ColorProp != "Black").FirstOrDefault();
            if (currentPos1 == null)
            {
                if (rS && cS)
                {
                    currentPos1 = Tubes.Where(x => x.Row == currentPos.Row).Where(y => y.Column == column).Where(x => x.ColorProp != "Black").FirstOrDefault();
                    if (currentPos1 != null)
                    {
                        currentPos = currentPos1;
                        row = currentPos.Row;
                    }
                    else
                    {
                        currentPos = Tubes.Where(x => x.Row == row).Where(y => y.Column == currentPos.Column).Where(x => x.ColorProp != "Black").FirstOrDefault();
                        column = currentPos.Column;
                    }
                }
                //additional elses must be added
            }
            else { currentPos = currentPos1; }
            pathList.Add(currentPos);
            if (currentPos != EnNode)
            {
                if (currentPos.ColorProp != "Red")
                {
                    currentPos.ColorProp = "Blue";
                }
                Pathfinder(currentPos, row, column, pathList);
            }
        }

        public void ResetCoordinates()
        {
            StartingNode = "None";
            EndNode = "None";
            StNode = null;
            EnNode = null;
            Path = "";
            ResetColor();
        }
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        internal void CollectAllTubes(ObservableCollection<TubeM> tubes)
        {
            this.Tubes = tubes;
        }

        internal void ConnectTubesheet(TubesheetModel tubesheetModel)
        {
            CollectAllTubes(tubesheetModel.Tubes);
        }
    }
}
